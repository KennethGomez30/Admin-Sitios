using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Sistema_Contable.Services;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Sistema_Contable.Pages.Asientos
{
    public class CreateModel : PageModel
    {
        private readonly IAsientoService _asientoService;
        private readonly ICuentaService _cuentaService;

        public CreateModel(
            IAsientoService asientoService,
            ICuentaService cuentaService)
        {
            _asientoService = asientoService;
            _cuentaService = cuentaService;
        }

        public string? ErrorMessage { get; set; }

        [BindProperty]
        public CreateAsientoDto Asiento { get; set; } = new();

        [BindProperty]
        public List<DetalleDto> Detalles { get; set; } = new();

        public List<SelectListItem> Cuentas { get; set; } = new();

        public async Task OnGetAsync()
        {
            await CargarCuentasAsync();

            if (!Detalles.Any())
            {
                Detalles.Add(new DetalleDto { TipoMovimiento = "deudor" });
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // 🔹 LIMPIAR FILAS VACÍAS (PASO 3)
            Detalles = Detalles
                .Where(d => d.CuentaId.HasValue && d.Monto > 0)
                .ToList();

            if (!Detalles.Any())
            {
                ModelState.AddModelError("", "Debe agregar al menos una línea válida.");
            }

            if (!ModelState.IsValid)
            {
                await CargarCuentasAsync();
                return Page();
            }

            // 1️⃣ USUARIO DESDE SESIÓN (igual que tus compañeros)
            var usuario = HttpContext.Session.GetString("UsuarioId");

            if (string.IsNullOrWhiteSpace(usuario))
            {
                ErrorMessage = "No se pudo obtener el usuario en sesión. Inicie sesión nuevamente.";
                await CargarCuentasAsync();
                return Page();
            }

            try
            {
                // 2️⃣ CREAR ASIENTO
                var creado = await _asientoService.CrearAsientoAsync(
                    Asiento.FechaAsiento,
                    Asiento.Codigo,
                    Asiento.Referencia ?? string.Empty,
                    usuario
                );

                if (creado == null || creado.AsientoId <= 0)
                {
                    ErrorMessage = "No se pudo crear el asiento.";
                    await CargarCuentasAsync();
                    return Page();
                }

                // 3️⃣ DETALLES (YA LIMPIOS)
                foreach (var d in Detalles)
                {
                    await _asientoService.AgregarDetalleAsync(
                        creado.AsientoId,
                        d.CuentaId!.Value,
                        d.TipoMovimiento,
                        d.Monto,
                        d.Descripcion ?? string.Empty,
                        usuario
                    );
                }

                TempData["SuccessMessage"] = "Asiento creado correctamente.";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.InnerException?.Message ?? ex.Message;
                await CargarCuentasAsync();
                return Page();
            }
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

        public class CreateAsientoDto
        {
            [Required]
            [DataType(DataType.Date)]
            public DateTime FechaAsiento { get; set; } = DateTime.Today;

            [Required]
            public string Codigo { get; set; } = string.Empty;

            public string? Referencia { get; set; }
        }

        public class DetalleDto
        {
            public int? CuentaId { get; set; }   // 🔹 AHORA OPCIONAL
            public string TipoMovimiento { get; set; } = "deudor";
            public decimal Monto { get; set; }
            public string? Descripcion { get; set; }
        }
    }
}
