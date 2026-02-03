using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Sistema_Contable.Pages
{
    public class LogoutModel : PageModel
    {
        public IActionResult OnGet()
        {
            // Limpiar sesión
            HttpContext.Session.Clear();

            // Redirigir al login
            return RedirectToPage("/Login");
        }
    }
}
