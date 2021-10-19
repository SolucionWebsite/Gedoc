using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using Gedoc.Helpers;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace Gedoc.WebApp.Helpers
{
    public static class ExcelExportEPPlus
    {
        public static byte[] CreateExcelDocument(DataTable dt)
        {
            if (dt == null)
            {
                return null;
            }

            for (var i = dt.Columns.Count - 1; i >= 0; i--)
            {
                var dc = dt.Columns[i];
                if (dc.ColumnName.StartsWith("Hidden_"))
                    dt.Columns.Remove(dc);
            }

            System.IO.MemoryStream stream = new System.IO.MemoryStream();
            using (ExcelPackage pck = new ExcelPackage())
            {
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Hoja1");
                ws.Cells["A1"].LoadFromDataTable(dt, true);
                int colNumber = 0;
                foreach (DataColumn col in dt.Columns)
                {
                    colNumber++;
                    // Formato para campos fecha
                    if (col.DataType == typeof(DateTime) || col.DataType == typeof(DateTime?))
                    {
                        var format = col.ExtendedProperties.Contains("DateFormat")
                            ? col.ExtendedProperties["DateFormat"].ToString()
                            : "dd/mm/yyyy HH:mm";
                        ws.Column(colNumber).Style.Numberformat.Format = format;
                    }
                    // Ancho de columnas
                    ws.Column(colNumber).AutoFit(10, 50);

                }
                // Se colorea la primera fila q tiene el nombre de cada columna
                ws.Row(1).Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Row(1).Style.Fill.BackgroundColor.SetColor(Color.DarkGray);

                return pck.GetAsByteArray();
            }
        }

        public static DataTable ConvertToDataTable<T>(List<T> models)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);
            // _Se toman todas las propiedades del modelo y se adiciona al datatable una columna por cada propiedad
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                var descAttribute = prop.GetCustomAttributes(typeof(DescriptionAttribute), true);
                // El nombre de columna se toma del atributo Description si existe, sino se deja con el mismo nombre de la propiedad 
                var colName = descAttribute.Length > 0 ? ((DescriptionAttribute)descAttribute[0]).Description : prop.Name;
                if (Nullable.GetUnderlyingType(prop.PropertyType) != null)
                {
                    dataTable.Columns.Add(new DataColumn
                    {
                        ColumnName = colName,
                        DataType = Nullable.GetUnderlyingType(prop.PropertyType),
                        AllowDBNull = true
                    });
                }
                else
                {
                    dataTable.Columns.Add(colName, prop.PropertyType);
                }
            }

            // Se adicionan al datatable las filas y sus respectivos valores
            foreach (T item in models)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }

            return dataTable;
        }
    }

    //public class VistaGeneroItem
    //{
    //    [Description("Año y Mes")] public string AnnoMes { get; set; }
    //    [Description("Género")] public string RemitenteGenero { get; set; }
    //    [Description("Documento Ingreso")] public string DocumentoIngreso { get; set; }
    //    [Description("Fecha Ingreso")] public DateTime FechaIngreso { get; set; }
    //    [Description("Etiqueta")] public string EtiquetaTitulos { get; set; }
    //    [Description("Estado")] public string EstadoTitulo { get; set; }
    //    [Description("Nùmero Ingreso")] public int NumeroIngreso { get; set; }
    //    [Description("Cetagorìa MN")] public string CategoriaMonumentoNacTitulo { get; set; }
    //    [Description("Remitente")] public string RemitenteNombre { get; set; }
    //    [Description("Institución Remitente")] public string RemitenteInstitucion { get; set; }
    //    [Description("Materia")] public string Materia { get; set; }
    //    [Description("Unidad Técnica")] public string UtAsignadaTitulo { get; set; }
    //    [Description("Profional UT")] public string ProfesionalNombre { get; set; }
    //    [Description("Fecha Cierre")] public DateTime? Cierre { get; set; }
    //    [Description("Motivo Cierre")] public string MotivoCierreTitulo { get; set; }
    //    [Description("Comentario Cierre")] public string ComentarioCierre { get; set; }
    //    [Description("Cerrado Por")] public string CerradoPor { get; set; }
    //    [Description("Tipo de Ingreso")] public string TipoIngreso { get; set; }
    //    [Description("Número de Caso")] public int? CasoId { get; set; }
    //    [Description("Tipo de Trámite")] public string TipoTramiteTitulo { get; set; }

    //    [Description("Canal de Llegada del Trámite")]
    //    public string CanalLlegadaTramiteTitulo { get; set; }

    //    [Description("Denominación Oficial")] public string MonumentoNacionalDenominacionOficial { get; set; }
    //    [Description("Otras denominaciones")] public string MonumentoNacionalOtrasDenominaciones { get; set; }
    //    [Description("Nombre o uso actual")] public string MonumentoNacionalNombreUsoActual { get; set; }

    //    [Description("Referecia de localidad")]
    //    public string MonumentoNacionalReferenciaLocalidad { get; set; }

    //    [Description("Región")] public string MonumentoNacionalRegionTitulo { get; set; }
    //    [Description("Comuna")] public string MonumentoNacionalComunaTitulo { get; set; }
    //}
}