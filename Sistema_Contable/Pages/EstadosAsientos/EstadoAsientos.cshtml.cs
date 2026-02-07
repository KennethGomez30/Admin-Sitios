using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sistema_Contable.Entities;
using Sistema_Contable.Services;

namespace Sistema_Contable.Pages.EstadosAsientos
{
	public class EstadoAsientoModel : PageModel
	{
		private readonly IEstadosAsientoService _service;

		public IEnumerable<EstadosAsiento> Estados { get; set; } = [];

		public EstadoAsientoModel(IEstadosAsientoService service)
		{
			_service = service;
		}

		public async Task OnGetAsync()
		{
			Estados = await _service.ListarAsync();
		}
	}
}