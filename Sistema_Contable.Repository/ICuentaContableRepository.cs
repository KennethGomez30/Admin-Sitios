using Sistema_Contable.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sistema_Contable.Repository
{
    public interface ICuentaContableRepository
    {
        Task<(IEnumerable<CuentaContable> Items, int Total)> ListarAsync(string? filtroEstado, int page, int pageSize);

        Task<CuentaContable?> ObtenerPorIdAsync(int id);
        Task<CuentaContable?> ObtenerPorCodigoAsync(string codigo);

        Task<int> CrearAsync(CuentaContable c);
        Task<bool> ActualizarAsync(CuentaContable c);
        Task<bool> EliminarAsync(int id);

        Task<bool> TieneHijosAsync(int idCuenta);
        Task<bool> TieneRelacionadosAsync(int idCuenta);

        Task<IEnumerable<(int Id, string Label)>> ListarParaPadreAsync(int? excluirId);
        Task<bool> DesactivarAceptaMovimientoAsync(int idCuentaPadre);
    }
}
