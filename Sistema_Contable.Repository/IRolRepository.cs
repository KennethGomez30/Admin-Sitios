using Sistema_Contable.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sistema_Contable.Repository
{
    public interface IRolRepository
    {
        Task<List<Rol>> ObtenerTodosAsync();
       // Task<Rol?> ObtenerPorIdAsync(int id);

        // ROLES
        Task<List<Rol>> ObtenerTodosAsync(string? filtroNombre = null);
        Task<Rol?> ObtenerPorIdAsync(int id);
        Task<int> CrearAsync(Rol rol);
        Task<bool> ActualizarAsync(Rol rol);
        Task<bool> EliminarAsync(int id);

        // VALIDACIONES
        Task<bool> ExisteNombreAsync(string nombre, int? excluirId = null);
        Task<bool> TienePantallasAsignadasAsync(int idRol);

        // PANTALLAS (para asignación)
        Task<List<Pantalla>> ObtenerPantallasActivasAsync();
        Task<List<long>> ObtenerPantallasIdsPorRolAsync(int idRol);
        Task ReemplazarPantallasDeRolAsync(int idRol, IEnumerable<long> pantallasIds);
        Task<bool> TieneUsuariosAsignadosAsync(int idRol);

    }
}
