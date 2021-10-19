using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Security.Cryptography;
using System.Text;
using Gedoc.Interop.Wss.Helpers;

namespace Gedoc.Interop.Wss.Services
{
    public class SeguridadSrv
    {
        public static string ClaveSeguridad = ConfigurationManager.AppSettings["ClaveSeguridad"];
        private HashHelper hashHelper = new HashHelper();

        public bool CompruebaHash(string textoOrigen, string hashOrigen)
        {
            var hashCheck = GetHash(textoOrigen);
            return ComparaHash(hashOrigen, hashCheck);
        }

        public string GetHash(string source)
        {
            var hash = hashHelper.HashString(source); // source.GetHashCode().ToString(); // <-- Se genera el hash de la misma manera q se implementó en la app externa q consume este servicio  // = GetHashSha1(source);
            return hash;
        }

        private string GetHashMd5(string source)
        {
            var tmpSource = ASCIIEncoding.ASCII.GetBytes(source);
            var tmpHash = new MD5CryptoServiceProvider().ComputeHash(tmpSource);

            return ByteArrayToString(tmpHash);
        }

        private string GetHashSha1(string source)
        {
            using (SHA256 sha256Hash = SHA256.Create())  
            {  
                var tmpSource = Encoding.UTF8.GetBytes(source);
                var tmpHash = sha256Hash.ComputeHash(tmpSource);  
  
                return ByteArrayToString(tmpHash);
            }  
        }

        private static string ByteArrayToString(byte[] arrInput)
        {
            var sOutput = new StringBuilder(arrInput.Length);
            for (var i = 0; i < arrInput.Length - 1; i++)
            {
                sOutput.Append(arrInput[i].ToString("X2"));
            }
            return sOutput.ToString();
        }

        private bool ComparaHash(string hash1, string hash2)
        {
            if (string.IsNullOrWhiteSpace(hash1) || string.IsNullOrWhiteSpace(hash2))
            {
                return false;
            }

            return hash1 == hash2;
        }

    }
}