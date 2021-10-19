using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Gedoc.Helpers;
using Kendo.Mvc;

namespace Gedoc.WebApp.Helpers
{
    public class KendoHelper
    {
        private static readonly string PdfOficioPaperSize =
            string.IsNullOrWhiteSpace(WebConfigValues.PdfOficioPaperSize) ? "A4" : WebConfigValues.PdfOficioPaperSize;
        private static readonly string PdfOficioMargenSup =
            string.IsNullOrWhiteSpace(WebConfigValues.PdfOficioMargenSup) ? "0.75in" : WebConfigValues.PdfOficioMargenSup;
        private static readonly string PdfOficioMargenDer =
            string.IsNullOrWhiteSpace(WebConfigValues.PdfOficioMargenDer) ? "0.75in" : WebConfigValues.PdfOficioMargenDer;
        private static readonly string PdfOficioMargenInf =
            string.IsNullOrWhiteSpace(WebConfigValues.PdfOficioMargenInf) ? "0.75in" : WebConfigValues.PdfOficioMargenInf;
        private static readonly string PdfOficioMargenIzq =
            string.IsNullOrWhiteSpace(WebConfigValues.PdfOficioMargenIzq) ? "0.75in" : WebConfigValues.PdfOficioMargenIzq;

        /// <summary>
        /// Permite convertir los filtros aplicados en la grila de Kendo  a filtros con formato de consulta de SQL o de consulta Dynamic LINQ.
        /// Por ej., el filtro
        ///
        /// "(DiasResolucion~eq~23~or~DiasResolucion~eq~24)~and~Resolucion~isnotnull~datetime'2020-08-09T00-00-00'"
        ///
        /// lo transforma a
        ///
        /// "((((((DiasResolucion = @0) or (DiasResolucion = @1))) and (Resolucion != NULL))))"
        ///
        /// Ref. https://www.telerik.com/forums/converting-datasourcerequest-filters-to-sqlserver-parameterized-query-in-controller-read-method
        /// </summary>
        /// <param name="filters">String q contiene los filtros aplicados en la grila. Por ej. "ID~eq~1"</param>
        /// <param name="compositionOperator">Operación AND o OR a aplicar. Pro defecto se toma AND</param>
        /// <param name="paramValues">Opcional. Si se especifica entonces en el filtro SQL resultante los valores se especifican
        /// como parametros y el valor de vada parametro se almacena en esta lista. Si no se especificar entonces en el filtro
        /// SQL resultante se especifican directamente los valores de cada campo</param>
        /// <returns>String con el filtro SQL para aplicar directamente en el WHERE de una consulta.</returns>
        public static string FiltersToParameterizedQuery(IList<IFilterDescriptor> filters, 
            FilterCompositionLogicalOperator compositionOperator = FilterCompositionLogicalOperator.And, 
            List<object> paramValues = null)
        {

            if (filters == null ||!filters.Any()) return "";

            string result = "(";
            string combineWith = "";

            foreach (var filter in filters)
            {
                if (filter is FilterDescriptor fd)
                {
                    result +=
                        combineWith + "("
                                    + DescriptorToLinqQuery(fd, paramValues) // Convertir a formato de Dynamic LINQ
                                    + ")"
                        ;
                }
                else if (filter is CompositeFilterDescriptor cfd)
                {
                    result +=
                        combineWith + "("
                                    + FiltersToParameterizedQuery(cfd.FilterDescriptors, cfd.LogicalOperator, paramValues)
                                    + ")"
                        ;
                }

                combineWith =
                    (compositionOperator == FilterCompositionLogicalOperator.And)
                        ? " and "
                        : " or "
                    ;
            }

            result += ")";
            return result;
        }

        private static string DescriptorToSqlServerQuery(FilterDescriptor fd, List<object> paramValues)
        {
            bool sinCorchete = true;
            string parameterName = "@" + (paramValues == null ? "PARAMETER0" : (paramValues?.Count ?? 0).ToString());
            string result;

            // Some string filter values are modified for use as parameteres in a SQL LIKE clause, thus work with a copy.
            // The original value must remain unchanged for when ToDataSourceResult(request) is used later.

            Object filterValue = fd.Value;

            switch (fd.Operator)
            {
                case FilterOperator.IsLessThan: result = "[" + fd.Member + "]" + " < " + parameterName; break;
                case FilterOperator.IsLessThanOrEqualTo: result = "[" + fd.Member + "]" + " <= " + parameterName; break;
                case FilterOperator.IsEqualTo: result = "[" + fd.Member + "]" + " = " + parameterName; break;
                case FilterOperator.IsNotEqualTo: result = "[" + fd.Member + "]" + " <> " + parameterName; break;
                case FilterOperator.IsGreaterThanOrEqualTo: result = "[" + fd.Member + "]" + " >= " + parameterName; break;
                case FilterOperator.IsGreaterThan: result = "[" + fd.Member + "]" + " > " + parameterName; break;
                case FilterOperator.StartsWith:
                    filterValue = fd.Value.ToString().ToSqlSafeLikeData() + "%";
                    result = "[" + fd.Member + "]" + " like " + parameterName; break;
                case FilterOperator.EndsWith:
                    filterValue = "%" + fd.Value.ToString().ToSqlSafeLikeData();
                    result = "[" + fd.Member + "]" + " like " + parameterName; break;
                case FilterOperator.Contains:
                    filterValue = "%" + fd.Value.ToString().ToSqlSafeLikeData() + "%";
                    result = "[" + fd.Member + "]" + " like " + parameterName; break;
                case FilterOperator.IsContainedIn:
                    throw new Exception("There is no translator for [" + fd.Member + "]" + " " + fd.Operator + " " + fd.Value);
                case FilterOperator.DoesNotContain:
                    filterValue = "%" + fd.Value.ToString().ToSqlSafeLikeData();
                    result = "[" + fd.Member + "]" + " not like " + parameterName; break;
                case FilterOperator.IsNull: result = "[" + fd.Member + "]" + " IS NULL"; break;
                case FilterOperator.IsNotNull: result = "[" + fd.Member + "]" + " IS NOT NULL"; break;
                case FilterOperator.IsEmpty: result = "[" + fd.Member + "]" + " = ''"; break;
                case FilterOperator.IsNotEmpty: result = "[" + fd.Member + "]" + " <> ''"; break;
                default:
                    throw new Exception("There is no translator for [" + fd.Member + "]" + " " + fd.Operator + " " + fd.Value);
            }
            if (sinCorchete)
            {
                result = result.Replace("[" + fd.Member + "]", fd.Member);
            }

            if (paramValues != null)
            {
                paramValues.Add(filterValue);
            }
            else
            {
                result = result.Replace(parameterName, (filterValue ?? "").ToString());
            }

            return result;
        }

        private static string DescriptorToLinqQuery(FilterDescriptor fd, List<object> paramValues)
        {
            string parameterName = "@" + (paramValues == null ? "PARAMETER0" : (paramValues?.Count ?? 0).ToString());
            string result;

            // Some string filter values are modified for use as parameteres in a SQL LIKE clause, thus work with a copy.
            // The original value must remain unchanged for when ToDataSourceResult(request) is used later.

            Object filterValue = fd.Value;

            switch (fd.Operator)
            {
                case FilterOperator.IsLessThan: result = fd.Member + " < " + parameterName; break;
                case FilterOperator.IsLessThanOrEqualTo: result = fd.Member + " <= " + parameterName; break;
                case FilterOperator.IsEqualTo: result = fd.Member + " = " + parameterName; break;
                case FilterOperator.IsNotEqualTo: result = fd.Member + " <> " + parameterName; break;
                case FilterOperator.IsGreaterThanOrEqualTo: result = fd.Member + " >= " + parameterName; break;
                case FilterOperator.IsGreaterThan: result = fd.Member + " > " + parameterName; break;
                case FilterOperator.StartsWith: // TODO: implementar para Dynamic LINQ
                    filterValue = fd.Value.ToString().ToSqlSafeLikeData() + "%";
                    result = fd.Member + " like " + parameterName; break;
                case FilterOperator.EndsWith: // TODO: implementar para Dynamic LINQ
                    filterValue = "%" + fd.Value.ToString().ToSqlSafeLikeData();
                    result = fd.Member + " like " + parameterName; break;
                case FilterOperator.Contains: // TODO: implementar para Dynamic LINQ
                    filterValue = "%" + fd.Value.ToString().ToSqlSafeLikeData() + "%";
                    result = fd.Member + " like " + parameterName; break;
                case FilterOperator.IsContainedIn: // TODO: implementar para Dynamic LINQ
                    throw new Exception("There is no translator for [" + fd.Member + " " + fd.Operator + " " + fd.Value);
                case FilterOperator.DoesNotContain: // TODO: implementar para Dynamic LINQ
                    filterValue = "%" + fd.Value.ToString().ToSqlSafeLikeData();
                    result = fd.Member + " not like " + parameterName; break;
                case FilterOperator.IsNull: result = fd.Member + " == NULL"; break;
                case FilterOperator.IsNotNull: result = fd.Member + " != NULL"; break;
                case FilterOperator.IsEmpty: result = fd.Member + " = \"\""; break;
                case FilterOperator.IsNotEmpty: result = fd.Member + " <> \"\""; break;
                default:
                    throw new Exception("There is no translator for [" + fd.Member + " " + fd.Operator + " " + fd.Value);
            }

            if (paramValues != null)
            {
                paramValues.Add(filterValue);
            }
            else
            {
                var valor = (filterValue ?? "").ToString(); 
                if (fd.ConvertedValue is DateTime valorFecha)
                {
                    valor = $"DateTime({valorFecha.Year},{valorFecha.Month},{valorFecha.Day},{valorFecha.Hour},{valorFecha.Minute},{valorFecha.Second})";
                }
                if (fd.ConvertedValue is String)
                {
                    valor = $"\"{valor}\"";
                }
                result = result.Replace(parameterName, valor);
            }

            return result;
        }

        public static void SetDropDownListMessages(Kendo.Mvc.UI.Fluent.DropDownListMessagesSettingsBuilder m)
        {
            m.NoData("No se encontraron datos");
        }

        public static void SetComboBoxMessages(Kendo.Mvc.UI.Fluent.MessagesSettingsBuilder m)
        {
            m.NoData("No se encontraron datos");
            m.Clear("limpiar");
        }

        public static void SetMultiSelectMessages(Kendo.Mvc.UI.Fluent.MultiSelectMessagesSettingsBuilder m)
        {
            m.Clear("limpiar");
            m.DeleteTag("borrar");
            m.NoData("No se encontraron datos");
            m.SingleTag("item(s) seleccionados");
        }

        public static Kendo.Mvc.UI.Fluent.PDFSettingsBuilder SetPdfSettingsOficio(Kendo.Mvc.UI.Fluent.PDFSettingsBuilder pdf, string proxyUrl)
        {
            pdf.Margin(PdfOficioMargenSup, PdfOficioMargenDer, PdfOficioMargenInf, PdfOficioMargenIzq)
                .PaperSize(PdfOficioPaperSize)
                .ProxyURL(proxyUrl) //.ForceProxy(true)
                .TemplateId("page-oficio-template");
            return pdf;
        }


    }

    public static class StringExtensions
    {
        public static string ToSqlSafeLikeData(this string val)
        {
            return Regex.Replace(val, @"([%_\[])", @"[$1]").Replace("'", "''");
        }

        public static string ToSqlSafeString(this string val)
        {
            return "'" + val.Replace("'", "''") + "'";
        }
    }

}