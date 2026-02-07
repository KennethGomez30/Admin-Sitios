
namespace Sistema_Contable.Entities
{
    public class AsientoDetalle
    {
        public long DetalleId { get; set; }
        public long AsientoId { get; set; }
        public int CuentaId { get; set; }
        public string TipoMovimiento { get; set; } // deudor / acreedor
        public decimal Monto { get; set; }
        public string Descripcion { get; set; }
    }
}
