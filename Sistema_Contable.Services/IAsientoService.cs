using Sistema_Contable.Entities;

namespace Sistema_Contable.Services
{
    public interface IAsientoService
    {
        Task<IEnumerable<Asiento>> ListarAsientosAsync(long? periodoId, string? estadoCodigo);

        Task<(Asiento Encabezado, IEnumerable<AsientoDetalle> Detalle)> ObtenerAsientoAsync(long asientoId);

        Task<Asiento> CrearAsientoAsync(
            DateTime fechaAsiento,
            string codigo,
            string referencia,
            string usuarioCreacionId
        );

        Task AnularOEliminarAsync(long asientoId, string usuario);


        Task AgregarDetalleAsync(
            long asientoId,
            int cuentaId,
            string tipoMovimiento,
            decimal monto,
            string descripcion,
            string usuario
        );

        
        Task EliminarDetalleAsync(long detalleId, string usuario);
        Task ActualizarEncabezadoAsync(long asientoId, DateTime fecha, string codigo, string referencia, string usuario);
        Task ActualizarDetalleAsync(
            long detalleId,
            int cuentaId,
            string tipoMovimiento,
            decimal monto,
            string descripcion,
            string usuario
        );


        Task AnularAsientoAsync(long asientoId, string usuario);

        Task<IEnumerable<PeriodoContable>> ListarPeriodosAsync(int? anio, int? mes);

    }
}
