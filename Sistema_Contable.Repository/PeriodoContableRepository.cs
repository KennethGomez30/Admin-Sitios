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
	public class PeriodoContableRepository : IPeriodoContableRepository
	{
		private readonly string _connectionString;

		public PeriodoContableRepository(string connectionString)
		{
			_connectionString = connectionString;
		}

		private IDbConnection Db() => new MySqlConnection(_connectionString);

		public async Task<IEnumerable<PeriodosContables>> ListarAsync(string? estado)
		{
			using var db = Db();
			return await db.QueryAsync<PeriodosContables>(
				"sp_periodos_listar",
				new { p_estado = estado },
				commandType: CommandType.StoredProcedure);
		}

		public async Task<PeriodosContables?> ObtenerAsync(int periodoId)
		{
			using var db = Db();
			return await db.QueryFirstOrDefaultAsync<PeriodosContables>(
				"sp_periodos_obtener",
				new { p_periodo_id = periodoId },
				commandType: CommandType.StoredProcedure);
		}

		public async Task<PeriodosContables?> ObtenerPorAnioMesAsync(int anio, int mes)
		{
			using var db = Db();
			return await db.QueryFirstOrDefaultAsync<PeriodosContables>(
				"sp_periodos_obtener_por_anio_mes",
				new { p_anio = anio, p_mes = mes },
				commandType: CommandType.StoredProcedure);
		}

		public async Task<int> InsertarAsync(PeriodosContables p)
		{
			using var db = Db();
			
			var id = await db.ExecuteScalarAsync<int>(
				"sp_periodos_insertar",
				new
				{
					p_anio = p.Anio,
					p_mes = p.Mes,
					p_estado = p.Estado,
					p_activo = p.Activo ? 1 : 0
				},
				commandType: CommandType.StoredProcedure);

			return id;
		}

		public async Task ActualizarAsync(PeriodosContables p)
		{
			using var db = Db();
			await db.ExecuteAsync(
				"sp_periodos_actualizar",
				new
				{
					p_periodo_id = p.PeriodoId,
					p_anio = p.Anio,
					p_mes = p.Mes,
					p_estado = p.Estado,
					p_usuario_cierre = p.UsuarioCierre,
					p_fecha_cierre = p.FechaCierre
				},
				commandType: CommandType.StoredProcedure);
		}

		public async Task<bool> TieneRelacionAsync(int periodoId)
		{
			using var db = Db();
			var tiene = await db.QueryFirstAsync<int>(
				"sp_periodos_tiene_relacion",
				new { p_periodo_id = periodoId },
				commandType: CommandType.StoredProcedure);

			return tiene == 1;
		}

		public async Task EliminarAsync(int periodoId)
		{
			using var db = Db();
			await db.ExecuteAsync(
				"sp_periodos_eliminar",
				new { p_periodo_id = periodoId },
				commandType: CommandType.StoredProcedure);
		}

		public async Task<PeriodosContables?> ObtenerPeriodoAbiertoMasRecienteAsync()
		{
			using var db = Db();
			return await db.QueryFirstOrDefaultAsync<PeriodosContables>(
				"sp_periodos_abierto_mas_reciente",
				commandType: CommandType.StoredProcedure);
		}

		public async Task<IEnumerable<PeriodosContables>> ListarPeriodosAbiertosAscAsync()
		{
			using var db = Db();
			return await db.QueryAsync<PeriodosContables>(
				"sp_periodos_abiertos_asc",
				commandType: CommandType.StoredProcedure);
		}

		public async Task<IEnumerable<PeriodosContables>> ListarRangoAscAsync(int anioDesde, int mesDesde, int anioHasta, int mesHasta)
		{
			using var db = Db();
			return await db.QueryAsync<PeriodosContables>(
				"sp_periodos_listar_rango_asc",
				new
				{
					p_anio_desde = anioDesde,
					p_mes_desde = mesDesde,
					p_anio_hasta = anioHasta,
					p_mes_hasta = mesHasta
				},
				commandType: CommandType.StoredProcedure);
		}

		public async Task MarcarActivoAsync(int periodoId)
		{
			using var db = Db();
			await db.ExecuteAsync(
				"sp_periodos_marcar_activo",
				new { p_periodo_id = periodoId },
				commandType: CommandType.StoredProcedure);
		}

		public async Task CerrarPeriodoAsync(int periodoId, string usuarioCierre)
		{
			using var db = Db();
			await db.ExecuteAsync(
				"sp_periodos_cerrar",
				new { p_periodo_id = periodoId, p_usuario_cierre = usuarioCierre },
				commandType: CommandType.StoredProcedure);
		}

		public async Task ReabrirPeriodoAsync(int periodoId)
		{
			using var db = Db();
			await db.ExecuteAsync(
				"sp_periodos_reabrir",
				new { p_periodo_id = periodoId },
				commandType: CommandType.StoredProcedure);
		}
	}
}
