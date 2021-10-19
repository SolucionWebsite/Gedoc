

//Tomado de https://gist.github.com/donpandix/045f865c3bf800893036

using System.Text.RegularExpressions;

namespace Gedoc.Interop.Wss.Helpers
{
    /// <summary>
    /// Validador de RUT Chileno
    /// Hace uso del algoritmo Modulo 11
    ///
    /// Chilean ID Number validator
    /// Use the algorithm called Module 11
    /// </summary>
    public class RutHelper {
		
        /// <summary>
        /// Metodo de validación de rut con digito verificador
        /// dentro de la cadena
        /// </summary>
        /// <param name="rut">string</param>
        /// <returns>booleano</returns>
        public static bool ValidaRut(string rut) {
            rut = rut.Replace(".", "").ToUpper();
            Regex expresion = new Regex("^([0-9]+-[0-9K])$");
            string dv = rut.Substring(rut.Length - 1, 1);
            if (!expresion.IsMatch(rut)) {
                return false;
            }
            char[] charCorte = { '-' };
            string[] rutTemp = rut.Split(charCorte);
            if (dv != Digito(int.Parse(rutTemp[0]))) {
                return false;
            }
            return true;
        }
	
	
        /// <summary>
        /// Método que valida el rut con el digito verificador
        /// por separado
        /// </summary>
        /// <param name="rut">integer</param>
        /// <param name="dv">char</param>
        /// <returns>booleano</returns>
        public static bool ValidaRut(string rut, string dv) {
            return ValidaRut(rut + "-" + dv);
        }
	
        /// <summary>
        /// método que calcula el digito verificador a partir
        /// de la mantisa del rut
        /// </summary>
        /// <param name="rut"></param>
        /// <returns></returns>
        public static string Digito(int rut) {
            int suma = 0;
            int multiplicador = 1;
            while (rut != 0) {
                multiplicador++;
                if (multiplicador == 8)
                    multiplicador = 2;
                suma += (rut % 10) * multiplicador;
                rut = rut / 10;
            }
            suma = 11 - (suma % 11);
            if (suma == 11)	{
                return "0";
            } else if (suma == 10) {
                return "K";
            } else {
                return suma.ToString();
            }
        }

        /// <summary>
        /// Formatea el rut especificado eliminando los puntos y poniendo un guión delante del dígito verificador para q quede
        /// de la forma 12312312-3
        /// </summary>
        /// <param name="rut"></param>
        /// <returns></returns>
        public static string FormateaRut(string rut)
        {
            if (rut.Length < 3)
            {
                return rut;
            }
            rut = rut.Replace(".", "");
            rut = rut.Replace("-", "");
            rut = rut.Insert(rut.Length - 1, "-");
            return rut;

        }

    }
}