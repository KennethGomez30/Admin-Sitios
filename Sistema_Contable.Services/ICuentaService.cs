using Sistema_Contable.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sistema_Contable.Services
{
    public interface ICuentaService
    {
        Task<IEnumerable<CuentaMovimiento>> ObtenerCuentasMovimientoAsync();
    }
}
