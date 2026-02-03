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
    public class BitacoraRepository : IBitacoraRepository
    {
        private readonly string _connectionString;

        public BitacoraRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task RegistrarAsync(Bitacora bitacora)
        {
            using var connection = new MySqlConnection(_connectionString);
            var query = @"INSERT INTO Bitacora (FechaBitacora, Usuario, Descripcion) 
                          VALUES (@FechaBitacora, @Usuario, @Descripcion)";

            await connection.ExecuteAsync(query, bitacora);
        }
    }
}
