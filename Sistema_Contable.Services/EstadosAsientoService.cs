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

		// Nombre: solo letras y espacios
		private static readonly Regex RxNombre =
			new(@"^[A-Za-zÁÉÍÓÚÜÑáéíóúüñ\s]+$", RegexOptions.Compiled);

		// Descripción: letras, números y espacios 
		private static readonly Regex RxDescripcion =
			new(@"^[A-Za-z0-9ÁÉÍÓÚÜÑáéíóúüñ\s]+$", RegexOptions.Compiled);

		public EstadoAsientoService(IEstadosAsientoRepository repo)
		{
			_repo = repo;
		}

		public Task<IEnumerable<EstadosAsiento>> ListarAsync()
			=> _repo.ListarAsync();

		public Task<EstadosAsiento?> ObtenerAsync(string codigo)
			=> _repo.ObtenerPorCodigoAsync((codigo ?? "").Trim());

		public async Task<(bool Ok, string Mensaje)> CrearAsync(EstadosAsiento estado)
		{
			var v = Validar(estado);
			if (!v.Ok) return v;

			estado.Codigo = estado.Codigo.Trim();

			Normalizar(estado);

			await _repo.InsertarAsync(estado);
			return (true, "Estado creado correctamente.");
		}

		public async Task<(bool Ok, string Mensaje)> EditarAsync(EstadosAsiento estado)
		{
			var v = Validar(estado);
			if (!v.Ok) return v;

			estado.Codigo = estado.Codigo.Trim();

			var actual = await _repo.ObtenerPorCodigoAsync(estado.Codigo);
			if (actual == null)
				return (false, "El estado no existe.");

			Normalizar(estado);

			await _repo.ActualizarAsync(estado);
			return (true, "Estado actualizado correctamente.");
		}

		public async Task<(bool Ok, string Mensaje)> EliminarAsync(string codigo)
		{
			codigo = (codigo ?? "").Trim();
			if (string.IsNullOrWhiteSpace(codigo))
				return (false, "El código es requerido.");

			var actual = await _repo.ObtenerPorCodigoAsync(codigo);
			if (actual == null)
				return (false, "El estado no existe.");

			await _repo.EliminarAsync(codigo);
			return (true, "Estado eliminado correctamente.");
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
	}
}