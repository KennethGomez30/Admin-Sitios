using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sistema_Contable.Entities;
using Sistema_Contable.Services;

namespace Sistema_Contable.Pages.CierreContable
{
    public class IndexModel : PageModel
    {
        private readonly ICierreContableService _service;

        public IndexModel(ICierreContableService service)
        {
            _service = service;
        }

        public List<(ulong periodo_id, int anio, int mes, string estado)> Periodos { get; set; } = new();

        [BindProperty]
        public ulong PeriodoId { get; set; }

        [TempData]
        public string? MensajeExito { get; set; }

        [TempData]
        public string? MensajeError { get; set; }

        [TempData]
        public string? ResultadoJson { get; set; }

        public CierreContableResultado? Resultado { get; set; }

        public async Task OnGetAsync()
        {
            Periodos = await _service.ObtenerPeriodosAsync();

            if (!string.IsNullOrWhiteSpace(ResultadoJson))
            {
                try
                {
                    Resultado = System.Text.Json.JsonSerializer.Deserialize<CierreContableResultado>(ResultadoJson);
                }
                catch
                {
                    Resultado = null;
                }
            }
        }

        public async Task<IActionResult> OnPostEjecutarAsync()
        {
            var usuario = HttpContext.Session.GetString("UsuarioId") ?? "N/A";

            var (ok, msg, res) = await _service.EjecutarCierreAsync(PeriodoId, usuario);

            if (ok)
                MensajeExito = msg;
            else
                MensajeError = msg;

            if (res != null)
            {
                ResultadoJson = System.Text.Json.JsonSerializer.Serialize(res);
            }
            else
            {
                ResultadoJson = null;
            }

            return RedirectToPage();
        }
    }
}