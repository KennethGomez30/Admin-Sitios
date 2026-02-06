using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sistema_Contable.Entities;
using Sistema_Contable.Services;

namespace Sistema_Contable.Pages.Roles
{
    public class CreateModel : PageModel
    {
        private readonly IRolService _rolService;

        public CreateModel(IRolService rolService)
        {
            _rolService = rolService;
        }

        [BindProperty]
        public string Nombre { get; set; } = string.Empty;

        [BindProperty]
        public List<long> PantallasSeleccionadas { get; set; } = new();

        public List<Pantalla> Pantallas { get; set; } = new();

        // Para modal (igual que Usuarios)
        public string? MensajeExito { get; set; }
        public string? MensajeError { get; set; }

        public async Task OnGetAsync()
        {
            // Si venís con mensajes (poco común en Create), los lee
            MensajeExito = TempData["MensajeExito"] as string;
            MensajeError = TempData["MensajeError"] as string;

            Pantallas = await _rolService.ObtenerPantallasActivasAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Pantallas = await _rolService.ObtenerPantallasActivasAsync();

            var usuario = HttpContext.Session.GetString("UsuarioId") ?? "N/A";

            var (ok, msg, _) = await _rolService.CrearAsync(usuario, Nombre, PantallasSeleccionadas);

            if (!ok)
            {
                MensajeError = msg; // se muestra en modal en la misma página
                return Page();
            }

            TempData["MensajeExito"] = msg;
            return RedirectToPage("./Index");
        }

    }// fin clase
} // fin namespace
