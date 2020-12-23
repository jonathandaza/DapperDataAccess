using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Helper.Utilities.Extensions
{
    public static class StringExtension
    {
        /// <summary>
        /// Método para truncar una cadena de texto
        /// </summary>
        /// <param name="value">Cadena texto</param>
        /// <param name="maxLength">Máxima cantidad a truncar la cadena de texto</param>
        /// <returns>Devuleve la cadena de texto truncada</returns>
        public static string Truncate(this string value, int maxLength)
        {
            if (String.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        /// <summary>
        /// Encripta texto en Base 64 con UTF8
        /// </summary>
        /// <param name="value">Texto a encriptar</param>
        /// <returns>Objeto de tipo <see cref="string"/> Con el texto encriptado</returns>
        public static string EncriptarTextoEnUtf8(this string value)
        {
            const string clave = "#f2xT10#";
            string textoEncriptado;

            using (var DES = new DESCryptoServiceProvider())
            {
                DES.Key = Encoding.UTF8.GetBytes(clave);
                DES.IV = Encoding.UTF8.GetBytes(clave);
                ICryptoTransform cryptoTransform = DES.CreateEncryptor();
                byte[] textoEnBytes = Encoding.UTF8.GetBytes(value);

                using (var memoryStream = new MemoryStream())
                using (var cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(textoEnBytes, 0, textoEnBytes.Length);
                    cryptoStream.Close();
                    textoEncriptado = Convert.ToBase64String(memoryStream.ToArray());
                    memoryStream.Close();
                }
            }
            return textoEncriptado;
        }

        /// <summary>
        /// Desencripta texto de base 64 con UTF8
        /// </summary>
        /// <param name="value">Texto encriptado</param>
        /// <returns>Objeto de tipo <see cref="string"/> Con el texto desencriptado</returns>
        public static string DesencriptarTextoUtf8(this string value)
        {
            const string clave = "#f2xT10#";
            string textoOriginal;

            using (var DES = new DESCryptoServiceProvider())
            {
                DES.Key = Encoding.UTF8.GetBytes(clave);
                DES.IV = Encoding.UTF8.GetBytes(clave);
                ICryptoTransform cryptoTransform = DES.CreateDecryptor();
                byte[] textoEnBytes = Convert.FromBase64String(value);

                using (var memoryStream = new MemoryStream())
                using (var cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(textoEnBytes, 0, textoEnBytes.Length);
                    cryptoStream.Close();
                    textoOriginal = Encoding.UTF8.GetString(memoryStream.ToArray());
                    memoryStream.Close();
                }
            }
            return textoOriginal;
        }

        /// <summary>
        /// pasa un string separandolos por coma a una lista de strings
        /// </summary>
        /// <param name="texto"></param>
        /// <returns></returns>
        public static List<string> TextoAListaPorComas(this string texto)
        {
            List<string> result = texto.Split(',').ToList();
            return result;
        }
    }
}
