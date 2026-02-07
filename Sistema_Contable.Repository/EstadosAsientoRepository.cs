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
	public class EstadosAsientoRepository : IEstadosAsientoRepository
	{
		private readonly string _connectionString;

		public EstadosAsientoRepository(string connectionString)
		{
			_connectionString = connectionString;
		}

		private IDbConnection Db() => new MySqlConnection(_connectionString);

		public async Task<IEnumerable<EstadosAsiento>> ListarAsync()
		{
			using var db = Db();
			return await db.QueryAsync<EstadosAsiento>(
				"sp_estados_asiento_listar",
				commandType: CommandType.StoredProcedure
			);
		}

		public async Task<EstadosAsiento?> ObtenerPorCodigoAsync(string codigo)
		{
			using var db = Db();
			return await db.QueryFirstOrDefaultAsync<EstadosAsiento>(
				"sp_estados_asiento_obtener",
				new { p_codigo = codigo },
				commandType: CommandType.StoredProcedure
			);
		}

		public async Task InsertarAsync(EstadosAsiento e)
		{
			using var db = Db();
			await db.ExecuteAsync(
				"sp_estados_asiento_Crear",
				new { p_codigo = e.Codigo, p_nombre = e.Nombre, p_descripcion = e.Descripcion },
				commandType: CommandType.StoredProcedure
			);
		}

		public async Task ActualizarAsync(EstadosAsiento e)
		{
			using var db = Db();
			await db.ExecuteAsync(
				"sp_estados_asiento_actualizar",
				new { p_codigo = e.Codigo, p_nombre = e.Nombre, p_descripcion = e.Descripcion },
				commandType: CommandType.StoredProcedure
			);
		}


		public async Task EliminarAsync(string codigo)
		{
			using var db = Db();
			await db.ExecuteAsync(
				"sp_estados_asiento_eliminar",
				new { p_codigo = codigo },
				commandType: CommandType.StoredProcedure
			);
		}
	}
}
