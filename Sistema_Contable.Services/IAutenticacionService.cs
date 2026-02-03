using Sistema_Contable.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sistema_Contable.Services
{
    public class ResultadoAutenticacion
    {
        public bool Exitoso { get; set; }
        public string Mensaje { get; set; }
        public Usuario Usuario { get; set; }
    }

    public interface IAutenticacionService
    {
        Task<ResultadoAutenticacion> AutenticarAsync(string identificacion, string contrasena);
    }
}
