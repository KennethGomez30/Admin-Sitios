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
    public class RolRepository : IRolRepository
    {
        private readonly string _connectionString;

        public RolRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<List<Rol>> ObtenerTodosAsync()
        {
            using var connection = new MySqlConnection(_connectionString);
            var query = "SELECT IdRol, Nombre, FechaCreacion, FechaModificacion FROM Roles ORDER BY Nombre";
            var result = await connection.QueryAsync<Rol>(query);
            return result.ToList();
        }

        public async Task<Rol?> ObtenerPorIdAsync(int id)
        {
            using var connection = new MySqlConnection(_connectionString);
            var query = "SELECT IdRol, Nombre, FechaCreacion, FechaModificacion FROM Roles WHERE IdRol = @Id";
            return await connection.QueryFirstOrDefaultAsync<Rol>(query, new { Id = id });
        }
    }
}
