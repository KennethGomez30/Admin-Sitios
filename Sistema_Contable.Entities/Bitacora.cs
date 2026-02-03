using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sistema_Contable.Entities
{
    public class Bitacora
    {
        public int IdBitacora { get; set; }
        public DateTime FechaBitacora { get; set; }
        public string? Usuario { get; set; }
        public string Descripcion { get; set; } = string.Empty;
    }
}
