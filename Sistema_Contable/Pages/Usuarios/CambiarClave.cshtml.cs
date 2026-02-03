using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sistema_Contable.Entities;
using Sistema_Contable.Repository;
using Sistema_Contable.Services;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Threading.Tasks;

namespace Sistema_Contable.Pages.Usuarios
{
    /// <summary>
    /// Página para cambiar la contraseña de usuarios - ADM8
    /// </summary>
    public class CambiarClaveModel : PageModel
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IBitacoraRepository _bitacoraRepository;

        public CambiarClaveModel(
            IUsuarioRepository usuarioRepository,
            IBitacoraRepository bitacoraRepository)
        {
            _usuarioRepository = usuarioRepository;
            _bitacoraRepository = bitacoraRepository;
        }

        /// <summary>
        /// Usuario al que se le cambiará la contraseña
        /// El funcionario será el que se seleccionó en el listado de funcionarios (HU ADM7)
        /// </summary>
        public UsuarioConRoles? Usuario { get; set; }

        /// <summary>
        /// Nueva contraseña a asignar al usuario
        /// Todos los campos son requeridos - ADM8
        /// </summary>
        [BindProperty]
        [Required(ErrorMessage = "Debe ingresar una contraseña")]
        public string NuevaContrasena { get; set; } = string.Empty;

        [TempData]
        public string? MensajeError { get; set; }

        [TempData]
        public string? MensajeExito { get; set; }

        /// <summary>
        /// A esta opción se ingresa desde la pantalla "Administración de usuarios" descrita en la HU ADM7
        /// El funcionario será el que se seleccionó en el listado de funcionarios (HU ADM7)
        /// </summary>
        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToPage("./Index");
            }

            Usuario = await _usuarioRepository.ObtenerConRolesPorIdAsync(id);

            if (Usuario == null)
            {
                TempData["MensajeError"] = "Usuario no encontrado.";
                return RedirectToPage("./Index");
            }

            return Page();
        }

        /// <summary>
        /// Botón "Aceptar" que realizará el cambio de clave al usuario - ADM8
        /// Se debe respetar lo indicado en el manejo de bitácoras
        /// Se deben aplicar las opciones que apliquen de la sección flujo de pantallas
        /// </summary>
        public async Task<IActionResult> OnPostAsync(string id)
        {
            var usuarioActual = HttpContext.Session.GetString("UsuarioId");

            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return RedirectToPage("./Index");
                }

                // Recargar usuario
                Usuario = await _usuarioRepository.ObtenerConRolesPorIdAsync(id);

                if (Usuario == null)
                {
                    TempData["MensajeError"] = "Usuario no encontrado.";
                    return RedirectToPage("./Index");
                }

                // Validar modelo - Todos los campos son requeridos
                if (!ModelState.IsValid)
                {
                    return Page();
                }

                // Validación adicional: campo requerido
                if (string.IsNullOrWhiteSpace(NuevaContrasena))
                {
                    MensajeError = "Debe ingresar una contraseña.";
                    return Page();
                }

                // Encriptar contraseña con MD5 (ADM1 - La contraseña estará encriptada en la BD)
                var contrasenaEncriptada = ContrasennaService.EncriptarMD5(NuevaContrasena);

                // Actualizar contraseña en la base de datos
                await _usuarioRepository.ActualizarContrasenaAsync(id, contrasenaEncriptada);

                // Registrar en bitácora - Se debe respetar lo indicado en el manejo de bitácoras
                var jsonCambio = JsonSerializer.Serialize(new
                {
                    Usuario = id,
                    Accion = "Cambio de contrasenna",
                    Fecha = DateTime.Now
                });

                await RegistrarBitacoraAsync(usuarioActual, $"Actualiza contraseña de usuario | {jsonCambio}");

                // Mensaje de éxito y redirigir al listado
                // Flujo de pantallas: "Al usar el botón de guardar, si la operación es exitosa, 
                // debe regresar al listado y mostrar un mensaje de éxito"
                TempData["MensajeExito"] = "Contraseña actualizada exitosamente.";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                await RegistrarBitacoraAsync(usuarioActual, $"Error al cambiar contraseña: {ex.Message}");
                MensajeError = "Ocurrió un error al cambiar la contraseña.";
                return Page();
            }
        }

        /// <summary>
        /// Botón "Autogenerar" que creará una contraseña para el usuario de manera automática - ADM8
        /// Esta clave debe tener letras, números y símbolos (+-*$.). Inicia con una letra
        /// </summary>
        public async Task<IActionResult> OnPostGenerarContrasenaAsync()
        {
            var contrasenaGenerada = ContrasennaService.GenerarContrasena();
            return new JsonResult(new { contrasena = contrasenaGenerada });
        }

        /// <summary>
        /// Registra acciones en bitácora según especificación del documento
        /// </summary>
        private async Task RegistrarBitacoraAsync(string? usuario, string accion)
        {
            var bitacora = new Bitacora
            {
                FechaBitacora = DateTime.Now,
                Usuario = usuario,
                Descripcion = accion
            };

            await _bitacoraRepository.RegistrarAsync(bitacora);
        }
    }
}