using System.Text.Json;
using System.Text.RegularExpressions;
using Sistema_Contable.Entities;
using Sistema_Contable.Repository;

namespace Sistema_Contable.Services
{
    public class CuentaContableService : ICuentaContableService
    {
        private readonly ICuentaContableRepository _repo;
        private readonly IBitacoraRepository _bitacoraRepository;

        // Validacion de formato de codigo crear cuentas contables
        private static readonly Regex CodigoRegex =
            new(@"^\d+(\.\d+)+$", RegexOptions.Compiled);

        private static readonly Regex NombreRegex =
            new(@"^[A-Za-zÁÉÍÓÚÜÑáéíóúüñ0-9 ]+$", RegexOptions.Compiled);

        private static readonly HashSet<string> TiposCuenta = new(StringComparer.OrdinalIgnoreCase)
        { "Activo", "Pasivo", "Capital", "Gasto", "Ingreso" };

        private static readonly HashSet<string> TiposSaldo = new(StringComparer.OrdinalIgnoreCase)
        { "deudor", "acreedor" };

        /// /////////////////////////////////////////////////////////////////////////////////////////
        public CuentaContableService(ICuentaContableRepository repo, IBitacoraRepository bitacoraRepository)
        {
            _repo = repo;
            _bitacoraRepository = bitacoraRepository;
        }

        public async Task<(IEnumerable<CuentaContable> Items, int Total)> ListarAsync(
            string? estado, int page, int pageSize, string? usuarioBitacora = null)
        {
            var (items, total) = await _repo.ListarAsync(estado, page, pageSize);

            await RegistrarBitacoraAsync(usuarioBitacora,
                "Consulta listado de cuentas contables",
                new { Estado = estado ?? "Todos", Page = page, PageSize = pageSize, Total = total });

            return (items, total);
        }

        public async Task<CuentaContable?> ObtenerAsync(int id, string? usuarioBitacora = null)
        {
            var cuenta = await _repo.ObtenerPorIdAsync(id);

            await RegistrarBitacoraAsync(usuarioBitacora,
                "Consulta detalle de cuenta contable",
                new { IdCuenta = id, Encontrada = cuenta != null });

            return cuenta;
        }

        public async Task<IEnumerable<(int Id, string Label)>> PadresAsync(int? excluirId, string? usuarioBitacora = null)
        {
            var padres = await _repo.ListarParaPadreAsync(excluirId);

            await RegistrarBitacoraAsync(usuarioBitacora,
                "Consulta lista de cuentas padre",
                new { ExcluirId = excluirId });

            return padres;
        }

        public async Task<(bool Ok, string Msg, int? IdNuevo)> CrearAsync(CuentaContable c, string usuario)
        {
            try
            {
                c = NormalizarEntrada(c);

                var (ok, msg) = await ValidarAsync(c, esEdicion: false);
                if (!ok)
                {
                    await RegistrarBitacoraAsync(usuario, "Fallo al crear cuenta contable (validación)", BitacoraCuenta(c, includeId: false));
                    return (false, msg, null);
                }

                c.UsuarioCreacion = usuario;

                if (c.IdCuentaPadre.HasValue)
                    await _repo.DesactivarAceptaMovimientoAsync(c.IdCuentaPadre.Value);

                var idNuevo = await _repo.CrearAsync(c);

                await RegistrarBitacoraAsync(usuario, "Crea cuenta contable",
                    BitacoraCuenta(new CuentaContable
                    {
                        IdCuenta = idNuevo,
                        Codigo = c.Codigo,
                        Nombre = c.Nombre,
                        Tipo = c.Tipo,
                        TipoSaldo = c.TipoSaldo,
                        AceptaMovimiento = c.AceptaMovimiento,
                        IdCuentaPadre = c.IdCuentaPadre,
                        Activo = c.Activo
                    }, includeId: true));

                return (true, "Cuenta contable creada correctamente.", idNuevo);
            }
            catch (Exception ex)
            {
                await RegistrarBitacoraAsync(usuario, $"Error técnico al crear cuenta contable: {ex.Message}",
                    BitacoraCuenta(c, includeId: false));
                throw;
            }
        }

        public async Task<(bool Ok, string Msg)> ActualizarAsync(CuentaContable c, string usuario)
        {
            try
            {
                c = NormalizarEntrada(c);

                var anterior = await _repo.ObtenerPorIdAsync(c.IdCuenta);
                if (anterior is null)
                {
                    await RegistrarBitacoraAsync(usuario, "Intento de actualizar cuenta contable inexistente",
                        new { IdCuenta = c.IdCuenta });
                    return (false, "Registro no encontrado.");
                }

                var (ok, msg) = await ValidarAsync(c, esEdicion: true);
                if (!ok)
                {
                    await RegistrarBitacoraAsync(usuario, "Fallo al actualizar cuenta contable (validación)",
                        new { Antes = BitacoraCuenta(anterior, includeId: true), Intento = BitacoraCuenta(c, includeId: true) });
                    return (false, msg);
                }

                // Regla: si tiene hijos => no acepta movimiento
                if (await _repo.TieneHijosAsync(c.IdCuenta))
                    c.AceptaMovimiento = false;

                c.UsuarioModificacion = usuario;

                if (c.IdCuentaPadre.HasValue)
                    await _repo.DesactivarAceptaMovimientoAsync(c.IdCuentaPadre.Value);

                var updated = await _repo.ActualizarAsync(c);
                if (!updated)
                {
                    await RegistrarBitacoraAsync(usuario, "No se pudo actualizar cuenta contable (repo)",
                        new { IdCuenta = c.IdCuenta });
                    return (false, "No se pudo actualizar el registro.");
                }

                await RegistrarBitacoraAsync(usuario, "Actualiza cuenta contable",
                    new
                    {
                        Antes = BitacoraCuenta(anterior, includeId: true),
                        Despues = BitacoraCuenta(c, includeId: true)
                    });

                return (true, "Cuenta contable actualizada correctamente.");
            }
            catch (Exception ex)
            {
                await RegistrarBitacoraAsync(usuario, $"Error técnico al actualizar cuenta contable: {ex.Message}",
                    new { IdCuenta = c.IdCuenta });
                throw;
            }
        }

        public async Task<(bool Ok, string Msg)> EliminarAsync(int id, string usuario)
        {
            try
            {
                var c = await _repo.ObtenerPorIdAsync(id);
                if (c is null)
                {
                    await RegistrarBitacoraAsync(usuario, "Intento de eliminar cuenta contable inexistente",
                        new { IdCuenta = id });
                    return (false, "Registro no encontrado.");
                }

                if (await _repo.TieneHijosAsync(id))
                {
                    await RegistrarBitacoraAsync(usuario, "Fallo al eliminar cuenta contable (tiene hijas)",
                        BitacoraCuenta(c, includeId: true));
                    return (false, "No se puede eliminar una cuenta que tiene cuentas hijas.(datos relacionados).");
                }

                if (await _repo.TieneRelacionadosAsync(id))
                {
                    await RegistrarBitacoraAsync(usuario, "Fallo al eliminar cuenta contable (datos relacionados)",
                        BitacoraCuenta(c, includeId: true));
                    return (false, "No se puede eliminar un registro con datos relacionados.");
                }

                var deleted = await _repo.EliminarAsync(id);
                if (!deleted)
                {
                    await RegistrarBitacoraAsync(usuario, "No se pudo eliminar cuenta contable (repo)",
                        new { IdCuenta = id });
                    return (false, "No se pudo eliminar el registro.");
                }

                await RegistrarBitacoraAsync(usuario, "Elimina cuenta contable", BitacoraCuenta(c, includeId: true));
                return (true, "Cuenta contable eliminada correctamente.");
            }
            catch (Exception ex)
            {
                await RegistrarBitacoraAsync(usuario, $"Error técnico al eliminar cuenta contable: {ex.Message}",
                    new { IdCuenta = id });
                throw;
            }
        }

        // Helpers internos
        private static CuentaContable NormalizarEntrada(CuentaContable c)
        {
            c.Codigo = (c.Codigo ?? "").Trim();
            c.Nombre = (c.Nombre ?? "").Trim();
            c.Nombre = Regex.Replace(c.Nombre, @"\s{2,}", " ");

            c.Tipo = (c.Tipo ?? "").Trim();
            c.TipoSaldo = (c.TipoSaldo ?? "").Trim();

            return c;
        }

        private async Task<(bool Ok, string Msg)> ValidarAsync(CuentaContable c, bool esEdicion)
        {
            // 1 Requeridos / longitudes (rápido y sin BD)
            if (string.IsNullOrWhiteSpace(c.Codigo))
                return (false, "El código es requerido.");

            if (c.Codigo.Length > 20)
                return (false, "El código no debe ser mayor a 20 caracteres.");

            if (string.IsNullOrWhiteSpace(c.Nombre))
                return (false, "El nombre es requerido.");

            if (c.Nombre.Length > 100)
                return (false, "El nombre no debe ser mayor a 100 caracteres.");

            // 2 Formato del nombre
            if (!NombreRegex.IsMatch(c.Nombre))
                return (false, "El nombre solo debe tener letras, números y espacios.");

            // 3 Catálogos (rápido)
            if (!TiposCuenta.Contains(c.Tipo))
                return (false, "El tipo de cuenta no es válido.");

            if (!TiposSaldo.Contains(c.TipoSaldo))
                return (false, "El tipo de saldo no es válido.");

            // 4 Formato del código 
            if (!c.IdCuentaPadre.HasValue)
            {
                // Nivel 1: solo números (1,2,3)
                if (!Regex.IsMatch(c.Codigo, @"^\d+$"))
                    return (false, "El código de cuenta padre de primer nivel debe contener solo números. Ejemplo: 1, 2, 3");
            }
            else
            {
                // Hijas: números con puntos (1.1, 1.1.1, 1.1.1.01)
                if (!CodigoRegex.IsMatch(c.Codigo))
                    return (false, "El código de cuenta hija debe tener el formato numérico separado por puntos. Ejemplo: 1.1, 1.1.1, 1.1.1.01");
            }

            // 5 Unicidad del código 
            var existe = await _repo.ObtenerPorCodigoAsync(c.Codigo);
            if (existe is not null && (!esEdicion || existe.IdCuenta != c.IdCuenta))
                return (false, "Ya existe una cuenta con ese código.");

            // 6 Validaciones de padre 
            if (c.IdCuentaPadre.HasValue)
            {
                // No puede ser su propio padre
                if (esEdicion && c.IdCuentaPadre.Value == c.IdCuenta)
                    return (false, "La cuenta padre no puede ser la misma cuenta.");

                var padre = await _repo.ObtenerPorIdAsync(c.IdCuentaPadre.Value);
                if (padre is null)
                    return (false, "La cuenta padre seleccionada no existe.");

                // Anti-ciclo simple (2 niveles)
                if (esEdicion && padre.IdCuentaPadre.HasValue && padre.IdCuentaPadre.Value == c.IdCuenta)
                    return (false, "No se puede asignar un padre que generaría un ciclo.");

                // Jerarquía del código: debe iniciar con "PADRE."
                if (!c.Codigo.StartsWith(padre.Codigo + "."))
                    return (false, $"El código de la cuenta hija debe iniciar con el código del padre ({padre.Codigo}).");
            }

            // 7  acepta movimiento solo si no tiene hijas (BD)
            if (esEdicion && c.AceptaMovimiento)
            {
                if (await _repo.TieneHijosAsync(c.IdCuenta))
                    return (false, "Una cuenta con cuentas hijas no puede aceptar movimiento.");
            }

            return (true, "");
        }


        // JSON limpio para bitácora (solo campos relevantes de la tabla bd)
        private static object BitacoraCuenta(CuentaContable c, bool includeId)
        {
            if (includeId)
            {
                return new
                {
                    c.IdCuenta,
                    c.Codigo,
                    c.Nombre,
                    c.Tipo,
                    c.TipoSaldo,
                    c.IdCuentaPadre,
                    c.AceptaMovimiento,
                    c.Activo
                };
            }

            return new
            {
                c.Codigo,
                c.Nombre,
                c.Tipo,
                c.TipoSaldo,
                c.IdCuentaPadre,
                c.AceptaMovimiento,
                c.Activo
            };
        }

        // Bitácora estilo AutenticacionService (texto + JSON opcional)
        private async Task RegistrarBitacoraAsync(string? usuario, string accion, object? datos = null)
        {
            var descripcion = accion;

            if (datos != null)
                descripcion += $" | Datos: {JsonSerializer.Serialize(datos)}";

            var bitacora = new Bitacora
            {
                FechaBitacora = DateTime.Now,
                Usuario = usuario,
                Descripcion = descripcion
            };

            await _bitacoraRepository.RegistrarAsync(bitacora);
        }
    } // class CuentaContableService
} // namespace Sistema_Contable.Services