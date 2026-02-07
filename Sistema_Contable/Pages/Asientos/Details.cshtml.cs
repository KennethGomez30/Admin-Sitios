using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sistema_Contable.Entities;
using Sistema_Contable.Services;

namespace Sistema_Contable.Pages.Asientos
{
    public class DetailsModel : PageModel
    {
        private readonly IAsientoService _asientoService;

        public DetailsModel(IAsientoService asientoService)
        {
            _asientoService = asientoService;
        }

        public Asiento Asiento { get; private set; }
        public IEnumerable<AsientoDetalle> Detalle { get; private set; }

        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(long id)
        {
            try
            {
                var resultado = await _asientoService.ObtenerAsientoAsync(id);

                Asiento = resultado.Encabezado;
                Detalle = resultado.Detalle;

                if (Asiento == null)
                {
                    return RedirectToPage("Index");
                }

                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return Page();
            }
        }
    }
}
