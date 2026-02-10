using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sistema_Contable.Entities;
using Sistema_Contable.Services;

namespace Sistema_Contable.Pages.EstadosAsientos
{
    public class CambiarEstadoAsientosModel : PageModel
    {
		private readonly ICambiarEstadoAsientoService _service;

		public CambiarEstadoAsientosModel(ICambiarEstadoAsientoService service)
		{
			_service = service;
		}

		[BindProperty(SupportsGet = true)]
		public string? Estado { get; set; }

		public IEnumerable<string> EstadosDisponibles { get; set; } = Enumerable.Empty<string>();
		public IEnumerable<CambiarEstadoAsiento> Asientos { get; set; } = Enumerable.Empty<CambiarEstadoAsiento>();
		private string UsuarioActual =>HttpContext.Session.GetString("UsuarioNombre")?? HttpContext.Session.GetString("UsuarioId")?? "Sistema";
		public async Task OnGetAsync()
		{
			EstadosDisponibles = await _service.ListarEstadosAsync(UsuarioActual);

			
			if (string.IsNullOrWhiteSpace(Estado))
				Estado = "Pendiente de aprobación";

			Asientos = await _service.ListarAsync(Estado == "Todos" ? null : Estado, UsuarioActual);
		}

		public async Task<IActionResult> OnPostAccionAsync(long id, string accion, string? estado)
		{
			Estado = estado;

			var (ok, msg) = await _service.EjecutarAccionAsync(id, accion, UsuarioActual);
			TempData[ok ? "Success" : "Error"] = msg;

			return RedirectToPage(new { Estado = Estado });
		}
	}
}