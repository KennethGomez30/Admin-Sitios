using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Dapper;
using Sistema_Contable.Entities;

namespace Sistema_Contable.Repository
{
    public class AsientoRepository : IAsientoRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public AsientoRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task ActualizarEncabezadoAsync(
            long asientoId,
            DateTime fecha,
            string codigo,
            string referencia,
            string usuario
        )
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            await connection.ExecuteAsync(
                "sp_asiento_actualizar_encabezado",
                new
                {
                    p_asiento_id = asientoId,
                    p_fecha_asiento = fecha,
                    p_codigo = codigo,
                    p_referencia = referencia,
                    p_usuario = usuario
                },
                commandType: CommandType.StoredProcedure
            );
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
            using var connection = _dbConnectionFactory.CreateConnection();

            await connection.ExecuteAsync(
                "sp_asiento_detalle_agregar",
                new
                {
                    p_asiento_id = asientoId,
                    p_cuenta_id = cuentaId,
                    p_tipo_movimiento = tipoMovimiento,
                    p_monto = monto,
                    p_descripcion = descripcion,
                    p_usuario = usuario
                },
                commandType: CommandType.StoredProcedure
            );
        }


        public async Task AnularAsientoAsync(long asientoId, string usuario)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            await connection.ExecuteAsync(
                "sp_asiento_anular",
                new
                {
                    p_asiento_id = asientoId,
                    p_usuario = usuario
                },
                commandType: CommandType.StoredProcedure
            );
        }


        public async Task<(Asiento Encabezado, IEnumerable<AsientoDetalle> Detalle)>
            ObtenerAsientoAsync(long asientoId)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            using var multi = await connection.QueryMultipleAsync(
                "sp_asiento_obtener_encabezado_y_detalle",
                new { p_asiento_id = asientoId },
                commandType: CommandType.StoredProcedure
            );

            var encabezado = await multi.ReadFirstAsync<Asiento>();
            var detalle = await multi.ReadAsync<AsientoDetalle>();

            return (encabezado, detalle);
        }



        public async Task<Asiento> CrearAsientoAsync(
            DateTime fechaAsiento,
            string codigo,
            string referencia,
            string usuarioCreacionId
        )
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            var asiento = await connection.QueryFirstAsync<Asiento>(
                "sp_asiento_crear",
                new
                {
                    p_fecha_asiento = fechaAsiento,
                    p_codigo = codigo,
                    p_referencia = referencia,
                    p_usuario_creacion_id = usuarioCreacionId
                },
                commandType: CommandType.StoredProcedure
            );

            return asiento;
        }



        public async Task EliminarDetalleAsync(long detalleId, string usuario)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            await connection.ExecuteAsync(
                "sp_asiento_detalle_eliminar",
                new
                {
                    p_detalle_id = detalleId,
                    p_usuario = usuario
                },
                commandType: CommandType.StoredProcedure
            );
        }


        public async Task<IEnumerable<Asiento>> ListarAsientosAsync(long? periodoId, string? estadoCodigo)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            return await connection.QueryAsync<Asiento>(
                "sp_asiento_listar",
                new
                {
                    p_periodo_id = periodoId,
                    p_estado_codigo = estadoCodigo
                },
                commandType: CommandType.StoredProcedure
            );
        }



        public async Task<long> ObtenerPeriodoActivoAsync()
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            var result = await connection.QueryFirstAsync<dynamic>(
                "sp_periodo_activo_obtener",
                commandType: CommandType.StoredProcedure
            );

            return (long)result.periodo_id;
        }


        public async Task<int> ObtenerSiguienteConsecutivoAsync(long periodoId)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            var result = await connection.QueryFirstAsync<dynamic>(
                "sp_asiento_siguiente_consecutivo",
                new { p_periodo_id = periodoId },
                commandType: CommandType.StoredProcedure
            );

            return (int)result.siguiente_consecutivo;
        }


    }
}
