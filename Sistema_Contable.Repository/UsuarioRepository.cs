using Dapper;
using MySql.Data.MySqlClient;
using Sistema_Contable.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sistema_Contable.Repository
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly string _connectionString;

        public UsuarioRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<Usuario?> ObtenerPorIdentificacionAsync(string identificacion)
        {
            using var connection = new MySqlConnection(_connectionString);
            var query = @"SELECT Identificacion, Nombre, Apellido, Correo, Contrasena, 
                                 Estado, IntentosLogin, FechaCreacion, FechaModificacion 
                          FROM Usuarios 
                          WHERE Identificacion = @Identificacion";

            return await connection.QueryFirstOrDefaultAsync<Usuario>(query, new { Identificacion = identificacion });
        }

        public async Task ActualizarIntentosLoginAsync(string identificacion, int intentos)
        {
            using var connection = new MySqlConnection(_connectionString);
            var query = @"UPDATE Usuarios 
                          SET IntentosLogin = @Intentos 
                          WHERE Identificacion = @Identificacion";

            await connection.ExecuteAsync(query, new { Identificacion = identificacion, Intentos = intentos });
        }

        public async Task BloquearUsuarioAsync(string identificacion)
        {
            using var connection = new MySqlConnection(_connectionString);
            var query = @"UPDATE Usuarios 
                          SET Estado = 'Bloqueado', IntentosLogin = 3 
                          WHERE Identificacion = @Identificacion";

            await connection.ExecuteAsync(query, new { Identificacion = identificacion });
        }

        public async Task<bool> TieneAccesoRutaAsync(string usuarioId, string ruta)
        {
            using var connection = new MySqlConnection(_connectionString);

            var sql = @"
        SELECT 1
        FROM UsuarioRoles ur
        JOIN Roles r ON r.IdRol = ur.RolId
        JOIN RolPantalla rp ON rp.IdRol = r.IdRol
        JOIN pantallas p ON p.pantalla_id = rp.pantalla_id
        WHERE ur.UsuarioIdentificacion = @usuarioId
          AND p.ruta = @ruta
          AND p.estado = 'Activa'
        LIMIT 1;";

            var res = await connection.ExecuteScalarAsync<int?>(sql, new { usuarioId, ruta });
            return res.HasValue;
        }
    }
}
