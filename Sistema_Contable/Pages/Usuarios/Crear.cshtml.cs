using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sistema_Contable.Entities;
using Sistema_Contable.Repository;
using Sistema_Contable.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sistema_Contable.Pages.Usuarios
{
    public class CrearModel : PageModel
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IRolRepository _rolRepository;
        private readonly IBitacoraRepository _bitacoraRepository;

        public CrearModel(
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
        [Required(ErrorMessage = "La contraseña es requerida")]
        public string Contrasena { get; set; } = string.Empty;

        [BindProperty]
        public List<int> RolesSeleccionados { get; set; } = new();

        public List<Rol> RolesDisponibles { get; set; } = new();

        // Variable local para mensajes (NO persiste)
        public string? MensajeError { get; set; }

        public class UsuarioInput
        {
            [Required(ErrorMessage = "La identificación es requerida")]
            [StringLength(50, ErrorMessage = "La identificación no puede exceder 50 caracteres")]
            public string Identificacion { get; set; } = string.Empty;

            [Required(ErrorMessage = "El nombre es requerido")]
            [StringLength(50, ErrorMessage = "El nombre no puede exceder 50 caracteres")]
            [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "El nombre solo puede contener letras y espacios")]
            public string Nombre { get; set; } = string.Empty;

            [Required(ErrorMessage = "El apellido es requerido")]
            [StringLength(50, ErrorMessage = "El apellido no puede exceder 50 caracteres")]
            [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "El apellido solo puede contener letras y espacios")]
            public string Apellido { get; set; } = string.Empty;

            [Required(ErrorMessage = "El correo es requerido")]
            [EmailAddress(ErrorMessage = "El correo no es válido")]
            [StringLength(100, ErrorMessage = "El correo no puede exceder 100 caracteres")]
            public string Correo { get; set; } = string.Empty;

            [Required(ErrorMessage = "El estado es requerido")]
            public string Estado { get; set; } = "Activo";
        }

        public async Task OnGetAsync()
        {
            // Cargar roles disponibles
            RolesDisponibles = await _rolRepository.ObtenerTodosAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var usuarioActual = HttpContext.Session.GetString("UsuarioId");

            try
            {
                // Recargar roles para el formulario
                RolesDisponibles = await _rolRepository.ObtenerTodosAsync();

                // Validaciones
                if (!ModelState.IsValid)
                {
                    return Page();
                }

                if (string.IsNullOrWhiteSpace(Contrasena))
                {
                    MensajeError = "La contraseña es requerida. Debe ingresar una contraseña o autogenerar una.";
                    return Page();
                }

                // Validar formato de la contraseña
                if (!ValidarFormatoContrasena(Contrasena))
                {
                    MensajeError = "La contraseña no cumple con el formato requerido. Debe contener letras, números, símbolos (+-*$.) e iniciar con una letra.";
                    return Page();
                }

                if (RolesSeleccionados == null || !RolesSeleccionados.Any())
                {
                    MensajeError = "Debe seleccionar al menos un rol para el usuario.";
                    return Page();
                }

                // Verificar si ya existe
                if (await _usuarioRepository.ExisteAsync(Usuario.Identificacion))
                {
                    MensajeError = "Ya existe un usuario con esta identificación.";
                    return Page();
                }

                // Crear entidad Usuario
                var nuevoUsuario = new Usuario
                {
                    Identificacion = Usuario.Identificacion.Trim(),
                    Nombre = Usuario.Nombre.Trim(),
                    Apellido = Usuario.Apellido.Trim(),
                    Correo = Usuario.Correo.Trim(),
                    Contrasena = ContrasennaService.EncriptarMD5(Contrasena),
                    Estado = Usuario.Estado,
                    IntentosLogin = 0
                };

                // Guardar usuario con roles
                await _usuarioRepository.CrearAsync(nuevoUsuario, RolesSeleccionados);

                // Registrar en bitácora
                var rolesNombres = RolesDisponibles
                    .Where(r => RolesSeleccionados.Contains(r.IdRol))
                    .Select(r => r.Nombre)
                    .ToList();

                var jsonNuevo = JsonSerializer.Serialize(new
                {
                    nuevoUsuario.Identificacion,
                    nuevoUsuario.Nombre,
                    nuevoUsuario.Apellido,
                    nuevoUsuario.Correo,
                    nuevoUsuario.Estado,
                    Roles = rolesNombres
                });

                await RegistrarBitacoraAsync(usuarioActual, $"Crea nuevo usuario | Datos: {jsonNuevo}");

                // Redirigir con mensaje de éxito
                TempData["MensajeExito"] = "Usuario creado exitosamente.";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                await RegistrarBitacoraAsync(usuarioActual, $"Error al crear usuario: {ex.Message}");
                MensajeError = "Ocurrió un error al crear el usuario.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostGenerarContrasenaAsync()
        {
            var contrasenaGenerada = ContrasennaService.GenerarContrasena();
            return new JsonResult(new { contrasena = contrasenaGenerada });
        }

        private bool ValidarFormatoContrasena(string contrasena)
        {
            if (string.IsNullOrWhiteSpace(contrasena))
                return false;

            // Debe iniciar con una letra (mayúscula o minúscula)
            if (!Regex.IsMatch(contrasena, @"^[a-zA-Z]"))
                return false;

            // Debe contener al menos una letra
            if (!Regex.IsMatch(contrasena, @"[a-zA-Z]"))
                return false;

            // Debe contener al menos un número
            if (!Regex.IsMatch(contrasena, @"[0-9]"))
                return false;

            // Debe contener alguno de + - * $ .
            if (!Regex.IsMatch(contrasena, @"[\+\-\*\$\.]"))
                return false;

            // letras, números y  +-*$.
            if (!Regex.IsMatch(contrasena, @"^[a-zA-Z0-9\+\-\*\$\.]+$"))
                return false;

            return true;
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