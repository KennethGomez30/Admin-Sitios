using Dapper;
using Sistema_Contable.Entities;
using System.Data;

namespace Sistema_Contable.Repository
{
    public class CierreContableRepository : ICierreContableRepository
    {
        private readonly IDbConnectionFactory _db;

        public CierreContableRepository(IDbConnectionFactory db)
        {
            _db = db;
        }

        public async Task<List<(ulong periodo_id, int anio, int mes, string estado)>> ObtenerPeriodosAsync()
        {
            using var conn = _db.CreateConnection();
            var sql = @"SELECT periodo_id, anio, mes, estado
                        FROM periodos_contables
                        WHERE estado = 'ABIERTO'
                        ORDER BY anio DESC, mes DESC;";
            var rows = await conn.QueryAsync(sql);
            return rows.Select(r => ((ulong)r.periodo_id, (int)r.anio, (int)r.mes, (string)r.estado)).ToList();
        }

        public async Task<(ulong periodo_id, int anio, int mes, string estado)?> ObtenerPeriodoPorIdAsync(ulong periodoId)
        {
            using var conn = _db.CreateConnection();
            var sql = @"SELECT periodo_id, anio, mes, estado
                        FROM periodos_contables
                        WHERE periodo_id=@periodoId
                        LIMIT 1;";
            var r = await conn.QueryFirstOrDefaultAsync(sql, new { periodoId });
            if (r == null) return null;
            return ((ulong)r.periodo_id, (int)r.anio, (int)r.mes, (string)r.estado);
        }

        public async Task<bool> ExistenPeriodosAnterioresAbiertosAsync(ulong periodoId)
        {
            using var conn = _db.CreateConnection();

            // Tomamos (año,mes) del periodo objetivo y buscamos anteriores ABIERTO
            var p = await ObtenerPeriodoPorIdAsync(periodoId);
            if (p == null) return true;

            var sql = @"
                SELECT 1
                FROM periodos_contables
                WHERE estado='ABIERTO'
                  AND (
                        anio < @anio
                        OR (anio = @anio AND mes < @mes)
                  )
                LIMIT 1;";
            var res = await conn.ExecuteScalarAsync<int?>(sql, new { anio = p.Value.anio, mes = p.Value.mes });
            return res.HasValue;
        }

        public async Task<List<(int id_cuenta, string codigo, string nombre, string tipo_saldo)>> ObtenerCuentasAsync()
        {
            using var conn = _db.CreateConnection();
            var sql = @"SELECT id_cuenta, codigo, nombre, tipo_saldo
                        FROM cuenta_contable
                        WHERE activo = 1
                        ORDER BY codigo;";
            var rows = await conn.QueryAsync(sql);
            return rows.Select(r => ((int)r.id_cuenta, (string)r.codigo, (string)r.nombre, (string)r.tipo_saldo)).ToList();
        }

        public async Task<decimal> ObtenerSaldoAnteriorAsync(ulong periodoIdAnterior, int cuentaId)
        {
            using var conn = _db.CreateConnection();
            var sql = @"SELECT saldo
                        FROM saldos_cuentas_periodo
                        WHERE periodo_id=@periodoIdAnterior AND cuenta_id=@cuentaId
                        LIMIT 1;";
            return await conn.ExecuteScalarAsync<decimal?>(sql, new { periodoIdAnterior, cuentaId }) ?? 0m;
        }

        public async Task<(decimal debe, decimal haber)> ObtenerMovimientosMesAsync(ulong periodoIdActual, int cuentaId)
        {
            using var conn = _db.CreateConnection();

            // Contabilizamos movimientos del periodo en asientos NO anulados.
            var sql = @"
                SELECT
                  SUM(CASE WHEN ad.tipo_movimiento='deudor' THEN ad.monto ELSE 0 END) AS Debe,
                  SUM(CASE WHEN ad.tipo_movimiento='acreedor' THEN ad.monto ELSE 0 END) AS Haber
                FROM asientos a
                JOIN asiento_detalle ad ON ad.asiento_id = a.asiento_id
                WHERE a.periodo_id = @periodoIdActual
                  AND a.estado_codigo <> 'Anulado'
                  AND ad.cuenta_id = @cuentaId;";
            var r = await conn.QueryFirstOrDefaultAsync(sql, new { periodoIdActual, cuentaId });
            var debe = r?.Debe == null ? 0m : (decimal)r.Debe;
            var haber = r?.Haber == null ? 0m : (decimal)r.Haber;
            return (debe, haber);
        }

        public async Task UpsertSaldoCuentaPeriodoAsync(ulong periodoId, int cuentaId, decimal saldo)
        {
            using var conn = _db.CreateConnection();
            var sql = @"
                INSERT INTO saldos_cuentas_periodo (periodo_id, cuenta_id, saldo)
                VALUES (@periodoId, @cuentaId, @saldo)
                ON DUPLICATE KEY UPDATE saldo = VALUES(saldo);";
            await conn.ExecuteAsync(sql, new { periodoId, cuentaId, saldo });
        }

        public async Task CerrarPeriodoAsync(ulong periodoId, string usuarioCierre)
        {
            using var conn = _db.CreateConnection();
            var sql = @"
                UPDATE periodos_contables
                SET estado='CERRADO',
                    usuario_cierre=@usuarioCierre,
                    fecha_cierre=NOW(),
                    activo=0
                WHERE periodo_id=@periodoId;";
            await conn.ExecuteAsync(sql, new { periodoId, usuarioCierre });
        }
    }
}