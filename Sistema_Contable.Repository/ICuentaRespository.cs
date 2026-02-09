using Sistema_Contable.Entities;

namespace Sistema_Contable.Repository
{
    public interface ICuentaRepository
    {
        Task<IEnumerable<CuentaMovimiento>> ListarCuentasMovimientoAsync();
    }
}
