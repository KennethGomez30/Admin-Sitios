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


        public async Task AnularOEliminarAsync(long asientoId, string usuario)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            var parameters = new
            {
                p_asiento_id = asientoId,
                p_usuario = usuario
            };

            await connection.ExecuteAsync(
                "sp_anular_o_eliminar_asiento",
                parameters,
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

            var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
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

            if (result == null)
                throw new Exception("El SP sp_asiento_crear no devolvió resultado.");

            // Support distintos alias (asiento_id / asientoId) defensivamente
            long asientoId = 0;
            try
            {
                asientoId = result.asiento_id != null ? (long)result.asiento_id
                         : result.asientoId != null ? (long)result.asientoId
                         : 0;
            }
            catch
            {
                asientoId = Convert.ToInt64(result.asiento_id ?? result.asientoId ?? 0);
            }

            if (asientoId <= 0)
                throw new Exception("El SP sp_asiento_crear devolvió AsientoId inválido.");

            var asiento = new Asiento
            {
                AsientoId = asientoId,
                Consecutivo = result.consecutivo != null ? (int)result.consecutivo : 0,
                PeriodoId = result.periodo_id != null ? (long)result.periodo_id : 0,
                EstadoCodigo = (result.estado != null ? (string)result.estado : (result.estado_codigo != null ? (string)result.estado_codigo : string.Empty)),
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
        public async Task<Asiento> CrearAsientoConDetallesAsync(
        DateTime fechaAsiento,
        string codigo,
        string referencia,
        string usuarioCreacionId,
        IEnumerable<(int CuentaId, string TipoMovimiento, decimal Monto, string Descripcion)> detalles
        )
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            await connection.OpenAsync();
            using var tran = connection.BeginTransaction();

            try
            {
                // 1) Crear encabezado (ejecutar SP dentro de la misma transacción)
                var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "sp_asiento_crear",
                    new
                    {
                        p_fecha_asiento = fechaAsiento,
                        p_codigo = codigo,
                        p_referencia = referencia,
                        p_usuario_creacion_id = usuarioCreacionId
                    },
                    commandType: CommandType.StoredProcedure,
                    transaction: tran
                );

                if (result == null)
                    throw new Exception("sp_asiento_crear no devolvió resultado.");

                long asientoId = result.asiento_id != null ? (long)result.asiento_id : Convert.ToInt64(result.asientoId ?? 0);
                if (asientoId <= 0)
                    throw new Exception("AsientoId inválido desde SP.");

                // 2) Agregar detalles (llamar al SP de detalle dentro de la misma transacción)
                foreach (var d in detalles)
                {
                    // saltar validaciones mínimas
                    if (d.CuentaId <= 0 || d.Monto <= 0) continue;

                    await connection.ExecuteAsync(
                        "sp_asiento_detalle_agregar",
                        new
                        {
                            p_asiento_id = asientoId,
                            p_cuenta_id = d.CuentaId,
                            p_tipo_movimiento = d.TipoMovimiento,
                            p_monto = d.Monto,
                            p_descripcion = d.Descripcion,
                            p_usuario = usuarioCreacionId
                        },
                        commandType: CommandType.StoredProcedure,
                        transaction: tran
                    );
                }

                // 3) commit
                tran.Commit();

                // 4) devolver objeto Asiento (puedes volver a consultar encabezado si quieres campos actualizados)
                var asiento = new Asiento
                {
                    AsientoId = asientoId,
                    FechaAsiento = fechaAsiento,
                    Codigo = codigo,
                    Referencia = referencia,
                    UsuarioCreacionId = usuarioCreacionId,
                    // opcionales: leer Totales desde la BD o dejarlos en 0 y que quien llame vuelva a consultarlos
                };

                return asiento;
            }
            catch
            {
                try { tran.Rollback(); } catch { }
                throw;
            }
            finally
            {
                connection.Close();
            }
        }



        public async Task<IEnumerable<PeriodoContable>> ListarPeriodosAsync(int? anio, int? mes)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            var rows = await connection.QueryAsync<dynamic>(
                "sp_Asiento_periodos_listar",
                new { p_anio = anio, p_mes = mes },
                commandType: CommandType.StoredProcedure
            );

            var list = new List<PeriodoContable>();

            foreach (var r in rows)
            {
                bool activoFlag = false;
                if (r.activo is bool b) activoFlag = b;
                else if (r.activo is int i) activoFlag = i == 1;
                else if (r.activo != null)
                {
                    // intento defensivo
                    var s = r.activo.ToString();
                    activoFlag = s == "1" || s.Equals("true", StringComparison.OrdinalIgnoreCase);
                }

                list.Add(new PeriodoContable
                {
                    PeriodoId = r.periodo_id != null ? (long)r.periodo_id : 0L,
                    Anio = r.anio != null ? (int)r.anio : 0,
                    Mes = r.mes != null ? (int)r.mes : 0,
                    Estado = r.estado ?? string.Empty,
                    Activo = activoFlag
                });
            }

            return list;
        }


        public async Task ActualizarDetalleAsync(
            long detalleId,
            int cuentaId,
            string tipoMovimiento,
            decimal monto,
            string descripcion,
            string usuario
        )
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            await connection.ExecuteAsync(
                "sp_asiento_detalle_actualizar",
                new
                {
                    p_detalle_id = detalleId,
                    p_cuenta_id = cuentaId,
                    p_tipo_movimiento = tipoMovimiento,
                    p_monto = monto,
                    p_descripcion = descripcion,
                    p_usuario = usuario
                },
                commandType: CommandType.StoredProcedure
            );
        }



    }


}
