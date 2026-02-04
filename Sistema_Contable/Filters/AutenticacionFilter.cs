using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Sistema_Contable.Filters
{
    public class AutenticacionFilter : IPageFilter
    {
        public void OnPageHandlerSelected(PageHandlerSelectedContext context)
        {
        }

        public void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
            var pagePath = context.ActionDescriptor.RelativePath;

            // Páginas que NO requieren autenticación (solo Login)
            var paginasPublicas = new[] { "/Login" };

            // Verificar si la página actual es pública
            bool esPaginaPublica = false;
            if (pagePath != null)
            {
                foreach (var paginaPublica in paginasPublicas)
                {
                    if (pagePath.Contains(paginaPublica, StringComparison.OrdinalIgnoreCase))
                    {
                        esPaginaPublica = true;
                        break;
                    }
                }
            }

            // Si es página pública, permitir acceso
            if (esPaginaPublica)
            {
                return;
            }

            // Para todas las demás páginas, verificar sesión
            var usuarioId = context.HttpContext.Session.GetString("UsuarioId");

            if (string.IsNullOrEmpty(usuarioId))
            {
                // No hay sesión activa, redirigir a Login
                context.HttpContext.Session.SetString("MensajeRedireccion",
                    "Por favor inicie sesión para utilizar el sistema");
                context.Result = new RedirectToPageResult("/Login");
            }
        }

        public void OnPageHandlerExecuted(PageHandlerExecutedContext context)
        {
        }
    }
}