using Sistema_Contable.Entities;
using Sistema_Contable.Repository;
using System.Text.Json;

namespace Sistema_Contable.Services
{
    public class CierreContableService : ICierreContableService
    {
        private readonly ICierreContableRepository _repo;
        private readonly IBitacoraRepository _bitacora;

        public CierreContableService(ICierreContableRepository repo, IBitacoraRepository bitacora)
        {
            _repo = repo;
            _bitacora = bitacora;
        }

        public Task<List<(ulong periodo_id, int anio, int mes, string estado)>> ObtenerPeriodosAsync()
            => _repo.ObtenerPeriodosAsync();

        public async Task<(bool Ok, string Msg, CierreContableResultado? Resultado)> EjecutarCierreAsync(ulong periodoId, string usuario)
        {
            try
            {
                await LogAsync(usuario, "El usuario ejecuta cierre contable (ADM14)", new { periodoId });

                var periodo = await _repo.ObtenerPeriodoPorIdAsync(periodoId);
                if (periodo == null)
                {
                    await LogAsync(usuario, "Cierre contable fallido: periodo no existe", new { periodoId });
                    return (false, "Periodo no encontrado.", null);
                }

                if (!string.Equals(periodo.Value.estado, "ABIERTO", StringComparison.OrdinalIgnoreCase))
                {
                    await LogAsync(usuario, "Cierre contable bloqueado: periodo no está abierto", periodo);
                    return (false, "Solo se puede cerrar un periodo en estado ABIERTO.", null);
                }

                // Regla: solo cerrar si NO hay periodos anteriores abiertos
                if (await _repo.ExistenPeriodosAnterioresAbiertosAsync(periodoId))
                {
                    await LogAsync(usuario, "Cierre contable bloqueado: existen periodos anteriores abiertos", periodo);
                    return (false, "No se puede cerrar: existen periodos anteriores abiertos.", null);
                }

                // Determinar periodo anterior (anio/mes)
                // Buscamos en lista por orden desc y tomamos el siguiente “anterior” a este.
                var periodos = await _repo.ObtenerPeriodosAsync();
                var ordenados = periodos.OrderByDescending(p => p.anio).ThenByDescending(p => p.mes).ToList();
                var idx = ordenados.FindIndex(p => p.periodo_id == periodoId);
                ulong periodoAnteriorId = 0;
                if (idx >= 0 && idx + 1 < ordenados.Count)
                    periodoAnteriorId = ordenados[idx + 1].periodo_id; // “siguiente” en lista desc = anterior en el tiempo

                var cuentas = await _repo.ObtenerCuentasAsync();

                var resultado = new CierreContableResultado
                {
                    PeriodoId = periodo.Value.periodo_id,
                    Anio = periodo.Value.anio,
                    Mes = periodo.Value.mes
                };

                foreach (var c in cuentas)
                {
                    var saldoAnterior = periodoAnteriorId == 0 ? 0m : await _repo.ObtenerSaldoAnteriorAsync(periodoAnteriorId, c.id_cuenta);
                    var (movDebe, movHaber) = await _repo.ObtenerMovimientosMesAsync(periodoId, c.id_cuenta);

                    // “Sumar o restar según naturaleza (tipo_saldo)”
                    // deudor: saldo + debe - haber
                    // acreedor: saldo - debe + haber
                    decimal saldoNuevo = c.tipo_saldo.Equals("deudor", StringComparison.OrdinalIgnoreCase)
                        ? (saldoAnterior + movDebe - movHaber)
                        : (saldoAnterior - movDebe + movHaber);

                    var linea = new CierreContableLinea
                    {
                        CuentaId = c.id_cuenta,
                        CodigoCuenta = c.codigo,
                        NombreCuenta = c.nombre,
                        TipoSaldo = c.tipo_saldo,
                        SaldoAnterior = saldoAnterior,
                        MovDebe = movDebe,
                        MovHaber = movHaber,
                        SaldoNuevo = saldoNuevo
                    };

                    resultado.Lineas.Add(linea);

                    // Totales Debe/Haber por naturaleza del saldo (para demostrar igualdad)
                    if (c.tipo_saldo.Equals("deudor", StringComparison.OrdinalIgnoreCase))
                        resultado.TotalDebe += saldoNuevo;
                    else
                        resultado.TotalHaber += saldoNuevo;
                }

                // Regla: al sumar saldos por naturaleza, Debe y Haber deben ser iguales
                if (!resultado.Balanceado)
                {
                    await LogAsync(usuario, "Cierre contable fallido: no balancea Debe vs Haber", new
                    {
                        periodoId,
                        resultado.TotalDebe,
                        resultado.TotalHaber
                    });

                    return (false, $"El cierre NO balancea. Debe={resultado.TotalDebe:0.00} Haber={resultado.TotalHaber:0.00}", resultado);
                }

                // Persistir saldos (saldos_cuentas_periodo)
                foreach (var l in resultado.Lineas)
                    await _repo.UpsertSaldoCuentaPeriodoAsync(periodoId, l.CuentaId, l.SaldoNuevo);

                // Cerrar periodo (si fue exitoso)
                await _repo.CerrarPeriodoAsync(periodoId, usuario);

                await LogAsync(usuario, "Cierre contable exitoso: se guardan saldos y se cierra periodo", new
                {
                    periodoId,
                    resultado.TotalDebe,
                    resultado.TotalHaber,
                    lineas = resultado.Lineas.Count
                });

                return (true, "Cierre contable ejecutado correctamente.", resultado);
            }
            catch (Exception ex)
            {
                await LogAsync(usuario, $"Error técnico en cierre contable: {ex.Message}", new { periodoId });
                return (false, "Ocurrió un error técnico al ejecutar el cierre.", null);
            }
        }

        private async Task LogAsync(string? usuario, string accion, object? datos = null)
        {
            var desc = accion;
            if (datos != null)
                desc += " | Datos: " + JsonSerializer.Serialize(datos);

            await _bitacora.RegistrarAsync(new Entities.Bitacora
            {
                FechaBitacora = DateTime.Now,
                Usuario = usuario,
                Descripcion = desc
            });
        }
    }
}
