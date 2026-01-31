using Sistema_Contable.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sistema_Contable.Repository
{
    public interface IBitacoraRepository
    {
        Task RegistrarAsync(Bitacora bitacora);
    }
}
