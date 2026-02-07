using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sistema_Contable.Entities;

namespace Sistema_Contable.Services
{
	public interface IEstadosAsientoService
	{
		Task<IEnumerable<EstadosAsiento>> ListarAsync();
		Task<EstadosAsiento?> ObtenerAsync(string codigo);

		Task<(bool Ok, string Mensaje)> CrearAsync(EstadosAsiento estado);
		Task<(bool Ok, string Mensaje)> EditarAsync(EstadosAsiento estado);
		Task<(bool Ok, string Mensaje)> EliminarAsync(string codigo);
	}
}
