using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sistema_Contable.Entities;
using Sistema_Contable.Services;

namespace Sistema_Contable.Pages.Roles
{
    public class EditModel : PageModel
    {
        private readonly IRolService _rolService;

        public EditModel(IRolService rolService)
        {
            _rolService = rolService;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        [BindProperty]
        public string Nombre { get; set; } = string.Empty;

        [BindProperty]
        public List<long> PantallasSeleccionadas { get; set; } = new();

        public List<Pantalla> Pantallas { get; set; } = new();
        public HashSet<long> PantallasAsignadas { get; set; } = new();

        // Para modal
        public string? MensajeError { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var rol = await _rolService.ObtenerPorIdAsync(Id);
            if (rol is null)
            {
                TempData["MensajeError"] = "Rol no encontrado.";
                return RedirectToPage("./Index");
            }

            Nombre = rol.Nombre;

            Pantallas = await _rolService.ObtenerPantallasActivasAsync();
            PantallasAsignadas = (await _rolService.ObtenerPantallasIdsPorRolAsync(Id)).ToHashSet();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Para repintar la lista si ocurre un error
            Pantallas = await _rolService.ObtenerPantallasActivasAsync();
            PantallasAsignadas = (await _rolService.ObtenerPantallasIdsPorRolAsync(Id)).ToHashSet();

            var usuario = HttpContext.Session.GetString("UsuarioId") ?? "N/A";

            var (ok, msg) = await _rolService.ActualizarAsync(usuario, Id, Nombre, PantallasSeleccionadas);

            if (!ok)
            {
                MensajeError = msg;
                return Page();
            }

            TempData["MensajeExito"] = msg;
            return RedirectToPage("./Index");
        }
    } // fin clase
}
