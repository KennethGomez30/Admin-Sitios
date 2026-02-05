using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Sistema_Contable.Pages
{
    public class IndexModel : PageModel
    {
        public string? UsuarioNombre { get; set; }

        public IActionResult OnGet()
        {
            // El filtro AutenticacionFilter ya validó que hay sesión
            // Pero por seguridad adicional, verificamos nuevamente
            var usuarioId = HttpContext.Session.GetString("UsuarioId");

            if (string.IsNullOrEmpty(usuarioId))
            {
                // Si por alguna razón no hay sesión, redirigir a Login
                HttpContext.Session.SetString("MensajeRedireccion",
                    "Por favor inicie sesión para utilizar el sistema");
                return RedirectToPage("/Login");
            }

            // Obtener nombre del usuario de la sesión
            UsuarioNombre = HttpContext.Session.GetString("UsuarioNombre");

            return Page();
        }
    }
}