//De Isma
namespace Sistema_Contable.Entities
{
    public class PeriodoContable
    {
        public long PeriodoId { get; set; }
        public int Anio { get; set; }
        public int Mes { get; set; }
        public string Estado { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public string Display => $"{Anio:0000}-{Mes:00} {(Activo ? "(Activo)" : "")}";
    }
}

