using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sistema_Contable.Entities
{
    public class UsuarioConRoles
    {
        public string Identificacion { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public int IntentosLogin { get; set; }
        public List<Rol> Roles { get; set; } = new List<Rol>();

        public string NombreCompleto => $"{Nombre} {Apellido}";
        public string RolesTexto => string.Join(", ", Roles.ConvertAll(r => r.Nombre));
    }
}
