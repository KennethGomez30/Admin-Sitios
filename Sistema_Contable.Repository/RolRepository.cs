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

        public async Task<Rol?> ObtenerPorIdAsync(int id)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Rol>(
                "sp_ObtenerRolPorId",
                new { p_Id = id },
                commandType: CommandType.StoredProcedure
            );
        }
    }
}