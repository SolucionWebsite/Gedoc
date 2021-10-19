namespace Gedoc.Etl.Winsrv.Helpers
{
    public class ResultadoOperacion
    {
        public int Codigo { get; set; }
        public string Texto { get; set; }
        public string DataExtra { get; set; }

        public ResultadoOperacion()
        {
            Codigo = 0;
            Texto = "";
        }

        public ResultadoOperacion(int codigo, string texto)
        {
            Codigo = codigo;
            Texto = texto;
        }
    }
}