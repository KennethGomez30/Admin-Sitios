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
                          SET IntentosLogin = @Intentos, FechaModificacion = NOW()
                          WHERE Identificacion = @Identificacion";

            await connection.ExecuteAsync(query, new { Identificacion = identificacion, Intentos = intentos });
        }

        public async Task BloquearUsuarioAsync(string identificacion)
        {
            using var connection = new MySqlConnection(_connectionString);
            var query = @"UPDATE Usuarios 
                          SET Estado = 'Bloqueado', IntentosLogin = 3, FechaModificacion = NOW()
                          WHERE Identificacion = @Identificacion";

            await connection.ExecuteAsync(query, new { Identificacion = identificacion });
        }

        // ADM7
        public async Task<List<UsuarioConRoles>> ObtenerTodosPaginadoAsync(int pagina, int porPagina)
        {
            using var connection = new MySqlConnection(_connectionString);

            var offset = (pagina - 1) * porPagina;

            var query = @"SELECT u.Identificacion, u.Nombre, u.Apellido, u.Correo, u.Estado, u.IntentosLogin,
                                 r.IdRol, r.Nombre
                          FROM Usuarios u
                          LEFT JOIN UsuarioRoles ur ON u.Identificacion = ur.UsuarioIdentificacion
                          LEFT JOIN Roles r ON ur.RolId = r.IdRol
                          ORDER BY u.Identificacion
                          LIMIT @Limit OFFSET @Offset";

            var usuariosDic = new Dictionary<string, UsuarioConRoles>();

            await connection.QueryAsync<UsuarioConRoles, Rol, UsuarioConRoles>(
                query,
                (usuario, rol) =>
                {
                    if (!usuariosDic.TryGetValue(usuario.Identificacion, out var usuarioActual))
                    {
                        usuarioActual = usuario;
                        usuarioActual.Roles = new List<Rol>();
                        usuariosDic.Add(usuario.Identificacion, usuarioActual);
                    }

                    if (rol != null && rol.IdRol > 0)
                    {
                        usuarioActual.Roles.Add(rol);
                    }

                    return usuarioActual;
                },
                new { Limit = porPagina, Offset = offset },
                splitOn: "IdRol"
            );

            return usuariosDic.Values.ToList();
        }

        public async Task<int> ContarTotalAsync()
        {
            using var connection = new MySqlConnection(_connectionString);
            var query = "SELECT COUNT(*) FROM Usuarios";
            return await connection.ExecuteScalarAsync<int>(query);
        }

        public async Task<UsuarioConRoles?> ObtenerConRolesPorIdAsync(string identificacion)
        {
            using var connection = new MySqlConnection(_connectionString);

            var query = @"SELECT u.Identificacion, u.Nombre, u.Apellido, u.Correo, u.Estado, u.IntentosLogin,
                                 r.IdRol, r.Nombre
                          FROM Usuarios u
                          LEFT JOIN UsuarioRoles ur ON u.Identificacion = ur.UsuarioIdentificacion
                          LEFT JOIN Roles r ON ur.RolId = r.IdRol
                          WHERE u.Identificacion = @Identificacion";

            UsuarioConRoles? usuario = null;

            await connection.QueryAsync<UsuarioConRoles, Rol, UsuarioConRoles>(
                query,
                (u, rol) =>
                {
                    if (usuario == null)
                    {
                        usuario = u;
                        usuario.Roles = new List<Rol>();
                    }

                    if (rol != null && rol.IdRol > 0)
                    {
                        usuario.Roles.Add(rol);
                    }

                    return usuario;
                },
                new { Identificacion = identificacion },
                splitOn: "IdRol"
            );

            return usuario;
        }

        public async Task<bool> ExisteAsync(string identificacion)
        {
            using var connection = new MySqlConnection(_connectionString);
            var query = "SELECT COUNT(*) FROM Usuarios WHERE Identificacion = @Identificacion";
            var count = await connection.ExecuteScalarAsync<int>(query, new { Identificacion = identificacion });
            return count > 0;
        }

        public async Task CrearAsync(Usuario usuario, List<int> rolesIds)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                // Insertar usuario
                var queryUsuario = @"INSERT INTO Usuarios (Identificacion, Nombre, Apellido, Correo, Contrasena, Estado, IntentosLogin)
                                     VALUES (@Identificacion, @Nombre, @Apellido, @Correo, @Contrasena, @Estado, @IntentosLogin)";

                await connection.ExecuteAsync(queryUsuario, usuario, transaction);

                // Insertar roles
                if (rolesIds != null && rolesIds.Count > 0)
                {
                    var queryRoles = @"INSERT INTO UsuarioRoles (UsuarioIdentificacion, RolId)
                                       VALUES (@UsuarioIdentificacion, @RolId)";

                    foreach (var rolId in rolesIds)
                    {
                        await connection.ExecuteAsync(queryRoles,
                            new { UsuarioIdentificacion = usuario.Identificacion, RolId = rolId },
                            transaction);
                    }
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task ActualizarAsync(Usuario usuario, List<int> rolesIds)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                // Actualizar usuario
                var queryUsuario = @"UPDATE Usuarios 
                                     SET Nombre = @Nombre, 
                                         Apellido = @Apellido, 
                                         Correo = @Correo, 
                                         Estado = @Estado,
                                         FechaModificacion = NOW()
                                     WHERE Identificacion = @Identificacion";

                await connection.ExecuteAsync(queryUsuario, usuario, transaction);

                // Eliminar roles antiguos
                var queryEliminarRoles = "DELETE FROM UsuarioRoles WHERE UsuarioIdentificacion = @Identificacion";
                await connection.ExecuteAsync(queryEliminarRoles, new { usuario.Identificacion }, transaction);

                // Insertar nuevos roles
                if (rolesIds != null && rolesIds.Count > 0)
                {
                    var queryRoles = @"INSERT INTO UsuarioRoles (UsuarioIdentificacion, RolId)
                                       VALUES (@UsuarioIdentificacion, @RolId)";

                    foreach (var rolId in rolesIds)
                    {
                        await connection.ExecuteAsync(queryRoles,
                            new { UsuarioIdentificacion = usuario.Identificacion, RolId = rolId },
                            transaction);
                    }
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> TieneRelacionesAsync(string identificacion)
        {
            using var connection = new MySqlConnection(_connectionString);

            // Obtener dinamicamente todas las tablas que tienen FK hacia Usuarios
            var query = @"
                SELECT 
                    TABLE_NAME,
                    COLUMN_NAME
                FROM 
                    INFORMATION_SCHEMA.KEY_COLUMN_USAGE
                WHERE 
                    REFERENCED_TABLE_SCHEMA = DATABASE()
                    AND REFERENCED_TABLE_NAME = 'Usuarios'
                    AND TABLE_NAME != 'UsuarioRoles'";

            var relaciones = await connection.QueryAsync<(string TABLE_NAME, string COLUMN_NAME)>(query);

            // Verificar cada tabla que referencia a Usuarios
            foreach (var (tabla, columna) in relaciones)
            {
                var queryCount = $"SELECT COUNT(*) FROM {tabla} WHERE {columna} = @Identificacion";
                var count = await connection.ExecuteScalarAsync<int>(queryCount, new { Identificacion = identificacion });

                if (count > 0)
                {
                    return true;
                }
            }

            return false;
        }

        public async Task<bool> EliminarAsync(string identificacion)
        {
            using var connection = new MySqlConnection(_connectionString);

            // Primero eliminar relaciones (CASCADE se encarga de UsuarioRoles)
            var query = "DELETE FROM Usuarios WHERE Identificacion = @Identificacion";
            var rowsAffected = await connection.ExecuteAsync(query, new { Identificacion = identificacion });

            return rowsAffected > 0;
        }

        public async Task ActualizarContrasenaAsync(string identificacion, string nuevaContrasena)
        {
            using var connection = new MySqlConnection(_connectionString);
            var query = @"UPDATE Usuarios 
                          SET Contrasena = @Contrasena, FechaModificacion = NOW()
                          WHERE Identificacion = @Identificacion";

            await connection.ExecuteAsync(query, new { Identificacion = identificacion, Contrasena = nuevaContrasena });
        }
    }
}