using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sistema_Contable.Entities;
using Sistema_Contable.Repository;

namespace Sistema_Contable.Services
{
	public class PeriodoContableService : IPeriodoContableService
	{
		private readonly IPeriodoContableRepository _repo;

		public PeriodoContableService(IPeriodoContableRepository repo)
		{
			_repo = repo;
		}

		public Task<IEnumerable<PeriodosContables>> ListarAsync(string? estado)
			=> _repo.ListarAsync(string.IsNullOrWhiteSpace(estado) ? null : estado.Trim());

		public Task<PeriodosContables?> ObtenerAsync(int periodoId)
			=> _repo.ObtenerAsync(periodoId);

		//Crear
		public async Task<(bool Ok, string Mensaje)> CrearAsync(int anio, int mes)
		{
			var v = ValidarAnioMes(anio, mes);
			if (!v.Ok) return v;

			// No permitir duplicados
			if (await _repo.ObtenerPorAnioMesAsync(anio, mes) != null)
				return (false, "Ya existe un período con ese año y mes.");


			var nuevo = new PeriodosContables
			{
				Anio = anio,
				Mes = mes,
				Estado = "Abierto",
				Activo = true
			};

			var abiertos = (await _repo.ListarPeriodosAbiertosAscAsync()).ToList();
			abiertos.Add(nuevo);

			if (!SonConsecutivos(abiertos))
				return (false, "No se puede crear el período abierto");

			var id = await _repo.InsertarAsync(nuevo);
			nuevo.PeriodoId = id;

			var abiertoReciente = abiertos
	   .OrderByDescending(x => x.Anio)
	   .ThenByDescending(x => x.Mes)
	   .FirstOrDefault();

			if (abiertoReciente != null)
			{
				
				if (abiertoReciente.PeriodoId == 0)
					await _repo.MarcarActivoAsync(id);
				else
					await _repo.MarcarActivoAsync(abiertoReciente.PeriodoId);
			}

			return (true, "Período creado correctamente.");
		}
		

		//Editar
		public async Task<(bool Ok, string Mensaje)> EditarAsync(int periodoId, int anio, int mes, string? usuarioCierre, DateTime? fechaCierre)
		{
			var v = ValidarAnioMes(anio, mes);
			if (!v.Ok) return v;

			var actual = await _repo.ObtenerAsync(periodoId);
			if (actual == null)
				return (false, "El período no existe.");

			var estado = (actual.Estado ?? "").Trim();


			// Evitar duplicados por año/mes
			var duplicado = await _repo.ObtenerPorAnioMesAsync(anio, mes);
			if (duplicado != null && duplicado.PeriodoId != periodoId)
				return (false, "Ya existe un período con ese año y mes.");

			actual.Anio = anio;
			actual.Mes = mes;
			actual.Estado = estado;

			if (estado.Equals("Cerrado", StringComparison.OrdinalIgnoreCase))
			{
				usuarioCierre = (usuarioCierre ?? "").Trim();
				if (string.IsNullOrWhiteSpace(usuarioCierre))
					return (false, "El usuario de cierre es requerido para períodos cerrados.");

				if (fechaCierre == null)
					return (false, "La fecha de cierre es requerida para períodos cerrados.");

				actual.UsuarioCierre = usuarioCierre;
				actual.FechaCierre = fechaCierre;
			}
			else
			{
				
				actual.UsuarioCierre = null;
				actual.FechaCierre = null;
			}

			await _repo.ActualizarAsync(actual);

			// Validar consecutividad de abiertos (por si año/mes tocó la secuencia)
			var abiertos = (await _repo.ListarPeriodosAbiertosAscAsync()).ToList();
			if (!SonConsecutivos(abiertos))
				return (false, "La modificación rompe la regla de períodos abiertos consecutivos.");

			return (true, "Período actualizado correctamente.");
		}

		//Eliminar
		public async Task<(bool Ok, string Mensaje)> EliminarAsync(int periodoId)
		{
			var actual = await _repo.ObtenerAsync(periodoId);
			if (actual == null)
				return (false, "El período no existe.");

			if (await _repo.TieneRelacionAsync(periodoId))
				return (false, "No se puede eliminar un registro con datos relacionados.");

			await _repo.EliminarAsync(periodoId);

			
			var abiertoReciente = await _repo.ObtenerPeriodoAbiertoMasRecienteAsync();
			if (abiertoReciente != null)
				await _repo.MarcarActivoAsync(abiertoReciente.PeriodoId);

			return (true, "Período eliminado correctamente.");
		}

		//Cerrar
		public async Task<(bool Ok, string Mensaje)> CerrarAsync(int periodoId, string usuarioCierre)
		{
			usuarioCierre = (usuarioCierre ?? "").Trim();
			if (string.IsNullOrWhiteSpace(usuarioCierre))
				return (false, "El usuario de cierre es requerido.");

			var target = await _repo.ObtenerAsync(periodoId);
			if (target == null)
				return (false, "El período no existe.");

			if (target.Estado.Equals("Cerrado", StringComparison.OrdinalIgnoreCase))
				return (false, "El período ya está cerrado.");

			var abiertosAsc = (await _repo.ListarPeriodosAbiertosAscAsync()).ToList();
			var cerroAlgo = false;
			foreach (var p in abiertosAsc)
			{
				if (EsMenorIgual(p.Anio, p.Mes, target.Anio, target.Mes))
				{
					await _repo.CerrarPeriodoAsync(p.PeriodoId, usuarioCierre);
				}
			}

			if (!cerroAlgo)
				await _repo.CerrarPeriodoAsync(target.PeriodoId, usuarioCierre);

			var abiertoReciente = await _repo.ObtenerPeriodoAbiertoMasRecienteAsync();
			if (abiertoReciente != null)
				await _repo.MarcarActivoAsync(abiertoReciente.PeriodoId);

			return (true, "Período(s) cerrado(s) correctamente.");
		}

		//Reabrir
		public async Task<(bool Ok, string Mensaje)> ReabrirAsync(int periodoId)
		{
			var target = await _repo.ObtenerAsync(periodoId);
			if (target == null)
				return (false, "El período no existe.");

			if (target.Estado.Equals("Abierto", StringComparison.OrdinalIgnoreCase))
				return (false, "El período ya está abierto.");

			var todos = (await _repo.ListarAsync(null)).ToList();

			var cerradoMasReciente = todos
				.Where(x => x.Estado.Equals("Cerrado", StringComparison.OrdinalIgnoreCase))
				.OrderByDescending(x => x.Anio)
				.ThenByDescending(x => x.Mes)
				.FirstOrDefault();

			if (cerradoMasReciente == null)
				return (false, "No hay períodos cerrados para reabrir.");

			var rango = await _repo.ListarRangoAscAsync(
				target.Anio, target.Mes,
				cerradoMasReciente.Anio, cerradoMasReciente.Mes
			);

			foreach (var p in rango)
			{
				if (p.Estado.Equals("Cerrado", StringComparison.OrdinalIgnoreCase))
					await _repo.ReabrirPeriodoAsync(p.PeriodoId);
			}

			// Validar consecutivos de abiertos
			var abiertos = (await _repo.ListarPeriodosAbiertosAscAsync()).ToList();
			if (!SonConsecutivos(abiertos))
				return (false, "No se puede reabrir.");

			// Activo = más reciente abierto
			var abiertoReciente = abiertos
				.OrderByDescending(x => x.Anio)
				.ThenByDescending(x => x.Mes)
				.FirstOrDefault();

			if (abiertoReciente != null)
				await _repo.MarcarActivoAsync(abiertoReciente.PeriodoId);

			return (true, "Período(s) reabierto(s) correctamente.");
		}

       //Validaciones
		private static (bool Ok, string Mensaje) ValidarAnioMes(int anio, int mes)
		{
			if (anio < 1900 || anio > 2100)
				return (false, "Año inválido.");

			if (mes < 1 || mes > 12)
				return (false, "Mes inválido.");

			return (true, "");
		}

		private static (int anio, int mes) SiguienteMes(int anio, int mes)
			=> mes == 12 ? (anio + 1, 1) : (anio, mes + 1);

		private static bool EsMenorIgual(int a1, int m1, int a2, int m2)
			=> (a1 < a2) || (a1 == a2 && m1 <= m2);

		private static bool SonConsecutivos(List<PeriodosContables> abiertosAsc)
		{
			if (abiertosAsc.Count <= 1) return true;

			abiertosAsc = abiertosAsc
				.OrderBy(x => x.Anio)
				.ThenBy(x => x.Mes)
				.ToList();

			for (int i = 1; i < abiertosAsc.Count; i++)
			{
				var prev = abiertosAsc[i - 1];
				var cur = abiertosAsc[i];

				var (sigAnio, sigMes) = SiguienteMes(prev.Anio, prev.Mes);
				if (cur.Anio != sigAnio || cur.Mes != sigMes)
					return false;
			}
			return true;
		}
	}
}