using Dapper;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Sistema_Contable.Entities;

namespace Sistema_Contable.Repository
{
    public class PantallaRepository : IPantallaRepository
    {
        private readonly string _cs;

        public PantallaRepository(IConfiguration config)
        {
            _cs = config.GetConnectionString("DefaultConnection")!;
        }

        private MySqlConnection Conn() => new MySqlConnection(_cs);

        public async Task<int> CountAsync(string? q)
        {
            using var db = Conn();
            var sql = @"SELECT COUNT(*) FROM pantallas
                        WHERE (@q IS NULL OR nombre LIKE CONCAT('%',@q,'%') OR ruta LIKE CONCAT('%',@q,'%'))";
            return await db.ExecuteScalarAsync<int>(sql, new { q });
        }

        public async Task<IEnumerable<Pantalla>> GetPagedAsync(int page, int pageSize, string? q)
        {
            using var db = Conn();
            var offset = (page - 1) * pageSize;

            var sql = @"SELECT pantalla_id, nombre, descripcion, ruta, estado
                        FROM pantallas
                        WHERE (@q IS NULL OR nombre LIKE CONCAT('%',@q,'%') OR ruta LIKE CONCAT('%',@q,'%'))
                        ORDER BY pantalla_id DESC
                        LIMIT @pageSize OFFSET @offset";

            return await db.QueryAsync<Pantalla>(sql, new { q, pageSize, offset });
        }

        public async Task<Pantalla?> GetByIdAsync(ulong id)
        {
            using var db = Conn();
            return await db.QueryFirstOrDefaultAsync<Pantalla>(
                @"SELECT pantalla_id, nombre, descripcion, ruta, estado
                  FROM pantallas WHERE pantalla_id=@id", new { id });
        }

        public async Task<bool> RutaExistsAsync(string ruta, ulong? excludeId = null)
        {
            using var db = Conn();
            var sql = @"SELECT COUNT(*)
                        FROM pantallas
                        WHERE ruta=@ruta AND (@excludeId IS NULL OR pantalla_id <> @excludeId)";
            var c = await db.ExecuteScalarAsync<int>(sql, new { ruta, excludeId });
            return c > 0;
        }

        public async Task<ulong> CreateAsync(Pantalla p)
        {
            using var db = Conn();
            var sql = @"INSERT INTO pantallas(nombre, descripcion, ruta, estado)
                        VALUES (@nombre, @descripcion, @ruta, @estado);
                        SELECT LAST_INSERT_ID();";
            return await db.ExecuteScalarAsync<ulong>(sql, p);
        }

        public async Task<bool> UpdateAsync(Pantalla p)
        {
            using var db = Conn();
            var sql = @"UPDATE pantallas
                        SET nombre=@nombre, descripcion=@descripcion, ruta=@ruta, estado=@estado
                        WHERE pantalla_id=@pantalla_id";
            return (await db.ExecuteAsync(sql, p)) > 0;
        }

        public async Task<bool> IsAssignedAsync(ulong pantallaId)
        {
            using var db = Conn();
            var sql = @"SELECT COUNT(*) FROM rolpantalla WHERE pantalla_id=@pantallaId";
            return (await db.ExecuteScalarAsync<int>(sql, new { pantallaId })) > 0;
        }

        public async Task<bool> DeleteAsync(ulong pantallaId)
        {
            using var db = Conn();
            var sql = @"DELETE FROM pantallas WHERE pantalla_id=@pantallaId";
            return (await db.ExecuteAsync(sql, new { pantallaId })) > 0;
        }

        public async Task<IEnumerable<Pantalla>> ObtenerMenuPorUsuarioAsync(string usuarioId, string seccion)
        {
            using var db = Conn();

            var sql = @"
        SELECT DISTINCT p.pantalla_id, p.nombre, p.descripcion, p.ruta, p.estado
        FROM UsuarioRoles ur
        JOIN RolPantalla rp ON rp.IdRol = ur.RolId
        JOIN pantallas p ON p.pantalla_id = rp.pantalla_id
        WHERE ur.UsuarioIdentificacion = @usuarioId
          AND p.estado = 'Activa'
          AND p.mostrar_en_menu = 1
          AND p.menu_seccion = @seccion
        ORDER BY p.pantalla_id;
    ";

            return await db.QueryAsync<Pantalla>(sql, new { usuarioId, seccion });
        }
    }
}
