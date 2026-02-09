using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sistema_Contable.Entities
{
    public class CuentaContable
    {
        public int IdCuenta { get; set; }
        public string Codigo { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Tipo { get; set; } = "";        // Activo/Pasivo/Capital/Gasto/Ingreso
        public string TipoSaldo { get; set; } = "";   // deudor/acreedor
        public bool AceptaMovimiento { get; set; }
        public int? IdCuentaPadre { get; set; }
        public bool Activo { get; set; }

        public DateTime FechaCreacion { get; set; }
        public DateTime FechaModificacion { get; set; }
        public string? UsuarioCreacion { get; set; }
        public string? UsuarioModificacion { get; set; }

        // Para listados (join a padre)
        public string? CuentaPadreNombre { get; set; }
        public string? CuentaPadreCodigo { get; set; }

    }
}
