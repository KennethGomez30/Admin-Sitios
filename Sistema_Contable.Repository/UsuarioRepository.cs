using Dapper;
using MySql.Data.MySqlClient;
using Sistema_Contable.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sistema_Contable.Repository
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public UsuarioRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<Usuario?> ObtenerPorIdentificacionAsync(string identificacion)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Usuario>(
                "sp_ObtenerUsuarioPorIdentificacion",
                new { p_Identificacion = identificacion },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task ActualizarIntentosLoginAsync(string identificacion, int intentos)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            await connection.ExecuteAsync(
                "sp_ActualizarIntentosLogin",
                new { p_Identificacion = identificacion, p_Intentos = intentos },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task BloquearUsuarioAsync(string identificacion)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            await connection.ExecuteAsync(
                "sp_BloquearUsuario",
                new { p_Identificacion = identificacion },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<List<UsuarioConRoles>> ObtenerTodosPaginadoAsync(int pagina, int porPagina)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            var offset = (pagina - 1) * porPagina;

            var usuariosDic = new Dictionary<string, UsuarioConRoles>();

            await connection.QueryAsync<UsuarioConRoles, Rol, UsuarioConRoles>(
                "sp_ObtenerUsuariosPaginados",
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
                new { p_Limit = porPagina, p_Offset = offset },
                splitOn: "IdRol",
                commandType: CommandType.StoredProcedure
            );

            return usuariosDic.Values.ToList();
        }

        public async Task<int> ContarTotalAsync()
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            return await connection.ExecuteScalarAsync<int>(
                "sp_ContarTotalUsuarios",
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<UsuarioConRoles?> ObtenerConRolesPorIdAsync(string identificacion)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            UsuarioConRoles? usuario = null;

            await connection.QueryAsync<UsuarioConRoles, Rol, UsuarioConRoles>(
                "sp_ObtenerUsuarioConRolesPorId",
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
                new { p_Identificacion = identificacion },
                splitOn: "IdRol",
                commandType: CommandType.StoredProcedure
            );

            return usuario;
        }

        public async Task<bool> ExisteAsync(string identificacion)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            var count = await connection.ExecuteScalarAsync<int>(
                "sp_ExisteUsuario",
                new { p_Identificacion = identificacion },
                commandType: CommandType.StoredProcedure
            );
            return count > 0;
        }

        public async Task CrearAsync(Usuario usuario, List<int> rolesIds)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                await connection.ExecuteAsync(
                    "sp_CrearUsuario",
                    new
                    {
                        p_Identificacion = usuario.Identificacion,
                        p_Nombre = usuario.Nombre,
                        p_Apellido = usuario.Apellido,
                        p_Correo = usuario.Correo,
                        p_Contrasena = usuario.Contrasena,
                        p_Estado = usuario.Estado,
                        p_IntentosLogin = usuario.IntentosLogin
                    },
                    transaction,
                    commandType: CommandType.StoredProcedure
                );

                if (rolesIds != null && rolesIds.Count > 0)
                {
                    foreach (var rolId in rolesIds)
                    {
                        await connection.ExecuteAsync(
                            "sp_InsertarUsuarioRol",
                            new
                            {
                                p_UsuarioIdentificacion = usuario.Identificacion,
                                p_RolId = rolId
                            },
                            transaction,
                            commandType: CommandType.StoredProcedure
                        );
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
            using var connection = _dbConnectionFactory.CreateConnection();
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                await connection.ExecuteAsync(
                    "sp_ActualizarUsuario",
                    new
                    {
                        p_Identificacion = usuario.Identificacion,
                        p_Nombre = usuario.Nombre,
                        p_Apellido = usuario.Apellido,
                        p_Correo = usuario.Correo,
                        p_Estado = usuario.Estado
                    },
                    transaction,
                    commandType: CommandType.StoredProcedure
                );

                await connection.ExecuteAsync(
                    "sp_EliminarRolesUsuario",
                    new { p_Identificacion = usuario.Identificacion },
                    transaction,
                    commandType: CommandType.StoredProcedure
                );

                if (rolesIds != null && rolesIds.Count > 0)
                {
                    foreach (var rolId in rolesIds)
                    {
                        await connection.ExecuteAsync(
                            "sp_InsertarUsuarioRol",
                            new
                            {
                                p_UsuarioIdentificacion = usuario.Identificacion,
                                p_RolId = rolId
                            },
                            transaction,
                            commandType: CommandType.StoredProcedure
                        );
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
            using var connection = _dbConnectionFactory.CreateConnection();

            // Verificar tablas con Foreign Key formal hacia Usuarios
            var queryFK = @"
                SELECT TABLE_NAME, COLUMN_NAME
                FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
                WHERE 
                    REFERENCED_TABLE_SCHEMA = DATABASE()
                    AND REFERENCED_TABLE_NAME = 'Usuarios'
                    AND CONSTRAINT_NAME != 'PRIMARY'
                    AND TABLE_NAME != 'UsuarioRoles'";

            var tablasConFK = await connection.QueryAsync<(string TABLE_NAME, string COLUMN_NAME)>(queryFK);

            foreach (var (tabla, columna) in tablasConFK)
            {
                var queryExiste = $"SELECT EXISTS(SELECT 1 FROM `{tabla}` WHERE `{columna}` = @Identificacion LIMIT 1)";
                var existe = await connection.ExecuteScalarAsync<bool>(
                    queryExiste,
                    new { Identificacion = identificacion }
                );

                if (existe)
                    return true;
            }

            // Verificar Bitacora (sin FK formal)
            var existeBitacora = await connection.ExecuteScalarAsync<bool>(
                "sp_ExisteEnBitacora",
                new { p_Identificacion = identificacion },
                commandType: CommandType.StoredProcedure
            );

            return existeBitacora;
        }

        public async Task<bool> EliminarAsync(string identificacion)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            var rowsAffected = await connection.ExecuteAsync(
                "sp_EliminarUsuario",
                new { p_Identificacion = identificacion },
                commandType: CommandType.StoredProcedure
            );
            return rowsAffected > 0;
        }

        public async Task ActualizarContrasenaAsync(string identificacion, string nuevaContrasena)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            await connection.ExecuteAsync(
                "sp_ActualizarContrasena",
                new { p_Identificacion = identificacion, p_Contrasena = nuevaContrasena },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<bool> TieneAccesoRutaAsync(string usuarioId, string ruta)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

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