using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sistema_Contable.Entities
{
    public class CierreContableResultado
    {
        public ulong PeriodoId { get; set; }
        public int Anio { get; set; }
        public int Mes { get; set; }
        public decimal TotalDebe { get; set; }
        public decimal TotalHaber { get; set; }
        public bool Balanceado => Math.Round(TotalDebe, 2) == Math.Round(TotalHaber, 2);
        public List<CierreContableLinea> Lineas { get; set; } = new();
    }

    public class CierreContableLinea
    {
        public int CuentaId { get; set; }
        public string CodigoCuenta { get; set; } = "";
        public string NombreCuenta { get; set; } = "";
        public string TipoSaldo { get; set; } = "";
        public decimal SaldoAnterior { get; set; }
        public decimal MovDebe { get; set; }
        public decimal MovHaber { get; set; }
        public decimal SaldoNuevo { get; set; }
    }
}
