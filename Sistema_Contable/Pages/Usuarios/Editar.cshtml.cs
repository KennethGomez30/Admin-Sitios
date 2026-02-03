using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sistema_Contable.Entities;
using Sistema_Contable.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Sistema_Contable.Pages.Usuarios
{
    public class EditarModel : PageModel
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IRolRepository _rolRepository;
        private readonly IBitacoraRepository _bitacoraRepository;

        public EditarModel(
            IUsuarioRepository usuarioRepository,
            IRolRepository rolRepository,
            IBitacoraRepository bitacoraRepository)
        {
            _usuarioRepository = usuarioRepository;
            _rolRepository = rolRepository;
            _bitacoraRepository = bitacoraRepository;
        }

        [BindProperty]
        public UsuarioInput Usuario { get; set; } = new();

        [BindProperty]
        public List<int> RolesSeleccionados { get; set; } = new();

        public List<Rol> RolesDisponibles { get; set; } = new();
        public UsuarioConRoles? UsuarioOriginal { get; set; }

        [TempData]
        public string? MensajeError { get; set; }

        public class UsuarioInput
        {
            public string Identificacion { get; set; } = string.Empty;

            [Required(ErrorMessage = "El nombre es requerido")]
            [StringLength(50, ErrorMessage = "El nombre no puede exceder 50 caracteres")]
            [RegularExpression(@"^[a-zA-Z·ÈÌÛ˙¡…Õ”⁄Ò—\s]+$", ErrorMessage = "El nombre solo puede contener letras y espacios")]
            public string Nombre { get; set; } = string.Empty;

            [Required(ErrorMessage = "El apellido es requerido")]
            [StringLength(50, ErrorMessage = "El apellido no puede exceder 50 caracteres")]
            [RegularExpression(@"^[a-zA-Z·ÈÌÛ˙¡…Õ”⁄Ò—\s]+$", ErrorMessage = "El apellido solo puede contener letras y espacios")]
            public string Apellido { get; set; } = string.Empty;

            [Required(ErrorMessage = "El correo es requerido")]
            [EmailAddress(ErrorMessage = "El correo no es v·lido")]
            [StringLength(100, ErrorMessage = "El correo no puede exceder 100 caracteres")]
            public string Correo { get; set; } = string.Empty;

            [Required(ErrorMessage = "El estado es requerido")]
            public string Estado { get; set; } = "Activo";
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToPage("./Index");
            }

            // Obtener usuario con roles
            UsuarioOriginal = await _usuarioRepository.ObtenerConRolesPorIdAsync(id);

            if (UsuarioOriginal == null)
            {
                TempData["MensajeError"] = "Usuario no encontrado.";
                return RedirectToPage("./Index");
            }

            // Cargar datos al modelo
            Usuario = new UsuarioInput
            {
                Identificacion = UsuarioOriginal.Identificacion,
                Nombre = UsuarioOriginal.Nombre,
                Apellido = UsuarioOriginal.Apellido,
                Correo = UsuarioOriginal.Correo,
                Estado = UsuarioOriginal.Estado
            };

            // Cargar roles
            RolesDisponibles = await _rolRepository.ObtenerTodosAsync();
            RolesSeleccionados = UsuarioOriginal.Roles.Select(r => r.IdRol).ToList();

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

                // Recargar datos originales
                UsuarioOriginal = await _usuarioRepository.ObtenerConRolesPorIdAsync(id);
                if (UsuarioOriginal == null)
                {
                    TempData["MensajeError"] = "Usuario no encontrado.";
                    return RedirectToPage("./Index");
                }

                // Recargar roles para el formulario
                RolesDisponibles = await _rolRepository.ObtenerTodosAsync();

                // Validaciones
                if (!ModelState.IsValid)
                {
                    return Page();
                }

                if (RolesSeleccionados == null || !RolesSeleccionados.Any())
                {
                    MensajeError = "Debe seleccionar al menos un rol para el usuario.";
                    return Page();
                }

                // Crear entidad Usuario actualizada
                var usuarioActualizado = new Usuario
                {
                    Identificacion = id,
                    Nombre = Usuario.Nombre.Trim(),
                    Apellido = Usuario.Apellido.Trim(),
                    Correo = Usuario.Correo.Trim(),
                    Estado = Usuario.Estado,
                    Contrasena = string.Empty // No se modifica la contraseÒa aquÌ
                };

                // Actualizar usuario con roles
                await _usuarioRepository.ActualizarAsync(usuarioActualizado, RolesSeleccionados);

                // Registrar en bit·cora (datos anteriores vs nuevos)
                var rolesNombresAnteriores = UsuarioOriginal.Roles.Select(r => r.Nombre).ToList();
                var rolesNombresNuevos = RolesDisponibles
                    .Where(r => RolesSeleccionados.Contains(r.IdRol))
                    .Select(r => r.Nombre)
                    .ToList();

                var jsonAnterior = JsonSerializer.Serialize(new
                {
                    UsuarioOriginal.Identificacion,
                    UsuarioOriginal.Nombre,
                    UsuarioOriginal.Apellido,
                    UsuarioOriginal.Correo,
                    UsuarioOriginal.Estado,
                    Roles = rolesNombresAnteriores
                });

                var jsonNuevo = JsonSerializer.Serialize(new
                {
                    usuarioActualizado.Identificacion,
                    usuarioActualizado.Nombre,
                    usuarioActualizado.Apellido,
                    usuarioActualizado.Correo,
                    usuarioActualizado.Estado,
                    Roles = rolesNombresNuevos
                });

                await RegistrarBitacoraAsync(usuarioActual,
                    $"Actualiza usuario | Anterior: {jsonAnterior} | Nuevo: {jsonNuevo}");

                // Redirigir con mensaje de Èxito
                TempData["MensajeExito"] = "Usuario actualizado exitosamente.";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                await RegistrarBitacoraAsync(usuarioActual, $"Error al actualizar usuario: {ex.Message}");
                MensajeError = "OcurriÛ un error al actualizar el usuario.";
                return Page();
            }
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