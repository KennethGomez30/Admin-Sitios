using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sistema_Contable.Entities
{
	public class CambiarEstadoAsiento
	{
		public long AsientoId { get; set; }
		public int Consecutivo { get; set; }
		public DateTime FechaAsiento { get; set; }
		public string Codigo { get; set; } = "";
		public string Referencia { get; set; } = "";

		public string EstadoCodigo { get; set; } = "";
		public string EstadoNombre { get; set; } = "";
	}
}
