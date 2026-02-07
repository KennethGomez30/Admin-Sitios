using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Sistema_Contable.Entities;
using Sistema_Contable.Services;

namespace Sistema_Contable.Pages.CuentasContables;

public class CreateModel : PageModel
{
    private readonly ICuentaContableService _service;

    public CreateModel(ICuentaContableService service)
    {
        _service = service;
    }

    [BindProperty]
    public CuentaContable Cuenta { get; set; } = new()
    {
        Activo = true,
        AceptaMovimiento = true
    };

    public List<SelectListItem> TiposCuenta { get; set; } = [];
    public List<SelectListItem> TiposSaldo { get; set; } = [];
    public List<SelectListItem> Padres { get; set; } = [];

    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        await CargarCombosAsync(excluirId: null);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await CargarCombosAsync(excluirId: null);

        var usuario = HttpContext.Session.GetString("UsuarioId") ?? "N/A";

        var (ok, msg, _) = await _service.CrearAsync(Cuenta, usuario);
        if (!ok)
        {
            ErrorMessage = msg;
            return Page();
        }

        TempData["SuccessMessage"] = msg;
        return RedirectToPage("./Index");
    }

    private async Task CargarCombosAsync(int? excluirId)
    {
        TiposCuenta =
        [
            new("Activo","Activo"),
            new("Pasivo","Pasivo"),
            new("Capital","Capital"),
            new("Gasto","Gasto"),
            new("Ingreso","Ingreso")
        ];

        TiposSaldo =
        [
            new("deudor","deudor"),
            new("acreedor","acreedor")
        ];

        var usuarioBitacora = HttpContext.Session.GetString("UsuarioId");
        var padres = await _service.PadresAsync(excluirId, usuarioBitacora);

        Padres = [new SelectListItem("(Ninguna)", "", true)];
        Padres.AddRange(padres.Select(p => new SelectListItem(p.Label, p.Id.ToString())));
    }
}
