using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sistema_Contable.Entities;

namespace Sistema_Contable.Repository
{
    public interface IAsientoRepository
    {
        Task<long> ObtenerPeriodoActivoAsync();

        Task<int> ObtenerSiguienteConsecutivoAsync(long periodoId);

        Task<Asiento> CrearAsientoAsync(
            DateTime fechaAsiento,
            string codigo,
            string referencia,
            string usuarioCreacionId
        );

        Task AgregarDetalleAsync(
            long asientoId,
            int cuentaId,
            string tipoMovimiento,
            decimal monto,
            string descripcion,
            string usuario
        );

        Task EliminarDetalleAsync(long detalleId, string usuario);

        Task<IEnumerable<Asiento>> ListarAsientosAsync(
            long? periodoId,
            string? estadoCodigo
        );

        Task<(Asiento Encabezado, IEnumerable<AsientoDetalle> Detalle)>
            ObtenerAsientoAsync(long asientoId);

        Task ActualizarEncabezadoAsync(
            long asientoId,
            DateTime fecha,
            string codigo,
            string referencia,
            string usuario
        );

        Task AnularAsientoAsync(long asientoId, string usuario);
    }
}
