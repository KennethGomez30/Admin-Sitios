using Sistema_Contable.Entities;
using Sistema_Contable.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Sistema_Contable.Services
{
    public class AutenticacionService : IAutenticacionService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IBitacoraRepository _bitacoraRepository;

        public AutenticacionService(IUsuarioRepository usuarioRepository, IBitacoraRepository bitacoraRepository)
        {
            _usuarioRepository = usuarioRepository;
            _bitacoraRepository = bitacoraRepository;
        }

        public async Task<ResultadoAutenticacion> AutenticarAsync(string identificacion, string contrasena)
        {
            try
            {
                // Validar credenciales proporcionadas
                if (string.IsNullOrWhiteSpace(identificacion) || string.IsNullOrWhiteSpace(contrasena))
                {
                    await RegistrarBitacoraAsync(null, "Intento de login sin credenciales completas");
                    return new ResultadoAutenticacion
                    {
                        Exitoso = false,
                        Mensaje = "Usuario y/o contraseña incorrectos."
                    };
                }

                // Obtener usuario
                var usuario = await _usuarioRepository.ObtenerPorIdentificacionAsync(identificacion);

                if (usuario == null)
                {
                    await RegistrarBitacoraAsync(identificacion, "Intento de login con usuario inexistente");
                    return new ResultadoAutenticacion
                    {
                        Exitoso = false,
                        Mensaje = "Usuario y/o contraseña incorrectos."
                    };
                }

                // Verificar si está bloqueado
                if (usuario.Estado == "Bloqueado" || usuario.IntentosLogin >= 3)
                {
                    await RegistrarBitacoraAsync(identificacion, "Intento de login con usuario bloqueado", usuario);
                    return new ResultadoAutenticacion
                    {
                        Exitoso = false,
                        Mensaje = "El usuario se encuentra bloqueado."
                    };
                }

                // Verificar contraseña
                var contrasenaHash = EncriptarMD5(contrasena);
                if (usuario.Contrasena != contrasenaHash)
                {
                    var nuevosIntentos = usuario.IntentosLogin + 1;

                    if (nuevosIntentos >= 3)
                    {
                        await _usuarioRepository.BloquearUsuarioAsync(identificacion);
                        await RegistrarBitacoraAsync(identificacion, "Usuario bloqueado por 3 intentos fallidos", usuario);
                    }
                    else
                    {
                        await _usuarioRepository.ActualizarIntentosLoginAsync(identificacion, nuevosIntentos);
                        await RegistrarBitacoraAsync(identificacion, $"Intento de login fallido ({nuevosIntentos}/3)", usuario);
                    }

                    return new ResultadoAutenticacion
                    {
                        Exitoso = false,
                        Mensaje = "Usuario y/o contraseña incorrectos."
                    };
                }

                // Verificar estado activo
                if (usuario.Estado != "Activo")
                {
                    await RegistrarBitacoraAsync(identificacion, "Intento de login con usuario inactivo", usuario);
                    return new ResultadoAutenticacion
                    {
                        Exitoso = false,
                        Mensaje = "El usuario se encuentra inactivo."
                    };
                }

                // Login exitoso
                await _usuarioRepository.ActualizarIntentosLoginAsync(identificacion, 0);
                await RegistrarBitacoraAsync(identificacion, "Login exitoso", usuario);

                return new ResultadoAutenticacion
                {
                    Exitoso = true,
                    Mensaje = "Login exitoso",
                    Usuario = usuario
                };
            }
            catch (Exception ex)
            {
                await RegistrarBitacoraAsync(identificacion, $"Error técnico en login: {ex.Message}");
                throw;
            }
        }

        private static string EncriptarMD5(string texto)
        {
            using var md5 = MD5.Create();
            var inputBytes = Encoding.UTF8.GetBytes(texto);
            var hashBytes = md5.ComputeHash(inputBytes);

            var sb = new StringBuilder();
            foreach (var b in hashBytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }

        private async Task RegistrarBitacoraAsync(string? usuario, string accion, Usuario? datosUsuario = null)
        {
            var descripcion = accion;
            if (datosUsuario != null)
            {
                var jsonUsuario = JsonSerializer.Serialize(new
                {
                    datosUsuario.Identificacion,
                    datosUsuario.Nombre,
                    datosUsuario.Apellido,
                    datosUsuario.Correo,
                    datosUsuario.Estado,
                    datosUsuario.IntentosLogin
                });
                descripcion += $" | Datos: {jsonUsuario}";
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
}
