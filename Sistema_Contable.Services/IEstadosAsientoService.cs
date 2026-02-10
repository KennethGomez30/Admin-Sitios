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
		Task<IEnumerable<EstadosAsiento>> ListarAsync(string usuario);
		Task<EstadosAsiento?> ObtenerAsync(string codigo, string usuario);

		Task<(bool Ok, string Mensaje)> CrearAsync(EstadosAsiento estado, string usuario);
		Task<(bool Ok, string Mensaje)> EditarAsync(EstadosAsiento estado, string usuario);
		Task<(bool Ok, string Mensaje)> EliminarAsync(string codigo, string usuario);
	}
}
