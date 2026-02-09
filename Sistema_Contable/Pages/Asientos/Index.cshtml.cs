using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sistema_Contable.Entities;
using Sistema_Contable.Services;
using Sistema_Contable.Repository;

namespace Sistema_Contable.Pages.Asientos
{
    public class IndexModel : PageModel
    {
        private readonly IAsientoService _asientoService;
        private readonly IAsientoRepository _asientoRepository;

        public IndexModel(IAsientoService asientoService, IAsientoRepository asientoRepository)
        {
            _asientoService = asientoService;
            _asientoRepository = asientoRepository;
        }

        public async Task<IActionResult> OnPostAnularAsync(long asientoId)
        {
            try
            {
                var usuario = HttpContext.Session.GetString("UsuarioId");

                if (string.IsNullOrWhiteSpace(usuario))
                    return BadRequest("Usuario no válido.");

                await _asientoService.AnularOEliminarAsync(asientoId, usuario);

                TempData["SuccessMessage"] = "Asiento anulado o eliminado correctamente.";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.InnerException?.Message ?? ex.Message;
                return RedirectToPage();
            }
        }


        // Resultados
        public List<Asiento> Asientos { get; set; } = new();

        // Periodos para el select
        public List<PeriodoContable> Periodos { get; set; } = new();

        // filtros binded desde query string (GET)
        [BindProperty(SupportsGet = true)]
        public long? PeriodoFiltroId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? EstadoFiltro { get; set; }

        public List<(string Codigo, string Nombre)> Estados { get; } = new()
        {
            ("EA3", "Borrador"),
            ("EA4", "Pendiente de aprobación"),
            ("EA2", "Aprobado"),
            ("EA5", "Rechazado"),
            ("EA1", "Anulado")
        };


        [TempData]
        public string? MensajeError { get; set; }

        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                // 1) cargar periodos (posible filtro por año/mes si luego lo agregás)
                var periodos = await _asientoRepository.ListarPeriodosAsync(null, null);
                Periodos = periodos?.ToList() ?? new List<PeriodoContable>();

                // 2) si no viene PeriodoFiltroId, usar periodo activo desde la BD
                if (!PeriodoFiltroId.HasValue)
                {
                    try
                    {
                        PeriodoFiltroId = await _asientoRepository.ObtenerPeriodoActivoAsync();
                    }
                    catch
                    {
                        // si falla, usar el primer periodo listado (defensivo)
                        if (Periodos.Any())
                            PeriodoFiltroId = Periodos.First().PeriodoId;
                    }
                }

                // 3) listar asientos con los filtros
                var asientos = await _asientoService.ListarAsientosAsync(PeriodoFiltroId, EstadoFiltro);
                Asientos = asientos?.ToList() ?? new List<Asiento>();

                return Page();
            }
            catch (Exception ex)
            {
                // Mostrar traza en TempData para depuración si neces. (en dev)
                MensajeError = ex.Message;
                ErrorMessage = ex.ToString();
                return Page();
            }
        }

        // Helper para la vista: si puede editar/anular
        public bool PuedeModificar(Asiento a)
        {
            if (a == null) return false;
            return a.EstadoCodigo == "Borrador" || a.EstadoCodigo == "Pendiente de aprobación";
        }

        // Helper para mostrar periodo (ej. "2026-02")
        public static string PeriodoDisplay(PeriodoContable p) =>
            p == null ? string.Empty : $"{p.Anio}-{p.Mes:D2}";
    }
}
