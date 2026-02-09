using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sistema_Contable.Entities;
using Sistema_Contable.Services;

namespace Sistema_Contable.Pages.Roles
{
    public class IndexModel : PageModel
    {
        private readonly IRolService _rolService;

        public IndexModel(IRolService rolService)
        {
            _rolService = rolService;
        }

        public List<Rol> Roles { get; set; } = new();

        // Paginación (mismo estilo que Usuarios)
        [BindProperty(SupportsGet = true)]
        public int Pagina { get; set; } = 1;

        public int PaginaActual => Pagina;
        public int TotalPaginas { get; set; }
        private const int TamanoPagina = 10;

        // Mensajes para modal
        public string? MensajeExito { get; set; }
        public string? MensajeError { get; set; }

        public async Task OnGetAsync()
        {
            // Recuperar mensajes desde TempData (si venís de Crear/Editar/Eliminar)
            MensajeExito = TempData["MensajeExito"] as string;
            MensajeError = TempData["MensajeError"] as string;

            var todos = await _rolService.ObtenerTodosAsync();

            var total = todos.Count;
            TotalPaginas = (int)Math.Ceiling(total / (double)TamanoPagina);

            if (Pagina < 1) Pagina = 1;
            if (TotalPaginas > 0 && Pagina > TotalPaginas) Pagina = TotalPaginas;

            Roles = todos
                .Skip((Pagina - 1) * TamanoPagina)
                .Take(TamanoPagina)
                .ToList();
        }

        // Handler para eliminar desde el modal
        public async Task<IActionResult> OnPostEliminarAsync(string idRolEliminar)
        {
            if (!int.TryParse(idRolEliminar, out var idRol) || idRol <= 0)
            {
                TempData["MensajeError"] = "Rol inválido.";
                return RedirectToPage("./Index");
            }

            var usuario = HttpContext.Session.GetString("UsuarioId") ?? "N/A";

            var (ok, msg) = await _rolService.EliminarAsync(usuario, idRol);

            if (!ok)
            {
                TempData["MensajeError"] = msg;
                return RedirectToPage("./Index");
            }

            TempData["MensajeExito"] = msg;
            return RedirectToPage("./Index");
        }
    }// fin clase
}
