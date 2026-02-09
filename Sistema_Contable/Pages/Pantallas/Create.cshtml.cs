using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sistema_Contable.Entities;
using Sistema_Contable.Services;

namespace Sistema_Contable.Pages.Pantallas
{
    public class CreateModel : PageModel
    {
        private readonly IPantallaService _pantallaService;

        public CreateModel(IPantallaService pantallaService)
        {
            _pantallaService = pantallaService;
        }

        [BindProperty] public Pantalla Pantalla { get; set; } = new();
        [TempData] public string? ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            var usuario = HttpContext.Session.GetString("UsuarioId");
            if (string.IsNullOrWhiteSpace(usuario))
                return RedirigirLogin("Debes iniciar sesión para acceder al sistema.");

            Pantalla.estado = "Activa";
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var usuario = HttpContext.Session.GetString("UsuarioId");
            if (string.IsNullOrWhiteSpace(usuario))
                return RedirigirLogin("Debes iniciar sesión para acceder al sistema.");

            if (!ModelState.IsValid) return Page();

            var (ok, error, _) = await _pantallaService.CrearAsync(Pantalla, usuario);

            if (!ok)
            {
                ErrorMessage = error ?? "No se pudo guardar.";
                return Page();
            }

            TempData["SuccessMessage"] = "Registro guardado con éxito.";
            return RedirectToPage("Index");
        }

        private IActionResult RedirigirLogin(string msg)
        {
            HttpContext.Session.SetString("MensajeRedireccion", msg);
            return RedirectToPage("/Login");
        }
    }
}