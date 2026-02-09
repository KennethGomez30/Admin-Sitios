using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Sistema_Contable.Pages
{
    public class ErrorModel : PageModel
    {
        public string? Detalle { get; set; }

        public void OnGet()
        {
            var feature = HttpContext.Features.Get<IExceptionHandlerFeature>();
            Detalle = feature?.Error?.ToString();
        }
    }
}
