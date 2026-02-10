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
		public bool MostrarModalEliminar { get; set; }
		public string? MensajeModalEliminar { get; set; }
		public int PeriodoIdModal { get; set; }
		public string? PeriodoTextoModal { get; set; }
		public bool MostrarModalAccion { get; set; }
		public string? MensajeModalAccion { get; set; }
		public int PeriodoIdAccionModal { get; set; }
		public string? AccionModal { get; set; }
		public string? PeriodoTextoAccionModal { get; set; }
		private string UsuarioActual => HttpContext.Session.GetString("UsuarioNombre") ?? HttpContext.Session.GetString("UsuarioId") ?? "Sistema";
		public async Task OnGetAsync()
		{
			var filtro = string.IsNullOrWhiteSpace(Estado) ? null : Estado.Trim();
			Periodos = await _service.ListarAsync(filtro, UsuarioActual);
		}
		public async Task<IActionResult> OnPostEliminarAsync(int idEliminar, string? estado)
		{
			Estado = estado;
			var (ok, msg) = await _service.EliminarAsync(idEliminar, UsuarioActual);

			if (ok)
			{
				TempData["Success"] = msg;
				return RedirectToPage();
			}


			TempData["Error"] = msg;


			Periodos = await _service.ListarAsync(Estado, UsuarioActual);

			MostrarModalEliminar = true;
			MensajeModalEliminar = msg;
			PeriodoIdModal = idEliminar;


			var p = Periodos.FirstOrDefault(x => x.PeriodoId == idEliminar);
			PeriodoTextoModal = p != null ? $"{p.Anio}-{p.Mes:D2}" : $"ID {idEliminar}";

			return Page();
		}

		public async Task<IActionResult> OnPostAccionPeriodoAsync(int periodoId, string accion, string? estado)
		{
			Estado = estado;

			var usuarioCierre = HttpContext.Session.GetString("UsuarioNombre")
								?? HttpContext.Session.GetString("UsuarioId")
								?? "Sistema";

			(bool ok, string msg) res;

			if (accion == "Cerrar")
				res = await _service.CerrarAsync(periodoId, usuarioCierre);
			else
				res = await _service.ReabrirAsync(periodoId, UsuarioActual);

			TempData[res.ok ? "Success" : "Error"] = res.msg;
			return RedirectToPage(new { Estado = Estado });
		}
	}
}
