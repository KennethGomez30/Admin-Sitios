using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sistema_Contable.Entities;
using Sistema_Contable.Services;

namespace Sistema_Contable.Pages.EstadosAsientos
{
	public class EstadoAsientosCrearModel : PageModel
	{
		private readonly IEstadosAsientoService _service;

		[BindProperty]
		public EstadosAsiento Estado { get; set; } = new();

		public EstadoAsientosCrearModel(IEstadosAsientoService service)
		{
			_service = service;
		}

		public void OnGet() { }

		public async Task<IActionResult> OnPostAsync()
		{
			var (ok, msg) = await _service.CrearAsync(Estado);

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
