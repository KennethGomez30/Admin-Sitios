using Sistema_Contable.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sistema_Contable.Services
{
    public interface ICuentaContableService
    {
        Task<(IEnumerable<CuentaContable> Items, int Total)> ListarAsync(string? estado, int page, int pageSize, string? usuarioBitacora = null);
        Task<CuentaContable?> ObtenerAsync(int id, string? usuarioBitacora = null);
        Task<IEnumerable<(int Id, string Label)>> PadresAsync(int? excluirId, string? usuarioBitacora = null);

        Task<(bool Ok, string Msg, int? IdNuevo)> CrearAsync(CuentaContable c, string usuario);
        Task<(bool Ok, string Msg)> ActualizarAsync(CuentaContable c, string usuario);
        Task<(bool Ok, string Msg)> EliminarAsync(int id, string usuario);
    }
}
