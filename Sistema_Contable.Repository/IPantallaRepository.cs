using Sistema_Contable.Entities;

namespace Sistema_Contable.Repository
{
    public interface IPantallaRepository
    {
        Task<int> CountAsync(string? q);
        Task<IEnumerable<Pantalla>> GetPagedAsync(int page, int pageSize, string? q);
        Task<Pantalla?> GetByIdAsync(ulong id);
        Task<bool> RutaExistsAsync(string ruta, ulong? excludeId = null);
        Task<ulong> CreateAsync(Pantalla p);
        Task<bool> UpdateAsync(Pantalla p);
        Task<bool> IsAssignedAsync(ulong pantallaId);
        Task<bool> DeleteAsync(ulong pantallaId);
        Task<IEnumerable<Pantalla>> ObtenerMenuPorUsuarioAsync(string usuarioId, string seccion);

    }
}
