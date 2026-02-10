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
		public bool MostrarModal { get; set; }
		public string? MensajeModal { get; set; }
		public string? CodigoModal { get; set; }
		public string? NombreModal { get; set; }
		private string UsuarioActual =>HttpContext.Session.GetString("UsuarioNombre")?? HttpContext.Session.GetString("UsuarioId")?? "Sistema";
		public EstadoAsientoModel(IEstadosAsientoService service)
		{
			_service = service;
		}

		public async Task OnGetAsync()
		{
			Estados = await _service.ListarAsync(UsuarioActual);
		}
		public async Task<IActionResult> OnPostEliminarAsync(string codigo)
		{
			var (ok, msg) = await _service.EliminarAsync(codigo, UsuarioActual);

			if (ok)
			{
				TempData["Success"] = msg;
				return RedirectToPage();
			}

			TempData["Error"] = msg;

			Estados = await _service.ListarAsync(UsuarioActual);

			MostrarModal = true;
			MensajeModal = msg;
			CodigoModal = codigo;

			var e = Estados.FirstOrDefault(x => x.Codigo == codigo);
			NombreModal = e?.Nombre ?? codigo;

			return Page();
		}

	}
}