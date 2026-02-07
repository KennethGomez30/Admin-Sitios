using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Sistema_Contable.Entities;
using Sistema_Contable.Services;
using System.ComponentModel.DataAnnotations;

namespace Sistema_Contable.Pages.Asientos
{
    public class EditModel : PageModel
    {
        private readonly IAsientoService _asientoService;
        // TODO: inyectar repositorio de cuentas si lo tienes (IConsultaCuentaRepository)
        public EditModel(IAsientoService asientoService)
        {
            _asientoService = asientoService;
        }

        [BindProperty]
        public AsientoDto Asiento { get; set; } = new AsientoDto();

        [BindProperty]
        public List<DetalleEditDto> Detalles { get; set; } = new List<DetalleEditDto>();

        [BindProperty]
        public string? Eliminados { get; set; }

        public List<SelectListItem> Cuentas { get; set; } = new List<SelectListItem>();

        public string? ErrorMessage { get; set; }

        public bool PuedeEditar { get; set; } = false;
        public bool PuedeAnular { get; set; } = false;
        public decimal TotalDebito { get; set; } = 0;
        public decimal TotalCredito { get; set; } = 0;
        public bool Balanceado => TotalDebito == TotalCredito;

        public async Task<IActionResult> OnGetAsync(long id)
        {
            try
            {
                // Cargar cuentas (TODO reemplazar por llamado real a repo)
                Cuentas = new List<SelectListItem>
                {
                    new SelectListItem("1000 - Caja", "1000"),
                    new SelectListItem("2000 - Bancos", "2000"),
                    new SelectListItem("3000 - Ventas", "3000")
                };

                var result = await _asientoService.ObtenerAsientoAsync(id);
                Asiento = new AsientoDto
                {
                    AsientoId = result.Encabezado.AsientoId,
                    Consecutivo = result.Encabezado.Consecutivo,
                    FechaAsiento = result.Encabezado.FechaAsiento,
                    Codigo = result.Encabezado.Codigo,
                    Referencia = result.Encabezado.Referencia,
                    EstadoCodigo = result.Encabezado.EstadoCodigo,
                    TotalDebito = result.Encabezado.TotalDebito,
                    TotalCredito = result.Encabezado.TotalCredito
                };

                Detalles = result.Detalle.Select(d => new DetalleEditDto
                {
                    DetalleId = d.DetalleId,
                    CuentaId = d.CuentaId,
                    TipoMovimiento = d.TipoMovimiento,
                    Monto = d.Monto,
                    Descripcion = d.Descripcion
                }).ToList();

                TotalDebito = Asiento.TotalDebito;
                TotalCredito = Asiento.TotalCredito;

                // Sólo permitir editar si está en los estados indicados
                PuedeEditar = Asiento.EstadoCodigo == "Borrador" || Asiento.EstadoCodigo == "Pendiente de aprobación";
                PuedeAnular = Asiento.EstadoCodigo != "Anulado";

                return Page();
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = ex.Message;
                return RedirectToPage("/Error");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                // recargar cuentas para la vista
                await CargarCuentasAsync();
                return Page();
            }

            try
            {
                var usuario = User?.Identity?.Name ?? "system";

                // actualizar encabezado
                await _asientoService.ActualizarEncabezadoAsync(
                    Asiento.AsientoId,
                    Asiento.FechaAsiento,
                    Asiento.Codigo ?? string.Empty,
                    Asiento.Referencia ?? string.Empty,
                    usuario
                );

                // procesar eliminados
                if (!string.IsNullOrWhiteSpace(Eliminados))
                {
                    var ids = Eliminados.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                        .Select(s => long.Parse(s));
                    foreach (var id in ids)
                    {
                        await _asientoService.EliminarDetalleAsync(id, usuario);
                    }
                }

                // procesar detalles (agregar nuevos)
                if (Detalles != null && Detalles.Any())
                {
                    foreach (var d in Detalles)
                    {
                        if (d.DetalleId == 0)
                        {
                            if (d.CuentaId == 0 || d.Monto <= 0) continue;
                            await _asientoService.AgregarDetalleAsync(
                                Asiento.AsientoId,
                                d.CuentaId,
                                d.TipoMovimiento ?? "deudor",
                                d.Monto,
                                d.Descripcion ?? string.Empty,
                                usuario
                            );
                        }
                        else
                        {
                            // Si quieres permitir editar líneas existentes (monto, desc, cuenta),
                            // necesitarías un SP para actualizar detalle. Si no existe, puedes:
                            // - eliminar y reinsertar; o
                            // - ignorar (mantener tal cual).
                            // Aquí asumimos que no hay SP de actualizar detalle; por simplicidad no se actualizan líneas existentes.
                        }
                    }
                }

                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = ex.Message;
                return RedirectToPage("/Error");
            }
        }

        // Endpoint para anular vía fetch desde JS
        public async Task<IActionResult> OnPostAnularAsync([FromBody] AnularDto dto)
        {
            try
            {
                var usuario = User?.Identity?.Name ?? "system";
                await _asientoService.AnularAsientoAsync(dto.AsientoId, usuario);
                return new JsonResult(new { ok = true });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private Task CargarCuentasAsync()
        {
            // TODO: reemplazar por llamada real a repo de cuentas
            Cuentas = new List<SelectListItem>
            {
                new SelectListItem("1000 - Caja", "1000"),
                new SelectListItem("2000 - Bancos", "2000"),
                new SelectListItem("3000 - Ventas", "3000")
            };
            return Task.CompletedTask;
        }

        public class AsientoDto
        {
            public long AsientoId { get; set; }
            public int Consecutivo { get; set; }

            [DataType(DataType.Date)]
            public DateTime FechaAsiento { get; set; }

            public string Codigo { get; set; } = string.Empty;
            public string Referencia { get; set; } = string.Empty;
            public string EstadoCodigo { get; set; } = string.Empty;
            public decimal TotalDebito { get; set; }
            public decimal TotalCredito { get; set; }
        }

        public class DetalleEditDto
        {
            public long DetalleId { get; set; }
            public int CuentaId { get; set; }
            public string TipoMovimiento { get; set; } = "deudor";
            public decimal Monto { get; set; }
            public string Descripcion { get; set; } = string.Empty;
        }

        public class AnularDto
        {
            public long AsientoId { get; set; }
        }
    }
}
