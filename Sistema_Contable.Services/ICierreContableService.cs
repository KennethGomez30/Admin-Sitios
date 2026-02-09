using Sistema_Contable.Entities;

namespace Sistema_Contable.Services
{
    public interface ICierreContableService
    {
        Task<List<(ulong periodo_id, int anio, int mes, string estado)>> ObtenerPeriodosAsync();
        Task<(bool Ok, string Msg, CierreContableResultado? Resultado)> EjecutarCierreAsync(ulong periodoId, string usuario);
    }
}
