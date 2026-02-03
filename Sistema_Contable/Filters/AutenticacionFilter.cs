using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Sistema_Contable.Filters
{
    public class AutenticacionFilter : IPageFilter
    {
        public void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
            var pagina = context.ActionDescriptor.RelativePath;

            // Páginas que NO requieren autenticación
            if (pagina != null && (pagina.Contains("/Login.cshtml") || pagina.Contains("/Logout.cshtml")))
            {
                return;
            }

            // Verificar si hay sesión activa
            var usuarioId = context.HttpContext.Session.GetString("UsuarioId");

            if (string.IsNullOrEmpty(usuarioId))
            {
                // Guardar mensaje para mostrar en Login
                context.HttpContext.Session.SetString("MensajeRedireccion",
                    "Por favor inicie sesión para utilizar el sistema");

                // Bloquear acceso y redirigir a Login
                context.Result = new RedirectToPageResult("/Login");
            }
        }

        public void OnPageHandlerExecuted(PageHandlerExecutedContext context)
        {
        }

        public void OnPageHandlerSelected(PageHandlerSelectedContext context)
        {
        }
    }
}