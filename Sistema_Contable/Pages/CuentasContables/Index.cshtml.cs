using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sistema_Contable.Entities;
using Sistema_Contable.Services;

namespace Sistema_Contable.Pages.CuentasContables;

public class IndexModel : PageModel
{
    private readonly ICuentaContableService _service;

    public IndexModel(ICuentaContableService service)
    {
        _service = service;
    }

    public IEnumerable<CuentaContable> Items { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    public string? Estado { get; set; } // "activa" / "inactiva" / null

    [BindProperty(SupportsGet = true)]
    public int Page { get; set; } = 1;

    public int PageSize { get; } = 10;
    public int Total { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)Total / PageSize);

    [TempData] public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        var usuarioBitacora = HttpContext.Session.GetString("UsuarioId");

        var (items, total) = await _service.ListarAsync(Estado, Page, PageSize, usuarioBitacora);
        Items = items;
        Total = total;
    }
    
    public async Task<IActionResult> OnPostEliminarAsync(int id)
    {
        var usuario = HttpContext.Session.GetString("UsuarioId") ?? "N/A";

        var (ok, msg) = await _service.EliminarAsync(id, usuario);
        if (!ok)
        {
            ErrorMessage = msg;
            await OnGetAsync();
            return Page();
        }

        SuccessMessage = msg;
        return RedirectToPage("./Index",new { Estado, Page });
    }

}
