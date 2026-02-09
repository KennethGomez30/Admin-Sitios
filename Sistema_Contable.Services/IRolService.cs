using Sistema_Contable.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sistema_Contable.Services
{
    // Servicio para gestionar roles y sus pantallas asociadas
    public interface IRolService
    {
        Task<List<Rol>> ObtenerTodosAsync(string? filtroNombre = null);
        Task<Rol?> ObtenerPorIdAsync(int id);
        Task<(bool Exitoso, string Mensaje, int? IdNuevo)> CrearAsync(string usuario, string nombre, List<long> pantallasIds);
        Task<(bool Exitoso, string Mensaje)> ActualizarAsync(string usuario, int idRol, string nombre, List<long> pantallasIds);
        Task<(bool Exitoso, string Mensaje)> EliminarAsync(string usuario, int idRol);

        // Para UI de asignación
        Task<List<Pantalla>> ObtenerPantallasActivasAsync();
        Task<List<long>> ObtenerPantallasIdsPorRolAsync(int idRol);
    }
}
