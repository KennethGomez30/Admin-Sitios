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
    public class BitacoraRepository : IBitacoraRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public BitacoraRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task RegistrarAsync(Bitacora bitacora)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            await connection.ExecuteAsync(
                "sp_RegistrarBitacora",
                new
                {
                    p_FechaBitacora = bitacora.FechaBitacora,
                    p_Usuario = bitacora.Usuario,
                    p_Descripcion = bitacora.Descripcion
                },
                commandType: CommandType.StoredProcedure
            );
        }
    }
}