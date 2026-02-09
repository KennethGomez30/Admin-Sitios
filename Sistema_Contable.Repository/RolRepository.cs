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
    public class RolRepository : IRolRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public RolRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }
        
        public async Task<List<Rol>> ObtenerTodosAsync()
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            var result = await connection.QueryAsync<Rol>(
                "sp_ObtenerTodosRoles",
                commandType: CommandType.StoredProcedure
            );
            return result.ToList();
        }
        /*
        public async Task<Rol?> ObtenerPorIdAsync(int id)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Rol>(
                "sp_ObtenerRolPorId",
                new { p_Id = id },
                commandType: CommandType.StoredProcedure
            );
        }*/

        // ROLES
        public async Task<List<Rol>> ObtenerTodosAsync(string? filtroNombre = null)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            var sql = @"
            SELECT IdRol, Nombre, FechaCreacion, FechaModificacion
            FROM roles
            WHERE (@Filtro IS NULL OR Nombre LIKE CONCAT('%', @Filtro, '%'))
            ORDER BY Nombre;";

            var result = await connection.QueryAsync<Rol>(sql, new { Filtro = filtroNombre });
            return result.ToList();
        }

        public async Task<Rol?> ObtenerPorIdAsync(int id)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            var sql = @"
            SELECT IdRol, Nombre, FechaCreacion, FechaModificacion
            FROM roles
            WHERE IdRol = @Id;";

            return await connection.QueryFirstOrDefaultAsync<Rol>(sql, new { Id = id });
        }

        public async Task<int> CrearAsync(Rol rol)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            var sql = @"
            INSERT INTO roles (Nombre)
            VALUES (@Nombre);
            SELECT LAST_INSERT_ID();";

            return await connection.ExecuteScalarAsync<int>(sql, new { rol.Nombre });
        }

        public async Task<bool> ActualizarAsync(Rol rol)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            var sql = @"
            UPDATE roles
            SET Nombre = @Nombre
            WHERE IdRol = @IdRol;";

            var rows = await connection.ExecuteAsync(sql, new { rol.Nombre, rol.IdRol });
            return rows > 0;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            var sql = @"DELETE FROM roles WHERE IdRol = @Id;";
            var rows = await connection.ExecuteAsync(sql, new { Id = id });
            return rows > 0;
        }

        // VALIDACIONES
        public async Task<bool> ExisteNombreAsync(string nombre, int? excluirId = null)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            var sql = @"
            SELECT COUNT(1)
            FROM roles
            WHERE Nombre = @Nombre
              AND (@ExcluirId IS NULL OR IdRol <> @ExcluirId);";

            var count = await connection.ExecuteScalarAsync<int>(sql, new { Nombre = nombre, ExcluirId = excluirId });
            return count > 0;
        }

        public async Task<bool> TienePantallasAsignadasAsync(int idRol)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            var sql = @"
            SELECT COUNT(1)
            FROM rolpantalla
            WHERE IdRol = @IdRol;";

            var count = await connection.ExecuteScalarAsync<int>(sql, new { IdRol = idRol });
            return count > 0;
        }

        // PANTALLAS / ROLPANTALLA
        public async Task<List<Pantalla>> ObtenerPantallasActivasAsync()
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            var sql = @"
                SELECT 
                    pantalla_id,
                    nombre,
                    descripcion,
                    ruta,
                    estado
                FROM pantallas
                WHERE estado = 'Activa'
                ORDER BY nombre;";

            var result = await connection.QueryAsync<Pantalla>(sql);
            return result.ToList();
        }

        public async Task<List<long>> ObtenerPantallasIdsPorRolAsync(int idRol)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            var sql = @"
            SELECT pantalla_id
            FROM rolpantalla
            WHERE IdRol = @IdRol;";

            var result = await connection.QueryAsync<long>(sql, new { IdRol = idRol });
            return result.ToList();
        }

        public async Task ReemplazarPantallasDeRolAsync(int idRol, IEnumerable<long> pantallasIds)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            await connection.OpenAsync();

            using var tx = await connection.BeginTransactionAsync();

            try
            {
                // 1) borrar las asignaciones actuales
                var deleteSql = "DELETE FROM rolpantalla WHERE IdRol = @IdRol;";
                await connection.ExecuteAsync(deleteSql, new { IdRol = idRol }, tx);

                // 2) insertar las nuevas (si vienen)
                var lista = pantallasIds?.Distinct().ToList() ?? new List<long>();
                if (lista.Count > 0)
                {
                    var insertSql = @"
                    INSERT INTO rolpantalla (IdRol, pantalla_id)
                    VALUES (@IdRol, @PantallaId);";

                    var data = lista.Select(pid => new { IdRol = idRol, PantallaId = pid });
                    await connection.ExecuteAsync(insertSql, data, tx);
                }

                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> TieneUsuariosAsignadosAsync(int idRol)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            var sql = @"
                SELECT COUNT(1)
                FROM usuarioroles
                WHERE RolId = @RolId;";

            var count = await connection.ExecuteScalarAsync<int>(sql, new { RolId = idRol });
            return count > 0;
        }


    }// class
}// namespace