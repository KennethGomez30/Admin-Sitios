using Dapper;
using System.Data;
using Sistema_Contable.Entities;

namespace Sistema_Contable.Repository;

public class CuentaContableRepository : ICuentaContableRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public CuentaContableRepository(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<(IEnumerable<CuentaContable> Items, int Total)> ListarAsync(
        string? filtroEstado, int page, int pageSize)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        var offset = (page - 1) * pageSize;

        string where = "";
        object param;

        if (!string.IsNullOrWhiteSpace(filtroEstado))
        {
            var activo = filtroEstado.Trim().ToLower() == "activa";
            where = " WHERE c.activo = @Activo ";
            param = new { Activo = activo, PageSize = pageSize, Offset = offset };
        }
        else
        {
            param = new { PageSize = pageSize, Offset = offset };
        }

        var sql = $@"
                    SELECT c.id_cuenta   AS IdCuenta,
                           c.codigo     AS Codigo,
                           c.nombre     AS Nombre,
                           c.tipo       AS Tipo,
                           c.tipo_saldo AS TipoSaldo,
                           c.acepta_movimiento AS AceptaMovimiento,
                           c.id_cuenta_padre AS IdCuentaPadre,
                           c.activo     AS Activo,
                           c.fecha_creacion AS FechaCreacion,
                           c.fecha_modificacion AS FechaModificacion,
                           c.usuario_creacion AS UsuarioCreacion,
                           c.usuario_modificacion AS UsuarioModificacion,
                           p.codigo AS CuentaPadreCodigo,
                           p.nombre AS CuentaPadreNombre
                    FROM cuenta_contable c
                    LEFT JOIN cuenta_contable p ON p.id_cuenta = c.id_cuenta_padre
                    {where}
                    ORDER BY c.codigo
                    LIMIT @PageSize OFFSET @Offset;

                    SELECT COUNT(1)
                    FROM cuenta_contable c
                    {where};";

        using var multi = await connection.QueryMultipleAsync(sql, param);

        var items = await multi.ReadAsync<CuentaContable>();
        var total = await multi.ReadSingleAsync<int>();

        return (items, total);
    }

    public async Task<CuentaContable?> ObtenerPorIdAsync(int id)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        var sql = @"
                    SELECT id_cuenta AS IdCuenta,
                           codigo AS Codigo,
                           nombre AS Nombre,
                           tipo AS Tipo,
                           tipo_saldo AS TipoSaldo,
                           acepta_movimiento AS AceptaMovimiento,
                           id_cuenta_padre AS IdCuentaPadre,
                           activo AS Activo,
                           fecha_creacion AS FechaCreacion,
                           fecha_modificacion AS FechaModificacion,
                           usuario_creacion AS UsuarioCreacion,
                           usuario_modificacion AS UsuarioModificacion
                    FROM cuenta_contable
                    WHERE id_cuenta = @Id;";

        return await connection.QueryFirstOrDefaultAsync<CuentaContable>(sql, new { Id = id });
    }

    public async Task<CuentaContable?> ObtenerPorCodigoAsync(string codigo)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        var sql = @"
                    SELECT id_cuenta AS IdCuenta,
                           codigo AS Codigo,
                           nombre AS Nombre,
                           tipo AS Tipo,
                           tipo_saldo AS TipoSaldo,
                           acepta_movimiento AS AceptaMovimiento,
                           id_cuenta_padre AS IdCuentaPadre,
                           activo AS Activo,
                           fecha_creacion AS FechaCreacion,
                           fecha_modificacion AS FechaModificacion,
                           usuario_creacion AS UsuarioCreacion,
                           usuario_modificacion AS UsuarioModificacion
                    FROM cuenta_contable
                    WHERE codigo = @Codigo;";

        return await connection.QueryFirstOrDefaultAsync<CuentaContable>(sql, new { Codigo = codigo });
    }

    public async Task<int> CrearAsync(CuentaContable c)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        var sql = @"
                    INSERT INTO cuenta_contable
                    (codigo, nombre, tipo, tipo_saldo, acepta_movimiento, id_cuenta_padre, activo, usuario_creacion)
                    VALUES
                    (@Codigo, @Nombre, @Tipo, @TipoSaldo, @AceptaMovimiento, @IdCuentaPadre, @Activo, @UsuarioCreacion);
                    SELECT LAST_INSERT_ID();";

        return await connection.ExecuteScalarAsync<int>(sql, new
        {
            c.Codigo,
            c.Nombre,
            c.Tipo,
            c.TipoSaldo,
            AceptaMovimiento = c.AceptaMovimiento ? 1 : 0,
            c.IdCuentaPadre,
            Activo = c.Activo ? 1 : 0,
            c.UsuarioCreacion
        });
    }

    public async Task<bool> ActualizarAsync(CuentaContable c)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        var sql = @"
                    UPDATE cuenta_contable
                    SET codigo = @Codigo,
                        nombre = @Nombre,
                        tipo = @Tipo,
                        tipo_saldo = @TipoSaldo,
                        acepta_movimiento = @AceptaMovimiento,
                        id_cuenta_padre = @IdCuentaPadre,
                        activo = @Activo,
                        usuario_modificacion = @UsuarioModificacion
                    WHERE id_cuenta = @IdCuenta;";

        var rows = await connection.ExecuteAsync(sql, new
        {
            c.Codigo,
            c.Nombre,
            c.Tipo,
            c.TipoSaldo,
            AceptaMovimiento = c.AceptaMovimiento ? 1 : 0,
            c.IdCuentaPadre,
            Activo = c.Activo ? 1 : 0,
            c.UsuarioModificacion,
            c.IdCuenta
        });

        return rows > 0;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        var sql = "DELETE FROM cuenta_contable WHERE id_cuenta = @Id;";
        var rows = await connection.ExecuteAsync(sql, new { Id = id });

        return rows > 0;
    }

    public async Task<bool> TieneHijosAsync(int idCuenta)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        var sql = "SELECT COUNT(1) FROM cuenta_contable WHERE id_cuenta_padre = @Id;";
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Id = idCuenta });

        return count > 0;
    }

    public async Task<bool> TieneRelacionadosAsync(int idCuenta)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        // asiento_detalle.id_cuenta (o cuenta_id).
        var sql = @"
                    SELECT
                      (SELECT COUNT(1) FROM asiento_detalle WHERE cuenta_id = @Id) +
                      (SELECT COUNT(1) FROM saldos_cuentas_periodo WHERE cuenta_id = @Id);";
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Id = idCuenta });

        return count > 0;
    }

    public async Task<IEnumerable<(int Id, string Label)>> ListarParaPadreAsync(int? excluirId)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        var sql = @"
                    SELECT id_cuenta AS Id,
                           CONCAT(codigo,' - ',nombre) AS Label
                    FROM cuenta_contable
                    WHERE (@Excluir IS NULL OR id_cuenta <> @Excluir)
                    ORDER BY codigo;";

        return await connection.QueryAsync<(int, string)>(sql, new { Excluir = excluirId });
    }

    public async Task<bool> DesactivarAceptaMovimientoAsync(int idCuentaPadre)
    {
        using var connection = _dbConnectionFactory.CreateConnection();

        var sql = @"
                    UPDATE cuenta_contable
                    SET acepta_movimiento = 0
                    WHERE id_cuenta = @Id;";

        var rows = await connection.ExecuteAsync(sql, new { Id = idCuentaPadre });
        return rows > 0;
    }
}
