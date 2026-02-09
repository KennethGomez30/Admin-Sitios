using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Sistema_Contable.Entities;
using Sistema_Contable.Services;

namespace Sistema_Contable.Pages.PeriodosContable
{
	public class PeriodoContableEditarModel : PageModel
	{
		private readonly IPeriodoContableService _service;

		public PeriodosContables? Periodo { get; set; }

		[BindProperty] public int PeriodoId { get; set; }
		[BindProperty] public int Anio { get; set; }
		[BindProperty] public int Mes { get; set; }
		[BindProperty] public string? UsuarioCierre { get; set; }
		[BindProperty] public DateTime? FechaCierre { get; set; }

		public List<SelectListItem> Anios { get; private set; } = new();
		public List<SelectListItem> Meses { get; private set; } = new();
		public bool EsCerrado { get; set; }
		public PeriodoContableEditarModel(IPeriodoContableService service)
		{
			_service = service;
		}

		public async Task<IActionResult> OnGetAsync(int id)
		{
			Periodo = await _service.ObtenerAsync(id);
			if (Periodo == null)
			{
				TempData["Error"] = "El período no existe.";
				return RedirectToPage("./PeriodoContableAdmin");
			}

			PeriodoId = Periodo.PeriodoId;
			Anio = Periodo.Anio;
			Mes = Periodo.Mes;

			var estado = (Periodo.Estado ?? "").Trim();
			EsCerrado = estado.Equals("Cerrado", StringComparison.OrdinalIgnoreCase);

			UsuarioCierre = Periodo.UsuarioCierre;
			FechaCierre = Periodo.FechaCierre;

			CargarCombos();
			return Page();
		}

		public async Task<IActionResult> OnPostAsync()
		{
			CargarCombos();

			var (ok, msg) = await _service.EditarAsync(PeriodoId, Anio, Mes, UsuarioCierre, FechaCierre);
			TempData[ok ? "Success" : "Error"] = msg;

			if (!ok) return Page();
			return RedirectToPage("./PeriodoContableAdmin");
		}

		private void CargarCombos()
		{
			var year = DateTime.Now.Year;

			Anios = Enumerable.Range(year - 10, 21)
				.Select(y => new SelectListItem
				{
					Value = y.ToString(),
					Text = y.ToString(),
					Selected = (y == Anio)
				}).ToList();

			Meses = Enumerable.Range(1, 12)
				.Select(m => new SelectListItem
				{
					Value = m.ToString(),
					Text = m.ToString("D2"),
					Selected = (m == Mes)
				}).ToList();
		}
	}
}