using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sistema_Contable.Entities;

namespace Sistema_Contable.Services
{
	public interface IPeriodoContableService
	{
		Task<IEnumerable<PeriodosContables>> ListarAsync(string? estado);
		Task<PeriodosContables?> ObtenerAsync(int periodoId);

		Task<(bool Ok, string Mensaje)> CrearAsync(int anio, int mes);
		Task<(bool Ok, string Mensaje)> EditarAsync(int periodoId, int anio, int mes);

		Task<(bool Ok, string Mensaje)> EliminarAsync(int periodoId);

		Task<(bool Ok, string Mensaje)> CerrarAsync(int periodoId, string usuarioCierre);
		Task<(bool Ok, string Mensaje)> ReabrirAsync(int periodoId);
	}
}
