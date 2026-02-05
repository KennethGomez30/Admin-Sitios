using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sistema_Contable.Entities;
using Sistema_Contable.Repository;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace Sistema_Contable.Pages.Usuarios
{
    public class IndexModel : PageModel
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IBitacoraRepository _bitacoraRepository;
        private const int REGISTROS_POR_PAGINA = 10;

        public IndexModel(IUsuarioRepository usuarioRepository, IBitacoraRepository bitacoraRepository)
        {
            _usuarioRepository = usuarioRepository;
            _bitacoraRepository = bitacoraRepository;
        }

        public List<UsuarioConRoles> Usuarios { get; set; } = new();
        public int PaginaActual { get; set; } = 1;
        public int TotalPaginas { get; set; }

        // Variables locales para mensajes (NO persisten)
        public string? MensajeExito { get; set; }
        public string? MensajeError { get; set; }

        public async Task OnGetAsync(int pagina = 1)
        {
            // Leer y ELIMINAR TempData inmediatamente
            MensajeExito = TempData["MensajeExito"] as string;
            MensajeError = TempData["MensajeError"] as string;

            // Limpiar TempData explícitamente para que NO persista
            TempData.Remove("MensajeExito");
            TempData.Remove("MensajeError");

            PaginaActual = pagina > 0 ? pagina : 1;

            // Obtener usuarios paginados
            Usuarios = await _usuarioRepository.ObtenerTodosPaginadoAsync(PaginaActual, REGISTROS_POR_PAGINA);

            // Calcular total de páginas
            var totalRegistros = await _usuarioRepository.ContarTotalAsync();
            TotalPaginas = (int)Math.Ceiling(totalRegistros / (double)REGISTROS_POR_PAGINA);

            // Registrar consulta en bitácora
            var usuarioActual = HttpContext.Session.GetString("UsuarioId");
            await RegistrarBitacoraAsync(usuarioActual, "El usuario consulta Usuarios");
        }

        public async Task<IActionResult> OnPostEliminarAsync(string identificacionEliminar)
        {
            var usuarioActual = HttpContext.Session.GetString("UsuarioId");

            try
            {
                // Verificar si tiene relaciones
                if (await _usuarioRepository.TieneRelacionesAsync(identificacionEliminar))
                {
                    await RegistrarBitacoraAsync(usuarioActual, $"Intento fallido de eliminar usuario '{identificacionEliminar}'  - Tiene datos relacionados");
                    TempData["MensajeError"] = "No se puede eliminar un registro con datos relacionados.";
                    return RedirectToPage();
                }

                // Obtener datos antes de eliminar para bitácora
                var usuarioEliminar = await _usuarioRepository.ObtenerConRolesPorIdAsync(identificacionEliminar);

                // Eliminar usuario
                var eliminado = await _usuarioRepository.EliminarAsync(identificacionEliminar);

                if (eliminado)
                {
                    // Registrar en bitácora
                    var jsonEliminado = JsonSerializer.Serialize(new
                    {
                        usuarioEliminar?.Identificacion,
                        usuarioEliminar?.Nombre,
                        usuarioEliminar?.Apellido,
                        usuarioEliminar?.Correo,
                        usuarioEliminar?.Estado,
                        Roles = usuarioEliminar?.Roles.ConvertAll(r => r.Nombre)
                    });

                    await RegistrarBitacoraAsync(usuarioActual, $"Elimina usuario | Datos eliminados: {jsonEliminado}");

                    TempData["MensajeExito"] = "Usuario eliminado exitosamente.";
                }
                else
                {
                    await RegistrarBitacoraAsync(usuarioActual, $"Intento fallido de eliminar usuario '{identificacionEliminar}' - No se pudo eliminar");
                    TempData["MensajeError"] = "No se pudo eliminar el usuario.";
                }
            }
            catch (Exception ex)
            {
                await RegistrarBitacoraAsync(usuarioActual, $"Error al eliminar usuario: {ex.Message}");
                TempData["MensajeError"] = "Ocurrió un error al eliminar el usuario.";
            }

            return RedirectToPage();
        }

        private async Task RegistrarBitacoraAsync(string? usuario, string accion)
        {
            var bitacora = new Bitacora
            {
                FechaBitacora = DateTime.Now,
                Usuario = usuario,
                Descripcion = accion
            };

            await _bitacoraRepository.RegistrarAsync(bitacora);
        }
    }
}