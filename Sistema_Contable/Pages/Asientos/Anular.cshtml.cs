using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sistema_Contable.Entities;
using Sistema_Contable.Services;

namespace Sistema_Contable.Pages.Asientos
{
    public class AnularModel : PageModel
    {
        private readonly IAsientoService _asientoService;

        public AnularModel(IAsientoService asientoService)
        {
            _asientoService = asientoService;
        }

        public Asiento? Asiento { get; set; }
        public IEnumerable<AsientoDetalle> Detalles { get; set; } = Enumerable.Empty<AsientoDetalle>();

        [BindProperty]
        public long AsientoId { get; set; }

        public decimal TotalDebito { get; set; }
        public decimal TotalCredito { get; set; }
        public bool Balanceado => TotalDebito == TotalCredito;

        public bool PuedeAnular { get; set; } = false;

        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(long id)
        {
            try
            {
                var result = await _asientoService.ObtenerAsientoAsync(id);
                Asiento = result.Encabezado;
                Detalles = result.Detalle ?? Enumerable.Empty<AsientoDetalle>();

                if (Asiento != null)
                {
                    AsientoId = Asiento.AsientoId;
                    TotalDebito = Asiento.TotalDebito;
                    TotalCredito = Asiento.TotalCredito;

                    // Permitir anular salvo que ya esté anulado
                    PuedeAnular = Asiento.EstadoCodigo != "Anulado";
                }

                return Page();
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = ex.Message;
                return RedirectToPage("/Error");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (AsientoId <= 0)
            {
                ErrorMessage = "Asiento inválido.";
                return Page();
            }

            try
            {
                var usuario = User?.Identity?.Name ?? "system";

                await _asientoService.AnularAsientoAsync(AsientoId, usuario);

                TempData["MensajeError"] = $"Asiento {AsientoId} anulado correctamente.";
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                // El SP puede lanzar SIGNAL con mensajes; los mostramos amigablemente
                TempData["MensajeError"] = ex.Message;
                return RedirectToPage("/Error");
            }
        }
    }
}
