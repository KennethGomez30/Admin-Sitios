using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sistema_Contable.Services;

namespace Sistema_Contable.Pages
{
    public class LoginModel : PageModel
    {
        private readonly IAutenticacionService _autenticacionService;

        public LoginModel(IAutenticacionService autenticacionService)
        {
            _autenticacionService = autenticacionService;
        }

        [BindProperty]
        public string Identificacion { get; set; } = string.Empty;

        [BindProperty]
        public string Contrasena { get; set; } = string.Empty;

        [TempData]
        public string? MensajeError { get; set; }

        // Quitar [TempData] de MensajeInfo - solo usar propiedad normal
        public string? MensajeInfo { get; set; }

        public void OnGet()
        {
            // Si ya existe sesión activa, redirigir a Index
            var usuarioIdActual = HttpContext.Session.GetString("UsuarioId");
            if (!string.IsNullOrEmpty(usuarioIdActual))
            {
                Response.Redirect("/Index");
                return;
            }

            // Leer mensaje ANTES de limpiar
            var mensajeRedireccion = HttpContext.Session.GetString("MensajeRedireccion");

            // Limpiar TODA la sesión
            HttpContext.Session.Clear();

            // Solo mostrar mensaje si existía
            if (!string.IsNullOrEmpty(mensajeRedireccion))
            {
                MensajeInfo = mensajeRedireccion;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var resultado = await _autenticacionService.AutenticarAsync(Identificacion, Contrasena);

            if (resultado.Exitoso)
            {
                // Iniciar sesión del usuario
                HttpContext.Session.SetString("UsuarioId", resultado.Usuario.Identificacion);
                HttpContext.Session.SetString("UsuarioNombre", resultado.Usuario.NombreCompleto);
                HttpContext.Session.SetString("UsuarioCorreo", resultado.Usuario.Correo);

                // Redirigir a la página de bienvenida (Index)
                return RedirectToPage("/Index");
            }

            // Mostrar mensaje de error
            MensajeError = resultado.Mensaje;
            return RedirectToPage();
        }
    }
}