using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Sistema_Contable.Entities;
using Sistema_Contable.Services;
using System.ComponentModel.DataAnnotations;

namespace Sistema_Contable.Pages.Asientos
{
    public class CreateModel : PageModel
    {
        private readonly IAsientoService _asientoService;

        public CreateModel(IAsientoService asientoService)
        {
            _asientoService = asientoService;
        }

        [BindProperty]
        public CreateAsientoDto Asiento { get; set; } = new CreateAsientoDto();

        [BindProperty]
        public List<DetalleDto> Detalles { get; set; } = new List<DetalleDto>();

        // Lista de cuentas para el select. Poblarla desde tu repo (aquí se deja placeholder).
        public List<SelectListItem> Cuentas { get; set; } = new List<SelectListItem>();

        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
            // TODO: Cargar cuentas reales desde repositorio. Ejemplo:
            // Cuentas = _cuentaRepository.Listar().Select(c => new SelectListItem(c.Nombre, c.Id.ToString())).ToList();

            // Placeholder (remplazar por llamada a repo)
            Cuentas = new List<SelectListItem>
            {
                new SelectListItem("1000 - Caja", "1000"),
                new SelectListItem("2000 - Bancos", "2000"),
                new SelectListItem("3000 - Ventas", "3000")
            };

            // Inicializa una línea vacía para UX
            if (!Detalles.Any())
            {
                Detalles.Add(new DetalleDto { TipoMovimiento = "deudor" });
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                // recargar cuentas si hubo error para que el select no quede vacío
                OnGet();
                return Page();
            }

            try
            {
                // usuario actual (ajusta según tu autenticación)
                var usuario = User?.Identity?.Name ?? "system";

                // 1) Crear encabezado (SP crea consecutivo y periodo activo)
                var creado = await _asientoService.CrearAsientoAsync(
                    Asiento.FechaAsiento,
                    Asiento.Codigo ?? string.Empty,
                    Asiento.Referencia ?? string.Empty,
                    usuario
                );

                // 2) Agregar detalles si hay
                if (Detalles != null && Detalles.Any())
                {
                    foreach (var d in Detalles)
                    {
                        // validar montos y cuenta
                        if (d.CuentaId == 0 || d.Monto <= 0) continue;

                        await _asientoService.AgregarDetalleAsync(
                            creado.AsientoId,
                            d.CuentaId,
                            d.TipoMovimiento ?? "deudor",
                            d.Monto,
                            d.Descripcion ?? string.Empty,
                            usuario
                        );
                    }
                }

                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                // Si quieres mostrar detalle en la página de error
                TempData["MensajeError"] = ex.Message;
                return RedirectToPage("/Error");
            }
        }

        public class CreateAsientoDto
        {
            [Required]
            [DataType(DataType.Date)]
            public DateTime FechaAsiento { get; set; } = DateTime.Today;

            [Required]
            public string Codigo { get; set; } = string.Empty;

            public string Referencia { get; set; } = string.Empty;
        }

        public class DetalleDto
        {
            public int CuentaId { get; set; }
            public string TipoMovimiento { get; set; } = "deudor";
            public decimal Monto { get; set; }
            public string Descripcion { get; set; } = string.Empty;
        }
    }
}
