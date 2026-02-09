using Sistema_Contable.Entities;
using Sistema_Contable.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sistema_Contable.Services
{
    public class CuentaService : ICuentaService
    {
        private readonly ICuentaRepository _repo;

        public CuentaService(ICuentaRepository repo)
        {
            _repo = repo;
        }

        public Task<IEnumerable<CuentaMovimiento>> ObtenerCuentasMovimientoAsync()
            => _repo.ListarCuentasMovimientoAsync();
    }
}
