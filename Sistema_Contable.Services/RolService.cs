using Sistema_Contable.Entities;
using Sistema_Contable.Repository;
using Sistema_Contable.Services;
using System.Text.Json;
using System.Text.RegularExpressions;

public class RolService : IRolService
{
    private readonly IRolRepository _rolRepository;
    private readonly IBitacoraRepository _bitacoraRepository;

    public RolService(IRolRepository rolRepository, IBitacoraRepository bitacoraRepository)
    {
        _rolRepository = rolRepository;
        _bitacoraRepository = bitacoraRepository;
    }

    public Task<List<Rol>> ObtenerTodosAsync(string? filtroNombre = null)
        => _rolRepository.ObtenerTodosAsync(filtroNombre);

    public Task<Rol?> ObtenerPorIdAsync(int id)
        => _rolRepository.ObtenerPorIdAsync(id);

    public Task<List<Pantalla>> ObtenerPantallasActivasAsync()
        => _rolRepository.ObtenerPantallasActivasAsync();

    public Task<List<long>> ObtenerPantallasIdsPorRolAsync(int idRol)
        => _rolRepository.ObtenerPantallasIdsPorRolAsync(idRol);

    public async Task<(bool Exitoso, string Mensaje, int? IdNuevo)> CrearAsync(string usuario, string nombre, List<long> pantallasIds)
    {
        try
        {
            var (ok, msg) = ValidarNombre(nombre);
            if (!ok) return (false, msg, null);

            var existe = await _rolRepository.ExisteNombreAsync(nombre);
            if (existe) return (false, "Ya existe un rol con ese nombre.", null);

            var nuevoId = await _rolRepository.CrearAsync(new Rol { Nombre = nombre.Trim() });

            await _rolRepository.ReemplazarPantallasDeRolAsync(nuevoId, pantallasIds ?? new List<long>());

            await RegistrarBitacoraAsync(usuario, "Crea rol", new
            {
                IdRol = nuevoId,
                Nombre = nombre.Trim(),
                Pantallas = pantallasIds ?? new List<long>()
            });

            return (true, "Rol creado correctamente.", nuevoId);
        }
        catch (Exception ex)
        {
            await RegistrarBitacoraAsync(usuario, $"Error técnico al crear rol: {ex.Message}");
            throw;
        }
    }

    public async Task<(bool Exitoso, string Mensaje)> ActualizarAsync(string usuario, int idRol, string nombre, List<long> pantallasIds)
    {
        try
        {
            if (idRol <= 0) return (false, "Rol inválido.");

            var rolActual = await _rolRepository.ObtenerPorIdAsync(idRol);
            if (rolActual is null) return (false, "Rol no encontrado.");

            // Capturar "antes" (para bitácora)
            var pantallasAntes = await _rolRepository.ObtenerPantallasIdsPorRolAsync(idRol);
            var antes = new
            {
                rolActual.IdRol,
                rolActual.Nombre,
                Pantallas = pantallasAntes
            };

            var (ok, msg) = ValidarNombre(nombre);
            if (!ok) return (false, msg);

            var existe = await _rolRepository.ExisteNombreAsync(nombre, excluirId: idRol);
            if (existe) return (false, "Ya existe un rol con ese nombre.");

            rolActual.Nombre = nombre.Trim();

            var actualizado = await _rolRepository.ActualizarAsync(rolActual);
            if (!actualizado) return (false, "No se pudo actualizar el rol.");

            await _rolRepository.ReemplazarPantallasDeRolAsync(idRol, pantallasIds ?? new List<long>());

            var despues = new
            {
                IdRol = idRol,
                Nombre = nombre.Trim(),
                Pantallas = pantallasIds ?? new List<long>()
            };

            await RegistrarBitacoraAsync(usuario, "Actualiza rol", new { Anterior = antes, Actual = despues });

            return (true, "Rol actualizado correctamente.");
        }
        catch (Exception ex)
        {
            await RegistrarBitacoraAsync(usuario, $"Error técnico al actualizar rol: {ex.Message}");
            throw;
        }
    }

    public async Task<(bool Exitoso, string Mensaje)> EliminarAsync(string usuario, int idRol)
    {
        try
        {
            if (idRol <= 0) return (false, "Rol inválido.");

            var existe = await _rolRepository.ObtenerPorIdAsync(idRol);
            if (existe is null) return (false, "Rol no encontrado.");

            var tieneUsuarios = await _rolRepository.TieneUsuariosAsignadosAsync(idRol);
            if (tieneUsuarios)
                return (false, "No se puede eliminar un registro con datos relacionados.");

            // Capturar pantallas antes de borrar (para bitácora)
            var pantallas = await _rolRepository.ObtenerPantallasIdsPorRolAsync(idRol);

            var eliminado = await _rolRepository.EliminarAsync(idRol);
            if (!eliminado) return (false, "No se pudo eliminar el rol.");

            await RegistrarBitacoraAsync(usuario, "Elimina rol", new
            {
                Eliminado = new { existe.IdRol, existe.Nombre, Pantallas = pantallas }
            });

            return (true, "Rol eliminado correctamente.");
        }
        catch (Exception ex)
        {
            await RegistrarBitacoraAsync(usuario, $"Error técnico al eliminar rol: {ex.Message}");
            throw;
        }
    }

    private static (bool Ok, string Mensaje) ValidarNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            return (false, "El nombre del rol es requerido.");

        var limpio = nombre.Trim();

        if (limpio.Length > 40)
            return (false, "El nombre del rol no debe ser mayor a 40 caracteres.");

        if (!Regex.IsMatch(limpio, @"^[A-Za-zÁÉÍÓÚÜÑáéíóúüñ\s]+$"))
            return (false, "El nombre del rol solo debe tener letras y espacios.");

        return (true, "");
    }

    // BITACORA
    private async Task RegistrarBitacoraAsync(string? usuario, string accion, object? datos = null)
    {
        var descripcion = accion;

        if (datos != null)
        {
            var json = JsonSerializer.Serialize(datos);
            descripcion += $" | Datos: {json}";
        }

        var bitacora = new Bitacora
        {
            FechaBitacora = DateTime.Now,
            Usuario = usuario,
            Descripcion = descripcion
        };

        await _bitacoraRepository.RegistrarAsync(bitacora);
    }
}
