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

		public CambiarEstadoAsientoService(ICambiarEstadoAsientoRepository repo)
		{
			_repo = repo;
		}

		public Task<IEnumerable<CambiarEstadoAsiento>> ListarAsync(string? estadoNombre)
			=> _repo.ListarAsync(string.IsNullOrWhiteSpace(estadoNombre) ? null : estadoNombre.Trim());

		public Task<IEnumerable<string>> ListarEstadosAsync()
			=> _repo.ListarEstadosAsync();

		public async Task<(bool Ok, string Mensaje)> EjecutarAccionAsync(long asientoId, string accion)
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
			return (true, $"Estado cambiado a: {nuevoEstado}.");
		}

		private static bool Eq(string a, string b)
			=> (a ?? "").Trim().Equals(b, StringComparison.OrdinalIgnoreCase);
	}
}
	

