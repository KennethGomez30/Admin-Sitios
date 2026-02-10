using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Sistema_Contable.Entities;
using Sistema_Contable.Repository;

namespace Sistema_Contable.Services
{
	public class EstadoAsientoService : IEstadosAsientoService
	{
		private readonly IEstadosAsientoRepository _repo;
		private readonly IBitacoraRepository _bitacoraRepo;

		// Nombre: solo letras y espacios
		private static readonly Regex RxNombre =
			new(@"^[A-Za-zÁÉÍÓÚÜÑáéíóúüñ\s]+$", RegexOptions.Compiled);

		// Descripción: letras, números y espacios 
		private static readonly Regex RxDescripcion =
			new(@"^[A-Za-z0-9ÁÉÍÓÚÜÑáéíóúüñ\s]+$", RegexOptions.Compiled);

		public EstadoAsientoService(IEstadosAsientoRepository repo, IBitacoraRepository bitacoraRepo)
		{
			_repo = repo;
			_bitacoraRepo = bitacoraRepo;
		}

		public async Task<IEnumerable<EstadosAsiento>> ListarAsync(string usuario)
		{
			try
			{
				await LogAsync(usuario, "El usuario consulta Estados de Asiento");
				return await _repo.ListarAsync();
			}
			catch (Exception ex)
			{
				await LogAsync(usuario, $"ERROR TECNICO EstadosAsiento.ListarAsync | {JsonError(ex)}");
				throw;
			}
		}

		public async Task<EstadosAsiento?> ObtenerAsync(string codigo, string usuario)
		{
			try
			{
				await LogAsync(usuario, $"El usuario consulta Estado de Asiento");
				return await _repo.ObtenerPorCodigoAsync(codigo);
			}
			catch (Exception ex)
			{
				await LogAsync(usuario, $"ERROR TECNICO EstadosAsiento.ObtenerAsync | {JsonError(ex)}");
				throw;
			}
		}

		public async Task<(bool Ok, string Mensaje)> CrearAsync(EstadosAsiento estado, string usuario)
		{
			try
			{
				var v = Validar(estado);
				if (!v.Ok) return v;

				estado.Codigo = estado.Codigo.Trim();

				Normalizar(estado);

				await _repo.InsertarAsync(estado);
				
				await LogAsync(usuario, $"CREAR EstadosAsiento | {Json(estado)}");
				return (true, "Estado creado correctamente.");
			}
			catch (Exception ex)
			{
				await LogAsync(usuario, $"ERROR TECNICO EstadosAsiento.CrearAsync | {JsonError(ex)}");
				return (false, "Ocurrió un error al crear el estado.");
			}
		}

		public async Task<(bool Ok, string Mensaje)> EditarAsync(EstadosAsiento estado, string usuario)
		{
			try
			{
				var v = Validar(estado);
				if (!v.Ok) return v;
				var antes = await _repo.ObtenerPorCodigoAsync(estado.Codigo);
				if (antes == null) return (false, "El estado no existe.");

				estado.Codigo = estado.Codigo.Trim();

				var actual = await _repo.ObtenerPorCodigoAsync(estado.Codigo);
				if (actual == null)
					return (false, "El estado no existe.");

				Normalizar(estado);

				await _repo.ActualizarAsync(estado);
				var despues = await _repo.ObtenerPorCodigoAsync(estado.Codigo);

				await LogAsync(usuario,
					$"ACTUALIZAR EstadosAsiento | {Json(new { Antes = antes, Despues = despues })}");
				return (true, "Estado actualizado correctamente.");
			}
			catch (Exception ex)
			{
				await LogAsync(usuario, $"ERROR TECNICO EstadosAsiento.EditarAsync | {JsonError(ex)}");
				return (false, "Ocurrió un error al actualizar el estado.");
			}
		}

		public async Task<(bool Ok, string Mensaje)> EliminarAsync(string codigo, string usuario)
		{
			try
			{
				var eliminado = await _repo.ObtenerPorCodigoAsync(codigo);
				codigo = (codigo ?? "").Trim();
				if (string.IsNullOrWhiteSpace(codigo))
					return (false, "El código es requerido.");

				var actual = await _repo.ObtenerPorCodigoAsync(codigo);
				if (actual == null)
					return (false, "El estado no existe.");

				await _repo.EliminarAsync(codigo);
				await LogAsync(usuario,
				$"ELIMINAR EstadosAsiento | {Json(new { Eliminado = eliminado })}");
				return (true, "Estado eliminado correctamente.");
			}
			catch (Exception ex)
			{
				await LogAsync(usuario, $"ERROR TECNICO EstadosAsiento.EliminarAsync | {JsonError(ex)}");
				return (false, "Ocurrió un error al eliminar el estado.");
			}
		}

		private static (bool Ok, string Mensaje) Validar(EstadosAsiento e)
		{
			if (e == null) return (false, "Datos inválidos.");

			e.Codigo = (e.Codigo ?? "").Trim();
			e.Nombre = (e.Nombre ?? "").Trim();
			e.Descripcion = (e.Descripcion ?? "").Trim();

			if (string.IsNullOrWhiteSpace(e.Codigo))
				return (false, "El código es requerido.");
			if (e.Codigo.Length > 10)
				return (false, "El código no debe ser mayor a 10 caracteres.");

			if (string.IsNullOrWhiteSpace(e.Nombre))
				return (false, "El nombre es requerido.");
			if (e.Nombre.Length > 40)
				return (false, "El nombre no debe ser mayor a 40 caracteres.");
			if (!RxNombre.IsMatch(e.Nombre))
				return (false, "El nombre solo debe tener letras y espacios.");

			if (string.IsNullOrWhiteSpace(e.Descripcion))
				return (false, "La descripción es requerida.");
			if (e.Descripcion.Length > 200)
				return (false, "La descripción no debe ser mayor a 200 caracteres.");
			if (!RxDescripcion.IsMatch(e.Descripcion))
				return (false, "La descripción solo debe tener letras, números y espacios.");

			return (true, "");
		}

		private static void Normalizar(EstadosAsiento e)
		{
			e.Nombre = Regex.Replace(e.Nombre, @"\s+", " ").Trim();
			e.Descripcion = Regex.Replace(e.Descripcion, @"\s+", " ").Trim();
		}

		private static string Json(object obj)
		=> System.Text.Json.JsonSerializer.Serialize(obj);

		private static string JsonError(Exception ex)
			=> Json(new
			{
				Tipo = ex.GetType().Name,
				Mensaje = ex.Message,
				Stack = ex.StackTrace?.Length > 1200 ? ex.StackTrace.Substring(0, 1200) : ex.StackTrace
			});

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