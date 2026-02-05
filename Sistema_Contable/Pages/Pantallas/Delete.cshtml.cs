using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sistema_Contable.Entities;
using Sistema_Contable.Services;

namespace Sistema_Contable.Pages.Pantallas
{
    public class DeleteModel : PageModel
    {
        private readonly IPantallaService _pantallaService;

        public DeleteModel(IPantallaService pantallaService)
        {
            _pantallaService = pantallaService;
        }

        [BindProperty(SupportsGet = true)]
        public ulong id { get; set; }

        public Pantalla? Pantalla { get; set; }

        [TempData] public string? ErrorMessage { get; set; }
        [TempData] public string? SuccessMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var usuario = HttpContext.Session.GetString("UsuarioId");

            if (string.IsNullOrWhiteSpace(usuario))
                return RedirectToPage("/Login");

            if (id == 0)
            {
                ErrorMessage = "Registro no válido.";
                return RedirectToPage("Index");
            }

            Pantalla = await _pantallaService.ObtenerPorIdAsync(id, usuario);

            if (Pantalla == null)
            {
                ErrorMessage = "Registro no encontrado.";
                return RedirectToPage("Index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var usuario = HttpContext.Session.GetString("UsuarioId");
            if (string.IsNullOrWhiteSpace(usuario))
                return RedirectToPage("/Login");

            if (id == 0)
            {
                ErrorMessage = "Registro no válido.";
                return RedirectToPage("Index");
            }

            var (ok, error) = await _pantallaService.EliminarAsync(id, usuario);

            if (!ok)
            {
                ErrorMessage = error ?? "No se puede eliminar una panatalla con datos relacionados.";
                Pantalla = await _pantallaService.ObtenerPorIdAsync(id, usuario);
                return Page();
            }

            SuccessMessage = "Registro eliminado con éxito.";
            return RedirectToPage("Index");
        }
    }
}
