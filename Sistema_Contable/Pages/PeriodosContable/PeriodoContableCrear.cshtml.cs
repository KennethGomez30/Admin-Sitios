using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Sistema_Contable.Services;

namespace Sistema_Contable.Pages.PeriodosContable
{
	public class PeriodosContableCrearModel : PageModel
	{
		private readonly IPeriodoContableService _service;

		[BindProperty] public int Anio { get; set; }
		[BindProperty] public int Mes { get; set; }

		public List<SelectListItem> Anios { get; private set; } = new();
		public List<SelectListItem> Meses { get; private set; } = new();
		private string UsuarioActual => HttpContext.Session.GetString("UsuarioNombre") ?? HttpContext.Session.GetString("UsuarioId") ?? "Sistema";
		public PeriodosContableCrearModel(IPeriodoContableService service)
		{
			_service = service;
		}

		public void OnGet()
		{
			CargarCombos();
		}

		public async Task<IActionResult> OnPostAsync()
		{
			CargarCombos();

			var (ok, msg) = await _service.CrearAsync(Anio, Mes, UsuarioActual);
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
					Selected = (Anio == 0 ? y == year : y == Anio)
				})
				.ToList();

			Meses = Enumerable.Range(1, 12)
				.Select(m => new SelectListItem
				{
					Value = m.ToString(),
					Text = m.ToString("D2"),
					Selected = (Mes == 0 ? m == DateTime.Now.Month : m == Mes)
				})
				.ToList();

			if (Anio == 0) Anio = year;
			if (Mes == 0) Mes = DateTime.Now.Month;
		}
	}
}