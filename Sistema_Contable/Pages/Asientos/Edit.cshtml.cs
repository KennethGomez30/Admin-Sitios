using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Sistema_Contable.Services;
using System.ComponentModel.DataAnnotations;

namespace Sistema_Contable.Pages.Asientos
{
    public class EditModel : PageModel
    {
        private readonly IAsientoService _asientoService;
        private readonly ICuentaService _cuentaService;

        public EditModel(
            IAsientoService asientoService,
            ICuentaService cuentaService)
        {
            _asientoService = asientoService;
            _cuentaService = cuentaService;
        }

        public string? ErrorMessage { get; set; }

        [BindProperty]
        public AsientoDto Asiento { get; set; } = new();

        [BindProperty]
        public List<DetalleEditDto> Detalles { get; set; } = new();

        [BindProperty]
        public string? Eliminados { get; set; }

        public List<SelectListItem> Cuentas { get; set; } = new();

        public bool PuedeEditar { get; set; }
        public bool PuedeAnular { get; set; }

        public decimal TotalDebito { get; set; }
        public decimal TotalCredito { get; set; }
        public bool Balanceado => TotalDebito == TotalCredito;

        public string NombreEstado { get; set; } = string.Empty;

        /* =========================
           GET
        ==========================*/
        public async Task<IActionResult> OnGetAsync(long id)
        {
            await CargarCuentasAsync();

            var result = await _asientoService.ObtenerAsientoAsync(id);
            if (result.Encabezado == null)
                return NotFound();

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
                CuentaId = d.CuentaId ?? 0,
                TipoMovimiento = d.TipoMovimiento,
                Monto = d.Monto,
                Descripcion = d.Descripcion
            }).ToList();

            TotalDebito = Asiento.TotalDebito;
            TotalCredito = Asiento.TotalCredito;

            NombreEstado = MapEstadoNombre(Asiento.EstadoCodigo);

            PuedeEditar = Asiento.EstadoCodigo is "EA3" or "EA4";
            PuedeAnular = Asiento.EstadoCodigo != "EA1";

            return Page();
        }

        /* =========================
           POST EDITAR
        ==========================*/
        public async Task<IActionResult> OnPostAsync()
        {
            await CargarCuentasAsync();

            if (!ModelState.IsValid)
                return Page();

            var usuario = ObtenerUsuario();

            try
            {
                // Encabezado
                await _asientoService.ActualizarEncabezadoAsync(
                    Asiento.AsientoId,
                    Asiento.FechaAsiento,
                    Asiento.Codigo,
                    Asiento.Referencia,
                    usuario
                );

                // Eliminados
                if (!string.IsNullOrWhiteSpace(Eliminados))
                {
                    foreach (var id in Eliminados.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    {
                        await _asientoService.EliminarDetalleAsync(long.Parse(id), usuario);
                    }
                }

                // Detalles
                foreach (var d in Detalles)
                {
                    if (d.CuentaId == 0 || d.Monto <= 0)
                        continue;

                    if (d.DetalleId == 0)
                    {
                        await _asientoService.AgregarDetalleAsync(
                            Asiento.AsientoId,
                            d.CuentaId,
                            d.TipoMovimiento,
                            d.Monto,
                            d.Descripcion,
                            usuario
                        );
                    }
                    else
                    {
                        await _asientoService.ActualizarDetalleAsync(
                            d.DetalleId,
                            d.CuentaId,
                            d.TipoMovimiento,
                            d.Monto,
                            d.Descripcion,
                            usuario
                        );
                    }
                }

                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.InnerException?.Message ?? ex.Message;
                return Page();
            }
        }

        /* =========================
           POST ANULAR
        ==========================*/
        public async Task<IActionResult> OnPostAnularAsync([FromBody] AnularDto dto)
        {
            var usuario = ObtenerUsuario();
            await _asientoService.AnularAsientoAsync(dto.AsientoId, usuario);
            return new JsonResult(new { ok = true });
        }

        /* =========================
           HELPERS
        ==========================*/
        private string ObtenerUsuario()
        {
            var usuario = HttpContext.Session.GetString("UsuarioId");

            if (string.IsNullOrWhiteSpace(usuario))
                throw new Exception("No se pudo obtener el usuario en sesión.");

            return usuario;
        }

        private async Task CargarCuentasAsync()
        {
            var cuentas = await _cuentaService.ObtenerCuentasMovimientoAsync();

            Cuentas = cuentas
                .Select(c => new SelectListItem(
                    $"{c.Codigo} - {c.Nombre}",
                    c.CuentaId.ToString()
                ))
                .ToList();
        }

        private string MapEstadoNombre(string codigo) => codigo switch
        {
            "EA1" => "Anulado",
            "EA2" => "Aprobado",
            "EA3" => "Borrador",
            "EA4" => "Pendiente de aprobación",
            "EA5" => "Rechazado",
            _ => codigo
        };

        /* =========================
           DTOs
        ==========================*/
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
