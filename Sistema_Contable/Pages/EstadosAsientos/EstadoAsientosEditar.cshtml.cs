using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sistema_Contable.Entities;
using Sistema_Contable.Services;

namespace Sistema_Contable.Pages.EstadosAsientos
{
	public class EstadoAsientosEditarModel : PageModel
	{
		private readonly IEstadosAsientoService _service;

		[BindProperty]
		public EstadosAsiento Estado { get; set; } = new();

		public EstadoAsientosEditarModel(IEstadosAsientoService service)
		{
			_service = service;
		}

		public async Task<IActionResult> OnGetAsync(string codigo)
		{
			var data = await _service.ObtenerAsync(codigo);

			if (data == null)
			{
				TempData["Error"] = "Registro no encontrado.";
				return RedirectToPage("./EstadoAsientos");
			}

			Estado = data;
			return Page();
		}

		public async Task<IActionResult> OnPostAsync()
		{
			var (ok, msg) = await _service.EditarAsync(Estado);

			if (!ok)
			{
				TempData["Error"] = msg; 
				return Page();
			}

			TempData["Success"] = msg;
			return RedirectToPage("./EstadoAsientos");
		}
	}
}
