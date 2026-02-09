using Dapper;
using Sistema_Contable.Entities;
using System.Data;

namespace Sistema_Contable.Repository
{
    public class CuentaRepository : ICuentaRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public CuentaRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<IEnumerable<CuentaMovimiento>> ListarCuentasMovimientoAsync()
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            // Mapear dinámicamente para no depender de MatchNamesWithUnderscores
            var rows = await connection.QueryAsync<dynamic>(
                "sp_cuentas_movimiento_listar",
                commandType: CommandType.StoredProcedure
            );

            var list = rows.Select(r => new CuentaMovimiento
            {
                CuentaId = r.id_cuenta != null ? (int)r.id_cuenta : 0,
                Codigo = r.codigo ?? string.Empty,
                Nombre = r.nombre ?? string.Empty
            }).ToList();

            return list;
        }
    }
}
