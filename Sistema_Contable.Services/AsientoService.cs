using Sistema_Contable.Entities;
using Sistema_Contable.Repository;

namespace Sistema_Contable.Services
{
    public class AsientoService : IAsientoService
    {
        private readonly IAsientoRepository _asientoRepository;

        public AsientoService(IAsientoRepository asientoRepository)
        {
            _asientoRepository = asientoRepository;
        }

        public async Task AnularOEliminarAsync(long asientoId, string usuario)
        {
            await _asientoRepository.AnularOEliminarAsync(asientoId, usuario);
        }

        public async Task<IEnumerable<Asiento>> ListarAsientosAsync(long? periodoId, string? estadoCodigo)
        {
            if (!periodoId.HasValue)
            {
                periodoId = await _asientoRepository.ObtenerPeriodoActivoAsync();
            }

            return await _asientoRepository.ListarAsientosAsync(periodoId, estadoCodigo);
        }

        public async Task<(Asiento Encabezado, IEnumerable<AsientoDetalle> Detalle)>
            ObtenerAsientoAsync(long asientoId)
        {
            return await _asientoRepository.ObtenerAsientoAsync(asientoId);
        }

        public async Task<Asiento> CrearAsientoAsync(
            DateTime fechaAsiento,
            string codigo,
            string referencia,
            string usuarioCreacionId
        )
        {
            var creado = await _asientoRepository.CrearAsientoAsync(fechaAsiento, codigo, referencia, usuarioCreacionId);

            if (creado == null || creado.AsientoId <= 0)
                throw new Exception("No se pudo crear el asiento (AsientoId inválido).");

            return creado;
        }

        public async Task AgregarDetalleAsync(
            long asientoId,
            int cuentaId,
            string tipoMovimiento,
            decimal monto,
            string descripcion,
            string usuario
        )
        {
            await _asientoRepository.AgregarDetalleAsync(
                asientoId,
                cuentaId,
                tipoMovimiento,
                monto,
                descripcion,
                usuario
            );
        }

        public async Task ActualizarDetalleAsync(long detalleId, int cuentaId, string tipoMovimiento, decimal monto, string descripcion, string usuario)
        {
            await _asientoRepository.ActualizarDetalleAsync(detalleId, cuentaId, tipoMovimiento, monto, descripcion, usuario);
        }

        public async Task EliminarDetalleAsync(long detalleId, string usuario)
        {
            await _asientoRepository.EliminarDetalleAsync(detalleId, usuario);
        }

        public async Task ActualizarEncabezadoAsync(
            long asientoId,
            DateTime fecha,
            string codigo,
            string referencia,
            string usuario
        )
        {
            await _asientoRepository.ActualizarEncabezadoAsync(
                asientoId,
                fecha,
                codigo,
                referencia,
                usuario
            );
        }

        public async Task AnularAsientoAsync(long asientoId, string usuario)
        {
            await _asientoRepository.AnularAsientoAsync(asientoId, usuario);
        }

        public async Task<IEnumerable<PeriodoContable>> ListarPeriodosAsync(int? anio, int? mes)
        {
            return await _asientoRepository.ListarPeriodosAsync(anio, mes);
        }

        
    }
}
