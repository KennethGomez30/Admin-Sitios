using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sistema_Contable.Entities
{
    public class Asiento
    {
        public long AsientoId { get; set; }
        public int Consecutivo { get; set; }
        public DateTime FechaAsiento { get; set; }
        public string Codigo { get; set; }
        public string Referencia { get; set; }
        public long PeriodoId { get; set; }
        public string EstadoCodigo { get; set; }
        public string UsuarioCreacionId { get; set; }
        public string UsuarioModificacionId { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public decimal TotalDebito { get; set; }
        public decimal TotalCredito { get; set; }
    }
}
