using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sistema_Contable.Entities;
using Sistema_Contable.Services;

namespace Sistema_Contable.Pages.Pantallas
{
    public class IndexModel : PageModel
    {
        private readonly IPantallaService _pantallaService;

        public IndexModel(IPantallaService pantallaService)
        {
            _pantallaService = pantallaService;
        }

        public List<Pantalla> Pantallas { get; set; } = new();

        [BindProperty(SupportsGet = true)] public string? q { get; set; }
        [BindProperty(SupportsGet = true)] public int page { get; set; } = 1;

        public int PageSize { get; } = 10;
        public int Total { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)Total / PageSize);

        [TempData] public string? SuccessMessage { get; set; }
        [TempData] public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var usuario = HttpContext.Session.GetString("UsuarioId");
            if (string.IsNullOrWhiteSpace(usuario))
                return RedirigirLogin("Debes iniciar sesión para acceder al sistema.");

            if (page < 1) page = 1;

            var (data, total) = await _pantallaService.ObtenerPaginadoAsync(page, PageSize, q, usuario);
            Pantallas = data.ToList();
            Total = total;

            if (TotalPages > 0 && page > TotalPages)
                return RedirectToPage(new { page = TotalPages, q });

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(ulong id)
        {
            var usuario = HttpContext.Session.GetString("UsuarioId");
            if (string.IsNullOrWhiteSpace(usuario))
                return RedirigirLogin("Debes iniciar sesión para acceder al sistema.");

            if (id == 0)
            {
                ErrorMessage = "Registro no válido.";
                return RedirectToPage(new { page, q });
            }

            var (ok, error) = await _pantallaService.EliminarAsync(id, usuario);

            if (ok)
                SuccessMessage = "Registro eliminado con éxito.";
            else
                ErrorMessage = error ?? "No se pudo eliminar.";

            return RedirectToPage(new { page, q });
        }


        private IActionResult RedirigirLogin(string msg)
        {
            HttpContext.Session.SetString("MensajeRedireccion", msg);
            return RedirectToPage("/Login");
        }
    }
}


