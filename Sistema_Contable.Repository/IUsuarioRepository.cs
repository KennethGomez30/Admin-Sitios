using Sistema_Contable.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sistema_Contable.Repository
{
    public interface IUsuarioRepository
    {
        Task<Usuario?> ObtenerPorIdentificacionAsync(string identificacion);
        Task ActualizarIntentosLoginAsync(string identificacion, int intentos);
        Task BloquearUsuarioAsync(string identificacion);

        Task<bool> TieneAccesoRutaAsync(string usuarioId, string ruta);

        // adm7
        Task<List<UsuarioConRoles>> ObtenerTodosPaginadoAsync(int pagina, int porPagina);
        Task<int> ContarTotalAsync();
        Task<UsuarioConRoles?> ObtenerConRolesPorIdAsync(string identificacion);
        Task<bool> ExisteAsync(string identificacion);
        Task CrearAsync(Usuario usuario, List<int> rolesIds);
        Task ActualizarAsync(Usuario usuario, List<int> rolesIds);
        Task<bool> EliminarAsync(string identificacion);
        Task<bool> TieneRelacionesAsync(string identificacion);
        Task ActualizarContrasenaAsync(string identificacion, string nuevaContrasena);
    }
}
