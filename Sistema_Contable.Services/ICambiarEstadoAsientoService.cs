using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sistema_Contable.Entities;

namespace Sistema_Contable.Services
{
	public interface ICambiarEstadoAsientoService
	{
		Task<IEnumerable<CambiarEstadoAsiento>> ListarAsync(string? estadoNombre);

		Task<IEnumerable<string>> ListarEstadosAsync();

		Task<(bool Ok, string Mensaje)> EjecutarAccionAsync(long asientoId, string accion);
	}
}
