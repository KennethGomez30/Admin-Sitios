using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sistema_Contable.Entities
{
	public class PeriodosContables
	{
		public int PeriodoId { get; set; }     
		public int Anio { get; set; }          
		public int Mes { get; set; }          

		public string Estado { get; set; } 

		public bool Activo { get; set; }

		public string? UsuarioCierre { get; set; }
		public DateTime? FechaCierre { get; set; }
	}
}
