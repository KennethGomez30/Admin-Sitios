using Sistema_Contable.Entities;

namespace Sistema_Contable.Repository
{
    public interface ICierreContableRepository
    {
        Task<List<(ulong periodo_id, int anio, int mes, string estado)>> ObtenerPeriodosAsync();
        Task<(ulong periodo_id, int anio, int mes, string estado)?> ObtenerPeriodoPorIdAsync(ulong periodoId);
        Task<bool> ExistenPeriodosAnterioresAbiertosAsync(ulong periodoId);
        Task<List<(int id_cuenta, string codigo, string nombre, string tipo_saldo)>> ObtenerCuentasAsync();
        Task<decimal> ObtenerSaldoAnteriorAsync(ulong periodoIdAnterior, int cuentaId);
        Task<(decimal debe, decimal haber)> ObtenerMovimientosMesAsync(ulong periodoIdActual, int cuentaId);
        Task UpsertSaldoCuentaPeriodoAsync(ulong periodoId, int cuentaId, decimal saldo);
        Task CerrarPeriodoAsync(ulong periodoId, string usuarioCierre);
    }
}
