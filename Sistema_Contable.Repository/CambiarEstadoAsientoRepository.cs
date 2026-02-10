using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using MySql.Data.MySqlClient;
using Sistema_Contable.Entities;

namespace Sistema_Contable.Repository
{
	public class CambiarEstadoAsientoRepository : ICambiarEstadoAsientoRepository
	{
		private readonly string _connectionString;

		public CambiarEstadoAsientoRepository(string connectionString)
		{
			_connectionString = connectionString;
		}

		private IDbConnection Db() => new MySqlConnection(_connectionString);

		public async Task<IEnumerable<CambiarEstadoAsiento>> ListarAsync(string? estadoNombre)
		{
			using var db = Db();
			return await db.QueryAsync<CambiarEstadoAsiento>(
				"sp_asientos_listar_por_estado",
				new { p_estado_nombre = estadoNombre },
				commandType: CommandType.StoredProcedure);
		}

		public async Task<string?> ObtenerEstadoNombreAsync(long asientoId)
		{
			using var db = Db();
			return await db.ExecuteScalarAsync<string?>(
				"sp_asientos_obtener_estado",
				new { p_asiento_id = asientoId },
				commandType: CommandType.StoredProcedure);
		}

		public async Task CambiarEstadoAsync(long asientoId, string nuevoEstadoNombre)
		{
			using var db = Db();
			await db.ExecuteAsync(
				"sp_asientos_cambiar_estado",
				new { p_asiento_id = asientoId, p_nuevo_estado_nombre = nuevoEstadoNombre },
				commandType: CommandType.StoredProcedure);
		}

		public async Task<IEnumerable<string>> ListarEstadosAsync()
		{
			using var db = Db();
			var estados = await db.QueryAsync<EstadosAsiento>(
				"sp_estados_asiento_listar",
				commandType: CommandType.StoredProcedure);
			return estados.Select(x => (x.Nombre ?? "").Trim()).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().OrderBy(x => x);
		}
	}
}
