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

		// =========================
		// CREAR
		// =========================
		public async Task<(bool Ok, string Mensaje)> CrearAsync(int anio, int mes)
		{
			var v = ValidarAnioMes(anio, mes);
			if (!v.Ok) return v;

			// No permitir duplicados
			if (await _repo.ObtenerPorAnioMesAsync(anio, mes) != null)
				return (false, "Ya existe un período con ese año y mes.");

			// Validar consecutividad con el abierto más reciente
			var abiertoReciente = await _repo.ObtenerPeriodoAbiertoMasRecienteAsync();
			if (abiertoReciente != null)
			{
				var (sigAnio, sigMes) = SiguienteMes(abiertoReciente.Anio, abiertoReciente.Mes);
				if (anio != sigAnio || mes != sigMes)
					return (false,
						$"Solo puede crear el siguiente período consecutivo al más reciente abierto ({sigAnio}-{sigMes:D2}).");
			}

			var nuevo = new PeriodosContables
			{
				Anio = anio,
				Mes = mes,
				Estado = "Abierto",
				Activo = true
			};

			await _repo.InsertarAsync(nuevo);
			await _repo.MarcarActivoAsync(nuevo.PeriodoId);

			return (true, "Período creado correctamente.");
		}

		// =========================
		// EDITAR
		// =========================
		public async Task<(bool Ok, string Mensaje)> EditarAsync(int periodoId, int anio, int mes)
		{
			var v = ValidarAnioMes(anio, mes);
			if (!v.Ok) return v;

			var actual = await _repo.ObtenerAsync(periodoId);
			if (actual == null)
				return (false, "El período no existe.");

			// No permitir editar períodos cerrados
			if (actual.Estado.Equals("Cerrado", StringComparison.OrdinalIgnoreCase))
				return (false, "No se puede modificar un período cerrado.");

			// Evitar duplicados por año/mes
			var duplicado = await _repo.ObtenerPorAnioMesAsync(anio, mes);
			if (duplicado != null && duplicado.PeriodoId != periodoId)
				return (false, "Ya existe un período con ese año y mes.");

			actual.Anio = anio;
			actual.Mes = mes;

			await _repo.ActualizarAsync(actual);

			// Validar que los abiertos sigan siendo consecutivos
			var abiertos = (await _repo.ListarPeriodosAbiertosAscAsync()).ToList();
			if (!SonConsecutivos(abiertos))
				return (false, "La modificación rompe la regla de períodos abiertos consecutivos.");

			return (true, "Período actualizado correctamente.");
		}

		// =========================
		// ELIMINAR
		// =========================
		public async Task<(bool Ok, string Mensaje)> EliminarAsync(int periodoId)
		{
			var actual = await _repo.ObtenerAsync(periodoId);
			if (actual == null)
				return (false, "El período no existe.");

			if (await _repo.TieneRelacionAsync(periodoId))
				return (false, "No se puede eliminar un registro con datos relacionados.");

			await _repo.EliminarAsync(periodoId);

			// Asegurar activo correcto
			var abiertoReciente = await _repo.ObtenerPeriodoAbiertoMasRecienteAsync();
			if (abiertoReciente != null)
				await _repo.MarcarActivoAsync(abiertoReciente.PeriodoId);

			return (true, "Período eliminado correctamente.");
		}

		// =========================
		// CERRAR
		// =========================
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

			foreach (var p in abiertosAsc)
			{
				if (EsMenorIgual(p.Anio, p.Mes, target.Anio, target.Mes))
				{
					await _repo.CerrarPeriodoAsync(p.PeriodoId, usuarioCierre);
				}
			}

			var nuevoActivo = await _repo.ObtenerPeriodoAbiertoMasRecienteAsync();
			if (nuevoActivo != null)
				await _repo.MarcarActivoAsync(nuevoActivo.PeriodoId);

			return (true, "Período(s) cerrado(s) correctamente.");
		}

		// =========================
		// REABRIR
		// =========================
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
				await _repo.ReabrirPeriodoAsync(p.PeriodoId);

			var abiertoReciente = await _repo.ObtenerPeriodoAbiertoMasRecienteAsync();
			if (abiertoReciente != null)
				await _repo.MarcarActivoAsync(abiertoReciente.PeriodoId);

			var abiertos = (await _repo.ListarPeriodosAbiertosAscAsync()).ToList();
			if (!SonConsecutivos(abiertos))
				return (false, "La reapertura rompe la regla de períodos abiertos consecutivos.");

			return (true, "Período(s) reabierto(s) correctamente.");
		}

		// =========================
		// HELPERS
		// =========================
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