using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sistema_Contable.Entities;
using Sistema_Contable.Services;

namespace Sistema_Contable.Pages.PeriodosContable
{
    public class PeriodoContableCerrarModel : PageModel
    {
		private readonly IPeriodoContableService _service;

		public PeriodosContables? Periodo { get; set; }

		[BindProperty] public string UsuarioCierre { get; set; } = "";

		public PeriodoContableCerrarModel(IPeriodoContableService service)
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

			if (Periodo.Estado.Equals("Cerrado", StringComparison.OrdinalIgnoreCase))
			{
				TempData["Error"] = "El período ya está cerrado.";
				return RedirectToPage("./PeriodoContableAdmin");
			}

			 UsuarioCierre = HttpContext.Session.GetString("UsuarioNombre") ?? "";

			return Page();
		}

		public async Task<IActionResult> OnPostAsync(int id)
		{
			if (string.IsNullOrWhiteSpace(UsuarioCierre))
			{
				TempData["Error"] = "Debe indicar el usuario de cierre.";
				return Page();
			}

			var (ok, msg) = await _service.CerrarAsync(id, UsuarioCierre.Trim());

			TempData[ok ? "Success" : "Error"] = msg;
			return RedirectToPage("./PeriodoContableAdmin");
		}
	}
}
