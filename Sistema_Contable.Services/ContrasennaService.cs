using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Sistema_Contable.Services
{
    public class ContrasennaService
    {
        private const string Letras = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        private const string Numeros = "0123456789";
        private const string Simbolos = "+-*$.";


        public static string GenerarContrasena()
        {
            var random = new Random();
            var longitud = random.Next(10, 13); // 10 a 12 caracteres
            var password = new StringBuilder();

            // Primer carácter DEBE ser una letra
            password.Append(Letras[random.Next(Letras.Length)]);

            // Asegurar que tenga al menos un número y un símbolo
            var caracteresRestantes = longitud - 3; // -1 por la letra inicial, -1 por número obligatorio, -1 por símbolo obligatorio

            // Agregar un número obligatorio
            password.Append(Numeros[random.Next(Numeros.Length)]);

            password.Append(Simbolos[random.Next(Simbolos.Length)]);

            var todosCaracteres = Letras + Numeros + Simbolos;
            for (int i = 0; i < caracteresRestantes; i++)
            {
                password.Append(todosCaracteres[random.Next(todosCaracteres.Length)]);
            }

            var chars = password.ToString().ToCharArray();
            var primeraLetra = chars[0];
            var restoChars = chars.Skip(1).ToArray();

            for (int i = restoChars.Length - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                var temp = restoChars[i];
                restoChars[i] = restoChars[j];
                restoChars[j] = temp;
            }

            // Reconstruir primera letra + resto mezclado
            return primeraLetra + new string(restoChars);
        }

        public static string EncriptarMD5(string texto)
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
    }
}