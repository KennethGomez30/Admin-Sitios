using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Sistema_Contable.Pages
{
    public class IndexModel : PageModel
    {
        public string? UsuarioNombre { get; set; }

        public void OnGet()
        {
            // El filtro AutenticacionFilter ya validó que hay sesión
            // Obtener nombre del usuario
            UsuarioNombre = HttpContext.Session.GetString("UsuarioNombre");
        }
    }
}