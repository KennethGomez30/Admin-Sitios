using Sistema_Contable.Entities;

namespace Sistema_Contable.Services
{
    public interface IPantallaService
    {
        Task<(IEnumerable<Pantalla> Data, int Total)> ObtenerPaginadoAsync(int page, int pageSize, string? q, string? usuario);
        Task<Pantalla?> ObtenerPorIdAsync(ulong id, string? usuario);

        Task<(bool Ok, string? Error, ulong? Id)> CrearAsync(Pantalla p, string? usuario);
        Task<(bool Ok, string? Error)> ActualizarAsync(Pantalla p, string? usuario);
        Task<(bool Ok, string? Error)> EliminarAsync(ulong id, string? usuario);
    }
}

