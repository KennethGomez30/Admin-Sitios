using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sistema_Contable.Entities;
using Sistema_Contable.Repository;
using Sistema_Contable.Services;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sistema_Contable.Pages.Usuarios
{
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

        public UsuarioConRoles? Usuario { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Debe ingresar una contraseña")]
        public string NuevaContrasena { get; set; } = string.Empty;

        // Variables locales para mensajes (NO persisten)
        public string? MensajeError { get; set; }
        public string? MensajeExito { get; set; }

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

                // Validar que el campo no esté vacío - Todos los campos son requeridos
                if (string.IsNullOrWhiteSpace(NuevaContrasena))
                {
                    ModelState.AddModelError("NuevaContrasena", "Debe ingresar una contraseña.");
                    MensajeError = "Debe ingresar una contraseña.";
                    return Page();
                }

                var resultadoValidacion = ValidarContrasena(NuevaContrasena);

                if (!resultadoValidacion.EsValida)
                {
                    ModelState.AddModelError("NuevaContrasena", resultadoValidacion.MensajeError);
                    MensajeError = resultadoValidacion.MensajeError;
                    return Page();
                }

                // Encriptar contraseña con MD5
                var contrasenaEncriptada = ContrasennaService.EncriptarMD5(NuevaContrasena);

                // Actualizar contraseña en la base de datos
                await _usuarioRepository.ActualizarContrasenaAsync(id, contrasenaEncriptada);

                // Registrar en bitácora - Se debe respetar lo indicado en el manejo de bitácoras
                var jsonCambio = JsonSerializer.Serialize(new
                {
                    Usuario = id,
                    Accion = "Cambio de contraseña",
                    Fecha = DateTime.Now
                });

                await RegistrarBitacoraAsync(usuarioActual, $"Actualiza contraseña de usuario | {jsonCambio}");

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

        public async Task<IActionResult> OnPostGenerarContrasenaAsync()
        {
            var contrasenaGenerada = ContrasennaService.GenerarContrasena();
            return new JsonResult(new { contrasena = contrasenaGenerada });
        }

        private (bool EsValida, string MensajeError) ValidarContrasena(string contrasena)
        {
            if (contrasena.Length < 8)
            {
                return (false, "La contraseña debe tener al menos 8 caracteres.");
            }

            if (!char.IsLetter(contrasena[0]))
            {
                return (false, "La contraseña debe iniciar con una letra.");
            }

            if (!Regex.IsMatch(contrasena, @"[a-zA-Z]"))
            {
                return (false, "La contraseña debe contener al menos una letra.");
            }

            if (!Regex.IsMatch(contrasena, @"[0-9]"))
            {
                return (false, "La contraseña debe contener al menos un número.");
            }

            if (!Regex.IsMatch(contrasena, @"[\+\-\*\$\.]"))
            {
                return (false, "La contraseña debe contener al menos un símbolo (+ - * $ .)");
            }

            if (!Regex.IsMatch(contrasena, @"^[a-zA-Z0-9\+\-\*\$\.]+$"))
            {
                return (false, "La contraseña solo puede contener letras, números y los símbolos: + - * $ .");
            }

            return (true, string.Empty);
        }

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