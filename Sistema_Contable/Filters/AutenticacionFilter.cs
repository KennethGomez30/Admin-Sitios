using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Sistema_Contable.Repository;

namespace Sistema_Contable.Filters
{
    public class AutenticacionFilter : IAsyncPageFilter
    {
        private readonly IUsuarioRepository _usuarioRepository;

        public AutenticacionFilter(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            var pagina = context.ActionDescriptor.RelativePath ?? "";

            var rutaActual = NormalizarRuta(pagina);

            if (rutaActual.Equals("/Login", StringComparison.OrdinalIgnoreCase) ||
                rutaActual.Equals("/Logout", StringComparison.OrdinalIgnoreCase) ||
                rutaActual.Equals("/Error", StringComparison.OrdinalIgnoreCase))
            {
                await next();
                return;
            }

            var usuarioId = context.HttpContext.Session.GetString("UsuarioId");
            if (string.IsNullOrEmpty(usuarioId))
            {
                context.HttpContext.Session.SetString("MensajeRedireccion",
                    "Por favor inicie sesión para utilizar el sistema");

                context.Result = new RedirectToPageResult("/Login");
                return;
            }

            if (rutaActual.Equals("/Index", StringComparison.OrdinalIgnoreCase))
            {
                await next();
                return;
            }

            var tieneAcceso = await _usuarioRepository.TieneAccesoRutaAsync(usuarioId, rutaActual);

            if (!tieneAcceso)
            {
                context.HttpContext.Session.SetString("MensajeRedireccion",
                    "No tiene permisos para acceder a esta pantalla.");

                context.Result = new RedirectToPageResult("/Index");
                return;
            }

            await next();
        }

        public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context) => Task.CompletedTask;

        private static string NormalizarRuta(string relativePath)
        {

            var ruta = relativePath.Replace(".cshtml", "");

            if (ruta.StartsWith("/Pages/", StringComparison.OrdinalIgnoreCase))
                ruta = ruta.Substring("/Pages".Length);

            if (string.IsNullOrWhiteSpace(ruta))
                ruta = "/Index";

            return ruta;
        }
    }
}
