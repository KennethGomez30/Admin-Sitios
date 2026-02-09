using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sistema_Contable.Entities;
using Sistema_Contable.Services;

namespace Sistema_Contable.Pages.PeriodosContable
{
    public class PeriodoContableReabrirModel : PageModel
    {
		private readonly IPeriodoContableService _service;

		public PeriodosContables? Periodo { get; set; }

		public PeriodoContableReabrirModel(IPeriodoContableService service)
		{
			_service = service;
		}

		public async Task<IActionResult> OnGetAsync(int id)
		{
			Periodo = await _service.ObtenerAsync(id);

			if (Periodo == null)
			{
				TempData["Error"] = "Registro no encontrado.";
				return RedirectToPage("./PeriodoContableAdmin");
			}

			if (Periodo.Estado.Equals("Abierto", StringComparison.OrdinalIgnoreCase))
			{
				TempData["Error"] = "El período ya está abierto.";
				return RedirectToPage("./PeriodoContableAdmin");
			}

			return Page();
		}

		public async Task<IActionResult> OnPostAsync(int id)
		{
			var (ok, msg) = await _service.ReabrirAsync(id);

			TempData[ok ? "Success" : "Error"] = msg;
			return RedirectToPage("./PeriodoContableAdmin");
		}
	}
}
