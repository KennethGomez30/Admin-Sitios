using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sistema_Contable.Entities;

namespace Sistema_Contable.Repository
{
	public interface ICambiarEstadoAsientoRepository
	{
		Task<IEnumerable<CambiarEstadoAsiento>> ListarAsync(string? estadoNombre); 
		Task<string?> ObtenerEstadoNombreAsync(long asientoId);
		Task CambiarEstadoAsync(long asientoId, string nuevoEstadoNombre);
		Task<IEnumerable<string>> ListarEstadosAsync();
	}
}
