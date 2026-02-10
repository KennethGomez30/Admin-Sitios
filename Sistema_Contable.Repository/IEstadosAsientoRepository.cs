using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sistema_Contable.Entities;

namespace Sistema_Contable.Repository
{
	public interface IEstadosAsientoRepository
	{
		Task<IEnumerable<EstadosAsiento>> ListarAsync();
		Task<EstadosAsiento?> ObtenerPorCodigoAsync(string codigo);

		Task InsertarAsync(EstadosAsiento estado);
		Task ActualizarAsync(EstadosAsiento estado);

		Task EliminarAsync(string codigo);
		
	}
}
