using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sistema_Contable.Entities;
using Sistema_Contable.Services;

namespace Sistema_Contable.Pages.EstadosAsientos
{
    public class EstadoAsientosEliminarModel : PageModel
    {
		private readonly IEstadosAsientoService _service;

		public EstadosAsiento? Estado { get; set; }

		public EstadoAsientosEliminarModel(IEstadosAsientoService service)
		{
			_service = service;
		}

		public async Task<IActionResult> OnGetAsync(string codigo)
		{
			Estado = await _service.ObtenerAsync(codigo);

			if (Estado == null)
			{
				TempData["Error"] = "Registro no encontrado.";
				return RedirectToPage("./EstadoAsientos");
			}

			return Page();
		}

		public async Task<IActionResult> OnPostAsync(string codigo)
		{
			var (ok, msg) = await _service.EliminarAsync(codigo);

			if (!ok)
			{
				TempData["Error"] = msg; 
				return RedirectToPage("./EstadoAsientos");
			}

			TempData["Success"] = msg;
			return RedirectToPage("./EstadoAsientos");
		}
	}
}
