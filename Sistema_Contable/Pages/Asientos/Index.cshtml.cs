using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sistema_Contable.Entities;
using Sistema_Contable.Services;

namespace Sistema_Contable.Pages.Asientos
{
    public class IndexModel : PageModel
    {
        private readonly IAsientoService _asientoService;

        public IndexModel(IAsientoService asientoService)
        {
            _asientoService = asientoService;
        }

        public IEnumerable<Asiento> Asientos { get; set; } = new List<Asiento>();

        [BindProperty(SupportsGet = true)]
        public string? EstadoFiltro { get; set; }

        public List<string> Estados { get; } = new()
        {
            "Borrador",
            "Pendiente de aprobación",
            "Aprobado",
            "Pendiente de aprobación",
            "Rechazado",
            "Anulado"
        };

        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                Asientos = await _asientoService.ListarAsientosAsync(
                    periodoId: null,
                    estadoCodigo: EstadoFiltro
                );

                return Page();
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = ex.Message;
                return RedirectToPage("/Error");
            }
        }
    }
}
