using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sistema_Contable.Entities;
using Sistema_Contable.Services;

namespace Sistema_Contable.Pages.Pantallas
{
    public class EditModel : PageModel
    {
        private readonly IPantallaService _pantallaService;

        public EditModel(IPantallaService pantallaService)
        {
            _pantallaService = pantallaService;
        }

        [BindProperty] public Pantalla Pantalla { get; set; } = new();
        [TempData] public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(ulong id)
        {
            var usuario = HttpContext.Session.GetString("UsuarioId");
            if (string.IsNullOrWhiteSpace(usuario))
                return RedirigirLogin("Debes iniciar sesión para acceder al sistema.");

            var p = await _pantallaService.ObtenerPorIdAsync(id, usuario);
            if (p == null)
            {
                TempData["ErrorMessage"] = "Registro no encontrado.";
                return RedirectToPage("Index");
            }

            Pantalla = p;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var usuario = HttpContext.Session.GetString("UsuarioId");
            if (string.IsNullOrWhiteSpace(usuario))
                return RedirigirLogin("Debes iniciar sesión para acceder al sistema.");


            if (!ModelState.IsValid) return Page();

            var (ok, error) = await _pantallaService.ActualizarAsync(Pantalla, usuario);
            if (!ok)
            {
                ErrorMessage = error ?? "No se pudo actualizar.";
                return Page();
            }

            TempData["SuccessMessage"] = "Registro actualizado con éxito.";
            return RedirectToPage("Index");
        }

        private IActionResult RedirigirLogin(string msg)
        {
            HttpContext.Session.SetString("MensajeRedireccion", msg);
            return RedirectToPage("/Login");
        }
    }
}