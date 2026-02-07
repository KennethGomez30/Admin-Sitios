using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sistema_Contable.Entities;

namespace Sistema_Contable.Repository
{
	public interface IPeriodoContableRepository
	{
		Task<IEnumerable<PeriodosContables>> ListarAsync(string? estado);
		Task<PeriodosContables?> ObtenerAsync(int periodoId);
		Task<PeriodosContables?> ObtenerPorAnioMesAsync(int anio, int mes);

		Task<int> InsertarAsync(PeriodosContables p);     // devuelve id int
		Task ActualizarAsync(PeriodosContables p);

		Task<bool> TieneRelacionAsync(int periodoId);
		Task EliminarAsync(int periodoId);

		Task<PeriodosContables?> ObtenerPeriodoAbiertoMasRecienteAsync();
		Task<IEnumerable<PeriodosContables>> ListarPeriodosAbiertosAscAsync();
		Task<IEnumerable<PeriodosContables>> ListarRangoAscAsync(int anioDesde, int mesDesde, int anioHasta, int mesHasta);

		Task MarcarActivoAsync(int periodoId);
		Task CerrarPeriodoAsync(int periodoId, string usuarioCierre);
		Task ReabrirPeriodoAsync(int periodoId);
	}
}
