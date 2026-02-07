using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sistema_Contable.Entities;
using Sistema_Contable.Services;

namespace Sistema_Contable.Pages.PeriodosContable
{
    public class PeriodoContableAdminModel : PageModel
    {
		private readonly IPeriodoContableService _service;

		public IEnumerable<PeriodosContables> Periodos { get; set; } = [];

		[BindProperty(SupportsGet = true)]
		public string? Estado { get; set; } 

		public PeriodoContableAdminModel(IPeriodoContableService service)
		{
			_service = service;
		}

		public async Task OnGetAsync()
		{
			var filtro = string.IsNullOrWhiteSpace(Estado) ? null : Estado.Trim();
			Periodos = await _service.ListarAsync(filtro);
		}
	}
}
