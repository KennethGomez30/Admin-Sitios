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
                    p_fecha = fecha,               // usar el nombre exacto del SP
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
                "sp_asiento_obtener",
                new { p_asiento_id = asientoId },
                commandType: CommandType.StoredProcedure
            );

            var encabezadoDyn = await multi.ReadFirstOrDefaultAsync<dynamic>();
            Asiento encabezado = null;
            if (encabezadoDyn != null)
            {
                encabezado = new Asiento
                {
                    AsientoId = encabezadoDyn.asiento_id != null ? (long)encabezadoDyn.asiento_id : 0,
                    Consecutivo = encabezadoDyn.consecutivo != null ? (int)encabezadoDyn.consecutivo : 0,
                    FechaAsiento = encabezadoDyn.fecha_asiento != null ? (DateTime)encabezadoDyn.fecha_asiento : DateTime.MinValue,
                    Codigo = encabezadoDyn.codigo ?? string.Empty,
                    Referencia = encabezadoDyn.referencia ?? string.Empty,
                    PeriodoId = encabezadoDyn.periodo_id != null ? (long)encabezadoDyn.periodo_id : 0,
                    EstadoCodigo = encabezadoDyn.estado_codigo ?? string.Empty,
                    UsuarioCreacionId = encabezadoDyn.usuario_creacion_id ?? string.Empty,
                    UsuarioModificacionId = encabezadoDyn.usuario_modificacion_id ?? string.Empty,
                    FechaCreacion = encabezadoDyn.fecha_creacion != null ? (DateTime)encabezadoDyn.fecha_creacion : DateTime.MinValue,
                    FechaModificacion = encabezadoDyn.fecha_modificacion != null ? (DateTime?)encabezadoDyn.fecha_modificacion : null,
                    TotalDebito = encabezadoDyn.total_debito != null ? (decimal)encabezadoDyn.total_debito : 0,
                    TotalCredito = encabezadoDyn.total_credito != null ? (decimal)encabezadoDyn.total_credito : 0
                };
            }

            var detalleDyn = await multi.ReadAsync<dynamic>();
            var detalleList = detalleDyn.Select(d => new AsientoDetalle
            {
                DetalleId = d.detalle_id != null ? (long)d.detalle_id : 0,
                AsientoId = d.asiento_id != null ? (long)d.asiento_id : asientoId,
                CuentaId = d.cuenta_id != null ? (int)d.cuenta_id : 0,
                TipoMovimiento = d.tipo_movimiento ?? string.Empty,
                Monto = d.monto != null ? (decimal)d.monto : 0,
                Descripcion = d.descripcion ?? string.Empty
            }).ToList();

            return (encabezado, detalleList);
        }


        public async Task<Asiento> CrearAsientoAsync(
            DateTime fechaAsiento,
            string codigo,
            string referencia,
            string usuarioCreacionId
        )
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            var result = await connection.QueryFirstAsync<dynamic>(
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

            var asiento = new Asiento
            {
                AsientoId = result.asiento_id != null ? (long)result.asiento_id : 0,
                Consecutivo = result.consecutivo != null ? (int)result.consecutivo : 0,
                PeriodoId = result.periodo_id != null ? (long)result.periodo_id : 0,
                EstadoCodigo = result.estado ?? "Borrador",
                FechaAsiento = fechaAsiento,
                Codigo = codigo,
                Referencia = referencia,
                TotalDebito = 0,
                TotalCredito = 0,
                FechaCreacion = DateTime.UtcNow,
                UsuarioCreacionId = usuarioCreacionId
            };

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

            var rows = await connection.QueryAsync<dynamic>(
                "sp_asientos_listar",
                new
                {
                    p_periodo_id = periodoId,
                    p_estado_codigo = estadoCodigo
                },
                commandType: CommandType.StoredProcedure
            );

            var list = rows.Select(r => new Asiento
            {
                AsientoId = r.asiento_id != null ? (long)r.asiento_id : 0,
                Consecutivo = r.consecutivo != null ? (int)r.consecutivo : 0,
                FechaAsiento = r.fecha_asiento != null ? (DateTime)r.fecha_asiento : DateTime.MinValue,
                Codigo = r.codigo ?? string.Empty,
                Referencia = r.referencia ?? string.Empty,
                TotalDebito = r.total_debito != null ? (decimal)r.total_debito : 0,
                TotalCredito = r.total_credito != null ? (decimal)r.total_credito : 0,
                EstadoCodigo = r.estado_codigo ?? string.Empty
            }).ToList();

            return list;
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
