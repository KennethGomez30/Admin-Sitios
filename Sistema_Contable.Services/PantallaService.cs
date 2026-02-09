using Sistema_Contable.Entities;
using Sistema_Contable.Repository;
using System.Text.Json;

namespace Sistema_Contable.Services
{
    public class PantallaService : IPantallaService
    {
        private readonly IPantallaRepository _pantallaRepository;
        private readonly IBitacoraRepository _bitacoraRepository;

        public PantallaService(IPantallaRepository pantallaRepository, IBitacoraRepository bitacoraRepository)
        {
            _pantallaRepository = pantallaRepository;
            _bitacoraRepository = bitacoraRepository;
        }

        public async Task<(IEnumerable<Pantalla> Data, int Total)> ObtenerPaginadoAsync(int page, int pageSize, string? q, string? usuario)
        {
            try
            {
                await RegistrarBitacoraAsync(usuario, "El usuario consulta pantallas");

                var total = await _pantallaRepository.CountAsync(string.IsNullOrWhiteSpace(q) ? null : q);
                var data = await _pantallaRepository.GetPagedAsync(page, pageSize, string.IsNullOrWhiteSpace(q) ? null : q);

                return (data, total);
            }
            catch (Exception ex)
            {
                await RegistrarBitacoraAsync(usuario, $"Error técnico en consulta pantallas: {ex.Message}");
                throw;
            }
        }

        public async Task<Pantalla?> ObtenerPorIdAsync(ulong id, string? usuario)
        {
            try
            {
                await RegistrarBitacoraAsync(usuario, "El usuario consulta pantalla");
                return await _pantallaRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                await RegistrarBitacoraAsync(usuario, $"Error técnico en consulta pantalla: {ex.Message}");
                throw;
            }
        }

        public async Task<(bool Ok, string? Error, ulong? Id)> CrearAsync(Pantalla p, string? usuario)
        {
            try
            {
                // validar ruta única porque en la tabla de BD tiene un UNIQUE en ruta
                if (await _pantallaRepository.RutaExistsAsync(p.ruta))
                {
                    await RegistrarBitacoraAsync(usuario, "Crear pantalla fallido: ruta duplicada", p);
                    return (false, "La ruta ya existe.", null);
                }

                var id = await _pantallaRepository.CreateAsync(p);

                var creado = new
                {
                    pantalla_id = id,
                    p.nombre,
                    p.descripcion,
                    p.ruta,
                    p.estado
                };

                await RegistrarBitacoraAsync(usuario, "Crear pantalla", creado);

                return (true, null, id);
            }
            catch (Exception ex)
            {
                await RegistrarBitacoraAsync(usuario, $"Error técnico en crear pantalla: {ex.Message}");
                throw;
            }
        }

        public async Task<(bool Ok, string? Error)> ActualizarAsync(Pantalla p, string? usuario)
        {
            try
            {
                var before = await _pantallaRepository.GetByIdAsync(p.pantalla_id);
                if (before == null)
                {
                    await RegistrarBitacoraAsync(usuario, "Actualizar pantalla fallido: no existe", p);
                    return (false, "Registro no encontrado.");
                }

                if (await _pantallaRepository.RutaExistsAsync(p.ruta, p.pantalla_id))
                {
                    await RegistrarBitacoraAsync(usuario, "Actualizar pantalla fallido: ruta duplicada", p);
                    return (false, "La ruta ya existe.");
                }

                var ok = await _pantallaRepository.UpdateAsync(p);
                if (!ok)
                {
                    await RegistrarBitacoraAsync(usuario, "Actualizar pantalla fallido: no se pudo actualizar", p);
                    return (false, "No se pudo actualizar.");
                }

                await RegistrarBitacoraAsync(usuario, "Actualizar pantalla (BEFORE)", before);
                await RegistrarBitacoraAsync(usuario, "Actualizar pantalla (AFTER)", p);

                return (true, null);
            }
            catch (Exception ex)
            {
                await RegistrarBitacoraAsync(usuario, $"Error técnico en actualizar pantalla: {ex.Message}");
                throw;
            }
        }

        public async Task<(bool Ok, string? Error)> EliminarAsync(ulong id, string? usuario)
        {
            try
            {
                var before = await _pantallaRepository.GetByIdAsync(id);
                if (before == null)
                {
                    await RegistrarBitacoraAsync(usuario, "Eliminar pantalla fallido: no existe");
                    return (false, "Registro no encontrado.");
                }

                // solo se elimina si no está asignada
                if (await _pantallaRepository.IsAssignedAsync(id))
                {
                    // Mensaje exacto que se pide en el PDF
                    var msg = "No se puede eliminar un registro con datos relacionados.";
                    await RegistrarBitacoraAsync(usuario, $"Eliminar pantalla bloqueado: {msg}", before);
                    return (false, msg);
                }

                var ok = await _pantallaRepository.DeleteAsync(id);
                if (!ok)
                {
                    await RegistrarBitacoraAsync(usuario, "Eliminar pantalla fallido: no se pudo eliminar", before);
                    return (false, "No se pudo eliminar.");
                }

                await RegistrarBitacoraAsync(usuario, "Eliminar pantalla", before);
                return (true, null);
            }
            catch (Exception ex)
            {
                await RegistrarBitacoraAsync(usuario, $"Error técnico en eliminar pantalla: {ex.Message}");

                
                var msgLower = (ex.Message ?? "").ToLower();

                if (msgLower.Contains("foreign key") || msgLower.Contains("constraint fails"))
                {
                    return (false, "No se puede eliminar un registro con datos relacionados.");
                }

                return (false, "Ocurrió un error inesperado al eliminar la pantalla.");
            }
        }


        // Bitácora 
        private async Task RegistrarBitacoraAsync(string? usuario, string accion, object? datos = null)
        {
            var descripcion = accion;

            if (datos != null)
            {
                // JSON de la bitacora
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

        public async Task<IEnumerable<Pantalla>> ObtenerMenuPorUsuarioAsync(string usuarioId, string seccion)
        {
            try
            {
                // opcional: bitácora si querés
                // await RegistrarBitacoraAsync(usuarioId, "Consulta menú por usuario");

                return await _pantallaRepository.ObtenerMenuPorUsuarioAsync(usuarioId, seccion);
            }
            catch (Exception ex)
            {
                await RegistrarBitacoraAsync(usuarioId, $"Error técnico consultando menú: {ex.Message}");
                throw;
            }
        }
    }
}
