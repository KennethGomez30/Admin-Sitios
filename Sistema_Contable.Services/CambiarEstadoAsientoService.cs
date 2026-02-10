using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sistema_Contable.Entities;
using Sistema_Contable.Repository;

namespace Sistema_Contable.Services
{
	public class CambiarEstadoAsientoService : ICambiarEstadoAsientoService
	{
		private readonly ICambiarEstadoAsientoRepository _repo;
		private readonly IBitacoraRepository _bitacoraRepo;

		public CambiarEstadoAsientoService(ICambiarEstadoAsientoRepository repo, IBitacoraRepository bitacoraRepo)
		{
			_repo = repo;
			_bitacoraRepo = bitacoraRepo;
		}

		public async Task<IEnumerable<CambiarEstadoAsiento>> ListarAsync(string? estadoNombre, string usuario)
		{
			try
			{
				await LogAsync(usuario, "El usuario consulta Asientos");
				return await _repo.ListarAsync(string.IsNullOrWhiteSpace(estadoNombre) ? null : estadoNombre.Trim());
			}
			catch (Exception ex)
			{
				await LogAsync(usuario, $"ERROR TECNICO CambiarEstadoAsiento.ListarAsync | {JsonError(ex)}");
				throw;
			}
		}


		public async Task<IEnumerable<string>> ListarEstadosAsync(string usuario)
		{
			try
			{
				await LogAsync(usuario, "El usuario consulta Estados disponibles de Asiento");
				return await _repo.ListarEstadosAsync();
			}
			catch (Exception ex)
			{
				await LogAsync(usuario, $"ERROR TECNICO CambiarEstadoAsiento.ListarEstadosAsync | {JsonError(ex)}");
				throw;
			}
		}

		public async Task<(bool Ok, string Mensaje)> EjecutarAccionAsync(long asientoId, string accion, string usuario)
		{
			try
			{
				accion = (accion ?? "").Trim();

				var estado = (await _repo.ObtenerEstadoNombreAsync(asientoId) ?? "").Trim();
				if (string.IsNullOrWhiteSpace(estado))
					return (false, "No se pudo obtener el estado del asiento.");


				if (Eq(estado, "Anulado"))
					return (false, "El asiento está Anulado. No se permite ninguna acción.");

				if (Eq(estado, "Borrador"))
					return (false, "El asiento está en Borrador. No se permite ninguna acción.");

				string? nuevoEstado = accion switch
				{
					// Pendiente -> Aprobar/Rechazar/Anular
					"Aprobar" => Eq(estado, "Pendiente de aprobación") ? "Aprobado" : null,
					"Rechazar" => Eq(estado, "Pendiente de aprobación") ? "Rechazado" : null,
					"Anular" => (Eq(estado, "Pendiente de aprobación") || Eq(estado, "Aprobado")) ? "Anulado" : null,

					// Aprobado -> Pendiente de aprobacion
					"ReversarAprobacion" => Eq(estado, "Aprobado") ? "Pendiente de aprobación" : null,

					// Rechazado -> Pendiente de aprobacion
					"ReversarRechazo" => Eq(estado, "Rechazado") ? "Pendiente de aprobación" : null,

					_ => null
				};

				if (nuevoEstado == null)
					return (false, "Acción no permitida para el estado actual.");

				await _repo.CambiarEstadoAsync(asientoId, nuevoEstado);
				await LogAsync(usuario,
			   $"ACTUALIZAR AsientoEstado | {Json(new { AsientoId = asientoId, Accion = accion, Antes = estado, Despues = nuevoEstado })}");
				return (true, $"Estado cambiado a: {nuevoEstado}.");
			}
			catch (Exception ex)
			{
				await LogAsync(usuario, $"ERROR TECNICO CambiarEstadoAsiento.EjecutarAccionAsync | {JsonError(ex)}");
				return (false, "Ocurrió un error al ejecutar la acción.");
			}
		}

		private static bool Eq(string a, string b)
			=> (a ?? "").Trim().Equals(b, StringComparison.OrdinalIgnoreCase);

		private static string Json(object obj)
		=> System.Text.Json.JsonSerializer.Serialize(obj);

		private static string JsonError(Exception ex)
			=> Json(new { Tipo = ex.GetType().Name, Mensaje = ex.Message, Stack = ex.StackTrace });

		private async Task LogAsync(string usuario, string descripcion)
		{
			try
			{
				await _bitacoraRepo.RegistrarAsync(new Bitacora
				{
					FechaBitacora = DateTime.Now,
					Usuario = usuario,
					Descripcion = descripcion
				});
			}
			catch { }
		}
	}
}

	

