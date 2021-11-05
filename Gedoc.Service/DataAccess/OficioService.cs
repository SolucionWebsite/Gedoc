using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Gedoc.Helpers;
using Gedoc.Helpers.Dto;
using Gedoc.Helpers.Enum;
using Gedoc.Helpers.Logging;
using Gedoc.Repositorio.Interfaces;
using Gedoc.Service.DataAccess.Interfaces;
using Gedoc.Service.FirmaDigital;
using Gedoc.Service.Pdf;
using Gedoc.Service.Sharepoint;

namespace Gedoc.Service.DataAccess
{
    public class OficioService : BaseService, IOficioService
    {
        private readonly string VariableDelimiter = "%";
        private readonly string ClassCampoSpan = "campo-plantilla";
        private readonly IMantenedorRepositorio _repoMant;
        private readonly IRequerimientoRepositorio _repoReq;
        private readonly IDespachoRepositorio _repoDespacho;
        private readonly IDespachoService _srvDespacho;
        private readonly IPdfService _pdfSvc;
        private readonly IAdjuntoService _adjSvc;

        public OficioService(IMantenedorRepositorio repoMant, IRequerimientoRepositorio repoReq,
            IDespachoRepositorio repoDespacho, IDespachoService srvDespacho,
            IPdfService pdfSvc, IAdjuntoService adjSvc)
        {
            _repoMant = repoMant;
            _repoReq = repoReq;
            _repoDespacho = repoDespacho;
            _srvDespacho = srvDespacho;
            _pdfSvc = pdfSvc;
            _adjSvc = adjSvc;
        }

        #region Plantillas de Oficios
        public DatosAjax<List<PlantillaOficioDto>> GetPlantillaOficioAll(bool resumen = true)
        {
            var resultadoOper = new ResultadoOperacion();
            var resultado = new DatosAjax<List<PlantillaOficioDto>>(new List<PlantillaOficioDto>(), resultadoOper);
            try
            {
                var datos = _repoMant.GetPlantillaOficioAll(resumen);

                foreach (var dato in datos)
                {
                    if (dato.TipoWord)
                    {
                        string url = "Adjuntos\\Adjuntos de Plantilla Oficio\\" + dato.NombreDocumento;
                        var idAdjunto = _adjSvc.GetArchivo(url).OrigenId;
                        dato.IdAdjunto = idAdjunto;
                    }
                }

                resultado.Data = datos;
                resultado.Total = datos?.Count ?? 0;
            }
            catch (Exception ex)
            {
                LogError(resultadoOper, ex, "Lo sentimos, ha ocurrido un error al obtener los datos.");
            }
            resultado.Resultado = resultadoOper;
            return resultado;
        }

        public PlantillaOficioDto GetPlantillaOficioById(int id)
        {
            try
            {
                return _repoMant.GetPlantillaOficioById(id);
            }
            catch (Exception ex)
            {
                LogError(ex);
                return null;
            }
        }

        /// <summary>
        /// Obtiene la plantilla con id plantillaId y sustituye las variables y funciones de acuerdo a los datos de los requerimientos
        /// especificados en reqIds y mainReqId
        /// </summary>
        /// <param name="plantillaId">Id de la plantilla</param>
        /// <param name="reqIds">Id de todos los requerimientos del Oficio</param>
        /// <param name="mainReqId">Id del requermiento principal</param>
        /// <returns>Valor de tipo ResultadoOoperacion. En la propiedad Extra de este valor se devuelve el contenido de la plantilla con las variables y funciones sustituidas
        /// por los valores correspondientes</returns>
        public ResultadoOperacion GetPlantillaConDatos(int plantillaId, List<int> reqIds, int? mainReqId)
        {
            var resultadoOper = new ResultadoOperacion(1, "Ok", null);
            try
            {
                if (!mainReqId.HasValue)
                {
                    // Se busca el requerimiento principal del ofico, es decir, el más reciente
                    mainReqId = GetDatosRequerimientoMainOficio(0, reqIds, false)?.Id;
                }
                var plantilla = GetPlantillaOficioById(plantillaId);
                var contenido = plantilla.Contenido;

                #region Sustitución de funciones
                // Función MAYUSC()
                contenido = ReemplazaFuncion("MAYUSC", contenido, mainReqId.GetValueOrDefault(0));
                // Función MINUSC()
                contenido = ReemplazaFuncion("MINUSC", contenido, mainReqId.GetValueOrDefault(0));
                // Función SIGLAS()
                contenido = ReemplazaFuncion("SIGLAS", contenido, mainReqId.GetValueOrDefault(0));
                // Función LISTA()
                contenido = ReemplazaFuncion("LISTA", contenido, mainReqId.GetValueOrDefault(0), reqIds);
                #endregion

                #region Sustitución de variables por sus respectivos valores
                var variables = GetVariablesRequerimiento(mainReqId.GetValueOrDefault(0), contenido);
                // Sustitución de variables en el contenido de la plantilla de oficio
                contenido = ReemplazaVariables(contenido, variables);
                #endregion

                #region Sustitución de clases y otros atributos especificos
                // Clase "no-editable" se le agrega el contenteditable="false"
                contenido = contenido.Replace("class=\"no-editable", "contenteditable=\"false\" class=\"no-editable\"");
                // Clase "si-editable" se le agrega el contenteditable="true"
                contenido = contenido.Replace("class=\"si-editable", "contenteditable=\"true\" class=\"si-editable\"");
                #endregion

                resultadoOper.Extra = contenido;

                return resultadoOper;
            }
            catch (Exception ex)
            {
                LogError(ex);
                return new ResultadoOperacion(-1, "Error", null); ;
            }
        }

        /// <summary>
        /// Devuelve el contenido pasado en el parámetro contenido, sustituyendo los valores de campos de requerimientos
        /// que se encuentre dentro de este contenido por los valores de los nuevos requerimientos pasados en reqIds y mainReqId
        /// </summary>
        /// <param name="oficioId">Id del oficio al q pertenece el contenido</param>
        /// <param name="contenido">Contenido del oficio a actualizar</param>
        /// <param name="reqIds">Id de todos los requerimientos del Oficio</param>
        /// <param name="mainReqId">Id del requermiento principal</param>
        /// <returns>Valor de tipo ResultadoOoperacion. En la propiedad Extra de este valor se devuelve el contenido de la plantilla con las variables y funciones sustituidas
        /// por los valores correspondientes</returns>
        public ResultadoOperacion GetContenidoOficioActualizado(int oficioId, string contenido, List<int> reqIds, int? mainReqId)
        {
            var resultadoOper = new ResultadoOperacion(1, "Ok", null);
            try
            {
                if (!mainReqId.HasValue)
                {
                    // Se busca el requerimiento principal del ofico, es decir, el más reciente
                    mainReqId = GetDatosRequerimientoMainOficio(0, reqIds, false)?.Id;
                }

                #region Sustitución de funciones
                //// Función MAYUSC()
                // Se buscan los <span> q tengan clase CAMPO-PLANTILLA. Deben estar en la forma "<span class='CAMPO-PLANTILLA NombreVariable'>Valor</span>"
                var delimIni = $"<span class=\"{ClassCampoSpan.ToUpper()} ";
                var delimFin = "</span>";
                var textoAReemplazarMay = ExtraeTextoEntreDelimitadores(contenido, delimIni, delimFin);
                //// Función MINUSC()
                //contenido = ReemplazaFuncion("MINUSC", contenido, mainReqId);
                //// Función LISTA()
                //contenido = ReemplazaFuncion("LISTA", contenido, mainReqId, reqIds);
                #endregion

                #region Sustitución de valores de campos del requerimiento principal
                // Se buscan los <span> q tengan clase campo-plantilla. Deben estar en la forma "<span class='campo-plantilla NombreVariable'>Valor</span>"
                delimIni = $"<span class=\"{ClassCampoSpan} ";
                delimFin = "</span>";
                var textoAReemplazar = ExtraeTextoEntreDelimitadores(contenido, delimIni, delimFin);
                textoAReemplazar.AddRange(textoAReemplazarMay);
                // Para cada texto en textoAReemplazar (deben estar en la forma "NombreVariable">Valor", por ej. "ComunaMN">La Florida")
                // se extrae el nombre de variable (NombreVariable) q está en el texto y se forma un Dictionary con esa variable
                // como key y el valor el del texto (es decir, "NombreVariable">Valor", q es como si fuera la variable q luego se sustituirá en el contenido del oficio)
                var variablesDatos = new Dictionary<string, string>();
                foreach (var texto in textoAReemplazar)
                {
                    var nombreVariable = texto.Contains("\">") ? texto.Substring(0, texto.IndexOf("\">")) : "";
                    if (!string.IsNullOrWhiteSpace(nombreVariable))
                        variablesDatos[nombreVariable] = texto;
                }
                // se obtiene el valor de cada variable a sustituir. variablesValores tendrá como key la 'variable' a sustituir
                // y q corresponde a un texto en la forma "NombreCampoReq'>Valor de NombreCampoReq", por ej. "DocumentoIngreso">106-2020"
                var ficha = _repoReq.GetFichaFullById(mainReqId.GetValueOrDefault(0));
                // variablesEnTexto tiene el nombre de la variable como key, pero es necesario un Dictionary con el nombre
                // de campo en la key (de acuerdo a la tabla CampoPlantilla) para pasarselo a GetValoresVariablesFromItem
                var camposPlant = _repoMant.GetCamposSeleccionable();
                var camposDatos = new Dictionary<string, string>();
                foreach (var item in variablesDatos)
                {
                    var variableBuscar = item.Key.Contains("-") 
                        ? item.Key.Substring(0, item.Key.IndexOf("-"))
                        : item.Key;
                    variableBuscar = $"{VariableDelimiter}{variableBuscar}{VariableDelimiter}";
                    var campo = camposPlant.FirstOrDefault(c =>
                                    c.Variable.Equals(variableBuscar, StringComparison.InvariantCultureIgnoreCase))?.Nombre
                                ?? item.Key;
                    // Para cuando el valor está en mayúsculas por estar dentro de la funcion MAYUSC()
                    campo = item.Key.Equals(item.Key.ToUpper(), StringComparison.InvariantCulture) ? campo.ToUpper() : campo;
                    // Para cuando el valor está en minúsculas por estar dentro de la funcion MINUSC()
                    campo = item.Key.Equals(item.Key.ToLower(), StringComparison.InvariantCulture) ? campo.ToLower() : campo;
                    camposDatos[campo] = item.Value;
                }
                var variablesValores = GetValoresVariablesFromItem(ficha, camposDatos);
                // Cada valor en variablesValores, q tiene el dato extraido de la ficha del requerimiento, es necesario llevarlo a la forma "NombreCampoReq">Valor"
                var keys = variablesValores.Keys.ToList();
                foreach (var key in keys)
                {
                    var valor = variablesValores[key];
                    var textoInicio = key.Contains("\">") ? key.Substring(0, key.IndexOf("\">") + 2) : "";
                    if (textoInicio.Contains("-SIGLAS"))
                    {
                        if (!string.IsNullOrWhiteSpace(valor))
                        {
                            // Se obtienen las siglas
                            string[] strSplit = valor.Split();
                            var siglas = "";
                            foreach (string res in strSplit)
                            {
                                siglas += res.Substring(0, 1);
                            }

                            valor = siglas.ToUpper();
                        }
                    } 
                    variablesValores[key] = textoInicio + valor;
                }
                // Sustitución de variables en el contenido de la plantilla de oficio
                contenido = ReemplazaVariables(contenido, variablesValores, false);
                #endregion

                resultadoOper.Extra = contenido;

                return resultadoOper;
            }
            catch (Exception ex)
            {
                LogError(ex);
                return null;
            }
        }

        /// <summary>
        /// Extrae las variables (están encerradas entre % %) del texto especificado y para cada variable encontrada obtiene de 
        /// la ficha de requerimiento el valor de la propiedad que tenga igual nombre a la variable.
        /// </summary>
        /// <param name="reqMainId">id de requerimiento para tomar el valor de las variables encontradas en texto</param>
        /// <param name="texto">Texto a extraer las variables</param>
        /// <returns>Dictionary con el nombre de la variable encontrada en texto como key y como valor el valor
        /// de la propiedad en requerimiento  que tiene el mismo nombre q la variable</returns>
        private Dictionary<string, string> GetVariablesRequerimiento(int reqMainId, string texto)
        {
            if (string.IsNullOrWhiteSpace(texto) || reqMainId <= 0)
                return new Dictionary<string, string>();
            var variablesEnTexto = _repoMant.GetCamposSeleccionable()
                .Where(d => texto.IndexOf(d.Variable) >= 0)
                .Select(d => new { d.Nombre, Variable = d.Variable.Replace(VariableDelimiter, "") })
                .ToDictionary(d => d.Nombre, d => d.Variable);

            //*var variablesEnTexto = texto.Split('%', '%').Where((item, index) => index % 2 != 0).ToList();
            // Se obtiene todos los datos de la ficha del requermiento para de ahí tomar el valor de las variables
            var ficha = _repoReq.GetFichaFullById(reqMainId);
            if (ficha == null)
                return new Dictionary<string, string>();
            var variables = GetValoresVariablesFromItem(ficha, variablesEnTexto);
            return variables;
        }

        /// <summary>
        /// Obtiene un Dictionary con el nombre de la variable, y su respectivo valor, de acuerdo a las variables
        /// pasadas en el parametro variablesEnTexto. El valor de cada variable se extrae de los valores de las propiedades
        /// del objeto pasado en el parámetro itemOrig.
        /// </summary>
        /// <typeparam name="T">Tipo del objeto pasado en el parámetro itemOirg</typeparam>
        /// <param name="itemOrig">Objeto de donde extraer los valores de las variables</param>
        /// <param name="variablesEnTexto">Dictionary donde la key es el nombre de la propiedad a tomar el valor del objeto itemOrig, y
        /// el valor del Dictionary es el nombre de la variable (sin %%)</param>
        /// <returns>Retorna un Dictionay con key el nombre de la variable (sin %%) y valor el valor de la propiedad en
        /// itemOrig asociado a dicha variable</returns>
        private Dictionary<string, string> GetValoresVariablesFromItem<T>(T itemOrig, Dictionary<string, string> variablesEnTexto)
        {
            var variablesData = new Dictionary<string, string>();
            var cls = typeof(T);
            PropertyInfo[] propertyInfos = cls.GetProperties();
            foreach (PropertyInfo pInfo in propertyInfos)
            {
                var propName = pInfo.Name;
                var propValue = pInfo.GetValue(itemOrig);
                if (variablesEnTexto.ContainsKey(propName) || variablesEnTexto.ContainsKey(propName.ToUpper()) )
                {
                    if (pInfo.PropertyType == typeof(List<GenericoDto>))
                    {
                        var valor = string.Join("; ", ((List<GenericoDto>)propValue).Select(v => v.Titulo));
                        variablesData.Add(variablesEnTexto[propName], valor); 
                    }
                    else if (pInfo.PropertyType == typeof(DateTime) || pInfo.PropertyType == typeof(DateTime?))
                    {
                        var fecha = (DateTime)(propValue ?? new DateTime());
                        // Si es una fecha. De momento se pone para q siempre sea sin hora:
                        var valor = true || DateTime.Compare(fecha, fecha.Date) == 0 ? fecha.ToString("dd/MM/yyyy") : fecha.ToString("dd/MM/yyyy HH:mm");
                        variablesData.Add(variablesEnTexto[propName], valor); 
                    }
                    else
                    {
                        if (variablesEnTexto.ContainsKey(propName))
                        {
                            variablesData[variablesEnTexto[propName]] = (propValue ?? "").ToString();
                        }
                        if (variablesEnTexto.ContainsKey(propName.ToUpper()))
                        {
                            variablesData[variablesEnTexto[propName.ToUpper()]] = (propValue ?? "").ToString().ToUpper();
                        }
                        if (variablesEnTexto.ContainsKey(propName.ToLower()))
                        {
                            variablesData[variablesEnTexto[propName.ToLower()]] = (propValue ?? "").ToString().ToLower();
                        }
                    }
                }
                //else if (camposPlantilla.Keys.Any(n => n.StartsWith(propName + "."))) // Para el caso q se especifique una variable q sea para tomar el valor de una propiedad q sea un objeto, por ejemplo  %MonumentoNacional.ComunaTitulo%
                //{
                //    var varsName = nombresVariable.Where(n => n.StartsWith(propName + ".")).Select(vn => vn.Split('.')[1]).ToList();

                //    var subVar = GetValoresVariablesFromItem(propValue, varsName); // --> No funciona, propValue es de tipo Object por lo q al ejecutarse aquí el GetValoresVariablesFromItem no hay propiedades de Object para recorrer y siempre devolverá vacìo
                //    variablesData = variablesData.Concat(subVar.Where(x => !variablesData.ContainsKey(x.Key)))
                //        .ToDictionary(x => x.Key, x => x.Value);
                //}
            }

            ////if (VariablesMensaje != null)
            ////{
            ////    foreach (var key in VariablesMensaje.Keys)
            ////    {
            ////        if (!variablesData.ContainsKey(key))
            ////            variablesData.Add(key, VariablesMensaje[key]);
            ////    }
            ////}

            return variablesData;
        }

        /// <summary>
        /// Reemplaza, en el texto especificado, la función especificada como parámetro con su respectivo valor.
        /// Esta función dentro del texto puede tener dentro variables (están encerradas entre %%) las cuales son
        /// sustituidas por sus respectivos valores y luego se le aplica la función especificada.
        /// <para>Ejemplo de una función dentro del texto: MAYUSC(%RemitenteNombre%), lo cual debe sustituirse por el nombre
        /// del remitente del requerimiento, en mayúsculas</para>
        /// </summary>
        /// <param name="funcion">Función a buscar y reemplazar en el texto</param>
        /// <param name="contenido">Texto a buscar la función</param>
        /// <param name="mainReqId">Id del requermiento del cual tomar los datos para sustituir los valores de las variables
        /// <param name="reqIds">Id de requermientos, se utiliza cuando es una función de repeteción como  LISTA()</param>
        /// dentro de la función.</param>
        /// <returns></returns>
        private string ReemplazaFuncion(string funcion, string contenido, int mainReqId, List<int> reqIds = null)
        {
            foreach (Match match in Regex.Matches(
                contenido,
                $"{funcion}[(](.*?)[)]"))
            {
                var contenidoFunc = match.Groups[1].Value;
                var contenidoFuncConDatos = "";
                var variablesEnFunc = new Dictionary<string, string>();
                if (funcion.Equals("LISTA", StringComparison.InvariantCultureIgnoreCase))
                { // Si es la función LISTA es necesario repetir la sustitución del contenido de la función por cada requerimiento del oficio
                    foreach (var id in (reqIds ?? new List<int>()))
                    {
                        // Obtención de variables, y sus valores, dentro de la función
                        variablesEnFunc = GetVariablesRequerimiento(id, contenidoFunc);
                        // Sustitución de variables en el contenido de la función
                        contenidoFuncConDatos += (contenidoFuncConDatos == "" ? "" : "<br/>") + ReemplazaVariables(contenidoFunc, variablesEnFunc, true, funcion);
                    }
                } else
                {
                    // Obtención de variables, y sus valores, dentro de la función
                    variablesEnFunc = GetVariablesRequerimiento(mainReqId, contenidoFunc);
                    // Sustitución de variables en el contenido de la función
                    contenidoFuncConDatos = ReemplazaVariables(contenidoFunc, variablesEnFunc, true, funcion);
                }

                // Se aplica la función en cuestión
                // Se decodifica cualquier codificiacón Html, por ejemplo de las tildes.
                contenidoFuncConDatos = System.Net.WebUtility.HtmlDecode(contenidoFuncConDatos) ?? "";
                switch (funcion)
                {
                    case "MAYUSC":
                        contenidoFuncConDatos = contenidoFuncConDatos.ToUpper();
                        break;
                    case "MINUSC":
                        contenidoFuncConDatos = contenidoFuncConDatos.ToLower();
                        break;
                    case "SIGLAS":
                        foreach (var valorVar in variablesEnFunc.Values)
                        {
                            if (!string.IsNullOrWhiteSpace(valorVar) && contenidoFuncConDatos.Contains(valorVar))
                            {
                                // Se obtienen las siglas
                                string[] strSplit = valorVar.Split();
                                var siglas = "";
                                foreach (string res in strSplit)
                                {
                                    siglas += res.Substring(0, 1);
                                }
                                // Se sustituye las siglas en el valor
                                contenidoFuncConDatos = contenidoFuncConDatos.Replace(valorVar, siglas.ToUpper());
                            }
                        }
                        break;
                }
                // Se vuelve a codificar a Html
                //contenidoFuncConDatos = System.Net.WebUtility.HtmlEncode(contenidoFuncConDatos);

                // Se sustituye, en el contenido de la plantilla, la función por el valor obtenido al aplicar la función
                // Si dentro de la función está la variable %SecreatarioTecn% entonces es necesario conservar la función para q al generar el pdf se aplique la función al nombre del secretario(a) técnico
                if (contenidoFunc.IndexOf("%SecretarioTecn%", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    contenidoFuncConDatos = Regex.Replace(contenidoFuncConDatos, "%SecretarioTecn%", "%SecretarioTecn%",
                        RegexOptions.IgnoreCase); // Para conservar la variable %SecretarioTecn% tal cual, en el caso de q se haya convertido a mayúsculas o minúsculas
                    contenido = contenido.Replace($"{funcion}({contenidoFunc})", $"{funcion}({contenidoFuncConDatos})");
                } else
                {
                    contenido = contenido.Replace($"{funcion}({contenidoFunc})", contenidoFuncConDatos);
                }
            }

            return contenido;
        }

        private List<string> ExtraeTextoEntreDelimitadores(string contenido, string delimitadorIni, string delimitadorFin)
        {
            var resultado = new List<string>();
            var pattern = $"{delimitadorIni}(.*?){delimitadorFin}";
            foreach (Match match in Regex.Matches(contenido, pattern ))
            {
                resultado.Add(match.Groups[1].Value);
            }

            return resultado;
        }


        /// <summary>
        /// Reemplaza, en el texto especificado, las variables por sus respectivos valores q se encuentran en
        /// el parámetro variables
        /// </summary>
        /// <param name="texto">Texto a sustituir las variables</param>
        /// <param name="variables">Dictionario con combinación de variable-valor para sustituir en el texto</param>
        /// <param name="usarDelimiter">True para reemplazar en el texto la variable q está encerrada entre el delimitador por defecto (%%), False
        /// para reemplazar en el texto la variable tal cual aparece en el parámetro variable</param>
        /// <param name="funcion">Nombre de la función cuando se está reemplazando las variables q están dentro de dicha función. Se utiliza
        /// solamente para añadir el nombre de la función al nombre de la variable en la clase css del span con q se encierra el valor
        /// de la variable sustituido en el texto.</param>
        /// <returns>Texto con las variables sustituidas por sus valores. Cuando el parametro usarDelimiter es true
        /// se encierran los valore sde las variables entre etiquetas span con una clase css q lleva el nombre de la variable
        /// para identificar ese valor a qué variable corresponde.</returns>
        private string ReemplazaVariables(string texto, Dictionary<string, string> variables, bool usarDelimiter = true, string funcion = "")
        {
            #region Variables genericas
            if (variables == null)
                variables = new Dictionary<string, string>();
            if (!variables.ContainsKey("FechaHoy"))
                variables.Add("FechaHoy", DateTime.Now.ToString("dd/MM/yyyy"));
            if (!variables.ContainsKey("FechaAhora"))
                variables.Add("FechaAhora", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
            //if (texto.Contains("SecretarioTecn"))
            //{
            //    // Se obtiene el Secretario Técnico q es el responsable de la UT Jefatura CMN
            //    var utJefaturaId = WebConfigValues.Ut_JefaturaCmn;
            //    utJefaturaId = utJefaturaId == 0 ? 76 : utJefaturaId;
            //    var respons = _repoMant.GetResponsableUt(utJefaturaId);
            //    // Se asigna el valor de la variable
            //    variables["SecretarioTecn"] = respons?.NombresApellidos;
            //}
            #endregion
            //if (variables != null)
            {
                variables.Keys.ToList().ForEach(nombreVar =>
                {
                    var variable = usarDelimiter ? $"{VariableDelimiter}{nombreVar}{VariableDelimiter}" : nombreVar;
                    var valorVariable = variables[nombreVar];
                    // Se encierra el valor de la variable entre etiquetas <span> con una clase igual al nombre de la variable
                    // de manera q en el texto obtenido luego de sustituir las variables se pueda detectar qué valores
                    // corresponden a la sustitución de variables y a qué variable está asociado cada uno
                    if (usarDelimiter)
                    {
                        funcion = funcion == "SIGLAS" ? "-SIGLAS" : ""; // string.IsNullOrWhiteSpace(funcion) ? "" : $"-{funcion}";
                        valorVariable = $"<span class='{ClassCampoSpan} {nombreVar}{funcion}'>{valorVariable}</span>";
                    }
                    texto = texto.Replace(variable, valorVariable);
                });
            }
            return texto;
        }

        public ResultadoOperacion SavePlantillaOficio(PlantillaOficioDto datos, bool updateSoloActivo)
        {
            var errorId = Guid.NewGuid();
            var resultado = new ResultadoOperacion(-1,
                "Lo sentimos, ha ocurrido un error grabar el registro.<br/>Por favor, revise el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
            // Log
            var logSistema = new LogSistemaDto()
            {
                Flujo = "PLANTILLAOFICIO",
                Fecha = DateTime.Now,
                Origen = "PLANTILLAOFICIO",
                Usuario = datos.UsuarioActual ?? "<desconocido>",
                UsuarioId = datos.UsuarioCreacionId,
                RequerimientoId = 0,
                EstadoId = 0,
                EtapaId = 0,
                Accion = datos.Id == 0 ? "CREACION" : "EDICION",
                ExtraData = ""
            };

            if (datos.Id == 0)
            { // Nuevo
                datos.FechaCreacion = DateTime.Now;
                datos.FechaModificacion = DateTime.Now;
                datos.UsuarioCreacionId = datos.UsuarioActualId;
                datos.UsuarioModificacionId = datos.UsuarioActualId;
                resultado = _repoMant.NewPlantillaOficio(datos);
            }
            else
            { // Update
                datos.FechaModificacion = DateTime.Now;
                datos.UsuarioModificacionId = datos.UsuarioActualId;
                resultado = _repoMant.UpdatePlantillaOficio(datos, updateSoloActivo);
            }

            logSistema.OrigenId = datos.Id;
            // Se graba log
            _repoMant.SaveLogSistema(logSistema);

            return resultado;
        }

        public List<CampoSeleccionableDto> GetCamposSeleccionablePorGrupos(int? tipoPlantilla)
        {
            try
            {
                var filtroTipoCampo = new List<string>();
                switch (tipoPlantilla)
                {
                    case (int)TipoPlantillaOficio.DespachoIniciativa:
                        filtroTipoCampo.Add("OFI");
                        break;
                }
                return _repoMant.GetCamposSeleccionablePorGrupos(filtroTipoCampo);
            }
            catch (Exception ex)
            {
                LogError(ex);
                return null;
            }
        }

        public List<CampoSeleccionableDto> GetCamposSeleccionableByPadre(int? padreId, int? tipoPlantilla)
        {
            try
            {
                if (tipoPlantilla.GetValueOrDefault(0) <= 0) return new List<CampoSeleccionableDto>();
                var filtroTipoCampo = new List<string>();
                switch (tipoPlantilla)
                {
                    case (int)TipoPlantillaOficio.DespachoIniciativa:
                        filtroTipoCampo.Add("OFI");
                        filtroTipoCampo.Add("FUN");
                        filtroTipoCampo.Add("OTR");
                        break;
                }
                var datos = _repoMant.GetCamposSeleccionableByPadre(padreId, filtroTipoCampo);
                datos.ForEach(d => d.HasChildren = d.PadreId == null); /* Tener en cuenta esta condición si en un futuro se necesit tener más de dos niveles en los campos a seleccionar, con esta condición se está limitando a q solo los items con padre en null son los q tienen hijos*/
                return datos;
            }
            catch (Exception ex)
            {
                LogError(ex);
                return new List<CampoSeleccionableDto>();
            }
        }

        public ResultadoOperacion MarcaDeletePlantilla(int plantillaId, UsuarioActualDto usuario)
        {
            var errorId = Guid.NewGuid();
            var resultado = new ResultadoOperacion(-1,
                "Lo sentimos, ha ocurrido un error al eliminar el registro.<br/>Por favor, revise el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
            try
            {
                var enUso = _repoDespacho.ExistePlantillaEnOficio(plantillaId);
                if (enUso)
                {
                    resultado = new ResultadoOperacion(-1,
                        "La plantilla se encuentra en uso en al menos un oficio, no se puede eliminar.", null);
                    return resultado;
                }
                _repoMant.MarcaDeletePlantilla(plantillaId, usuario.UsuarioId);
                resultado.Codigo = 1;
                resultado.Mensaje = "Registro eliminado con éxito.";

                // Se graba el Log de Sistema al eliminar
                var logSistema = new LogSistemaDto()
                {
                    Flujo = "PLANTILLAOFICIO",
                    Fecha = DateTime.Now,
                    Origen = "PLANTILLAOFICIO",
                    OrigenId = plantillaId,
                    OrigenFecha = null,
                    Usuario = usuario.LoginName,
                    UsuarioId = usuario.UsuarioId,
                    RequerimientoId = 0,
                    EstadoId = 0,
                    EtapaId = 0,
                    UnidadTecnicaId = 0,
                    Accion = "ELIMINADO",
                    ExtraData = "",
                    DireccionIp = usuario.DireccionIp,
                    NombrePc = usuario.NombrePc,
                    UserAgent = usuario.UserAgent
                };
                _repoMant.SaveLogSistema(logSistema);
            }
            catch (Exception ex)
            {
                LogError(null, ex, resultado.Mensaje);
                resultado = new ResultadoOperacion(-1,
                    "Lo sentimos, ha ocurrido un error al eliminar el registro.<br/>Por favor, chequee el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
            }
            return resultado;
        }

        public ResultadoOperacion DeletePlantilla(int id)
        {
            var errorId = Guid.NewGuid();
            var resultado = new ResultadoOperacion(-1,
                "Lo sentimos, ha ocurrido un error al eliminar el registro.<br/>Por favor, revise el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
            try
            {
                // TODO: hacer las validaciones necesarias para poder borrar la plantilla: q no se haya utilizado en algún momento
                _repoMant.DeletePlantilla(id);
                resultado.Codigo = 1;
                resultado.Mensaje = "Registro eliminado con éxito.";
            }
            catch (Exception ex)
            {
                LogError(null, ex, resultado.Mensaje);
                resultado = new ResultadoOperacion(-1,
                    "Lo sentimos, ha ocurrido un error al eliminar el registro.<br/>Por favor, chequee el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
            }
            return resultado;
        }
        #endregion

        public RequerimientoDto GetDatosRequerimientoMainOficio(int oficioId, List<int> reqsId, bool releer)
        {
            var reqMain = new RequerimientoDto();
            try
            {
                if ((reqsId?.Count() ?? 0) > 0)
                {
                    // siempre q se especifique una lista de ids de requerimientos se busca el principal dentro de esta lista, es decir, el más reciente
                    var datos = _repoReq.GetRequerimientoByIds(reqsId, false);
                    reqMain = (datos ?? new List<RequerimientoDto>())
                        .OrderByDescending(d => d.FechaIngreso)
                        //.Select(d => new RequerimientoDto
                        //{ // Se reducen los campos a retornar
                        //    Id = d.Id,
                        //    FechaIngreso = d.FechaIngreso,
                        //    FechaDocumento = d.FechaDocumento,
                        //    DocumentoIngreso = d.DocumentoIngreso
                        //})
                        .FirstOrDefault();
                }
                else
                {
                    var datos = _repoDespacho.GetOficoById(oficioId);
                    var reqMainId = (datos?.RequerimientoPrincipalId).GetValueOrDefault(0);
                    reqMain = _repoReq.GetById(reqMainId);
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                reqMain = null;
            }

            return reqMain;
        }
        #region Oficios
        public DatosAjax<List<OficioDto>> GetDatosBandejaOficio(ParametrosGrillaDto<int> param, int idUsuario)
        {
            var resultado = new DatosAjax<List<OficioDto>>(new List<OficioDto>(), new ResultadoOperacion(-1, "Lo sentimos, ha ocurrido un error al obtener los datos de la bandeja.", null));
            try
            {
                var idBandejaMain = param.Dato;
                var configBandeja = _repoMant.GetConfigBandeja((int)Bandeja.Oficio, true, true, bandejaMainId: idBandejaMain);
                if (configBandeja == null)
                {
                    resultado.Resultado.Codigo = -1;
                    resultado.Resultado.Mensaje = "No se pueden mostrar los datos de la pestaña Oficios, no se encontró la configuración de la bandeja.";
                    return resultado;
                }

                // TODO: revisar si de acuerdo a los roles del usuario se aplican determinados filtros a los datos de la bandeja
                // TODO: además si el usuario no tiene dentro de los permisos acceder a la bandeja entonces devolver
                // error para evitar q se acceda especificando directamente la Url de la bandeja
                var take = param.Take > 0 ? param.Take : param.PageSize;
                var skip = param.Skip == 0 && param.Page > 0 ? ((param.Page - 1) * take) : param.Skip;
                var sort = param.Sort == null
                    ? new SortParam() { Field = "Id", Dir = "DESC" }
                    : new SortParam() { Field = param.Sort.Split('-')[0], Dir = param.Sort.Contains("-") ? param.Sort.Split('-')[1] : "DESC" };
                sort.Field = sort.Field == "EstadoOficioTitulo" ? "EstadoOficio.Titulo" : sort.Field;
                sort.Field = sort.Field == "EtapaOficioTitulo" ? "EtapaOficio.Titulo" : sort.Field;
                sort.Field = sort.Field == "ProfesionalNombre" ? "UsuarioCreacion.NombresApellidos" : sort.Field;
                sort.Field = sort.Field == "UnidadTecnicaNombre" ? "UnidadTecnica.Titulo" : sort.Field;
                var fechaDesde = DateTime.Now.AddDays(configBandeja.DiasAtras > 0 ? -1 * configBandeja.DiasAtras : configBandeja.DiasAtras);

                resultado = _repoDespacho.GetDatosBandejaEntradaOficio(idBandejaMain, skip, take, sort, param.FilterText, fechaDesde, idUsuario, configBandeja);
            }
            catch (Exception ex)
            {
                resultado.Resultado.Extra = ex.Message;
                LogError(resultado.Resultado, ex, "Lo sentimos, ha ocurrido un error al obtener los datos de la pestaña Oficios.");
            }
            return resultado;
        }

        public DatosAjax<List<OficioDto>> GetOficoAll()
        {
            var resultadoOper = new ResultadoOperacion();
            var resultado = new DatosAjax<List<OficioDto>>(new List<OficioDto>(), resultadoOper);
            try
            {
                var datos = _repoDespacho.GetOficoAll();
                resultado.Data = datos;
                resultado.Total = datos?.Count ?? 0;
            }
            catch (Exception ex)
            {
                LogError(resultadoOper, ex, "Lo sentimos, ha ocurrido un error al obtener los datos.");
            }
            resultado.Resultado = resultadoOper;
            return resultado;
        }

        public OficioDto GetOficoById(int id, bool procesaEncabezado = false)
        {
            try
            {
                var oficio = _repoDespacho.GetOficoById(id);
                if (procesaEncabezado)
                {
                    SeparaEncabezadoPiePagina(oficio);
                }
                return oficio;
            }
            catch (Exception ex)
            {
                LogError(ex);
                return null;
            }
        }

        public DatosAjax<List<OficioObservacionDto>> GetObservacionesOficio(int oficioId)
        {
            var resultadoOper = new ResultadoOperacion();
            var resultado = new DatosAjax<List<OficioObservacionDto>>(new List<OficioObservacionDto>(), resultadoOper);
            try
            {
                var datos = _repoDespacho.GetObservacionesOficio(oficioId);
                resultado.Data = datos;
                resultado.Total = datos?.Count ?? 0;
            }
            catch (Exception ex)
            {
                LogError(resultadoOper, ex, "Lo sentimos, ha ocurrido un error al obtener los datos.");
            }
            resultado.Resultado = resultadoOper;
            return resultado;
        }

        public void SeparaEncabezadoPiePagina(OficioDto oficio)
        {
            // Encabezado
            var tagIni = "<div class=\"encabezado-pdf\"";
            var tagFin = "</div>";
            var encabezados = ExtraeTextoEntreDelimitadores(oficio.Contenido
                , tagIni
                , tagFin);
            var encabezado = "";
            // Solo tomo el primer encabezado si hay alguno, el resto se descarta, y sustituyo la clase
            if (encabezados.Count > 0)
            {
                encabezado = tagIni.Replace("encabezado-pdf", "encabezado-impresion") + encabezados[0] + tagFin;
            }
            oficio.Encabezado = encabezado;

            // Pie
            tagIni = "<div class=\"pie-pdf\"";
            tagFin = "</div>";
            var pies = ExtraeTextoEntreDelimitadores(oficio.Contenido
                , tagIni
                , tagFin);
            var pie = "";
            // Solo tomo el primer pie de página si hay alguno, el resto se descarta, y sustituyo la clase
            if (pies.Count > 0)
            {
                pie = tagIni.Replace("pie-pdf", "pie-impresion") + pies[0] + tagFin;
            }
            oficio.Pie = pie;
        }

        public ResultadoOperacion SaveOficio(OficioDto oficio, byte[] file)
        {
            var errorId = Guid.NewGuid();
            // var resultadoEnvioEmail = new ResultadoOperacion();
            var resultLog = new ResultadoOperacion(1, "OK", null);
            var resultado = new ResultadoOperacion(-1,
                "Lo sentimos, ha ocurrido un error al grabar los datos.<br/>Por favor, revise el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
            try
            {
                var logSistema = new LogSistemaDto()
                {
                    Flujo = oficio.Flujo.ToString().ToUpper(),
                    Fecha = DateTime.Now,
                    Origen = "OFICIO",
                    Usuario = oficio.DatosUsuarioActual?.LoginName ?? "<desconocido>",
                    UsuarioId = oficio.DatosUsuarioActual?.UsuarioId,
                    DireccionIp = oficio.DatosUsuarioActual?.DireccionIp,
                    NombrePc = oficio.DatosUsuarioActual?.NombrePc,
                    UserAgent = oficio.DatosUsuarioActual?.UserAgent,
                    ExtraData = oficio.Observaciones
                };

                oficio.FechaModificacion = DateTime.Now;
                oficio.UsuarioModificacionId = oficio.DatosUsuarioActual?.UsuarioId;
                if (oficio.Id == 0)
                {  // Nuevo Oficio
                    oficio.NumeroOficio = ""; // Se genera al firmar el oficio
                    oficio.FechaEmisionOficio = null; // Se genera al firmar el oficio

                    // Si es tipo plantilla Despacho entonces no tiene Unidad Técnica y si es Despacho Iniciativa no tiene Requerimiento
                    if (oficio.TipoPlantillaId.GetValueOrDefault(1) == (int) TipoPlantillaOficio.DespachoIniciativa)
                    {
                        oficio.Requerimiento = new List<GenericoDto>();
                    }
                    else
                    {
                        oficio.UnidadTecnicaId = null;
                        var reqIds = (oficio.Requerimiento ?? new List<GenericoDto>()).Select(r => r.IdInt).ToList();
                        oficio.RequerimientoPrincipalId = GetDatosRequerimientoMainOficio(0, reqIds, false)?.Id;
                    }
                    oficio.FechaCreacion = DateTime.Now;
                    oficio.UsuarioCreacionId = oficio.DatosUsuarioActual?.UsuarioId;
                    oficio.Eliminado = false;
                    oficio.EstadoId = (int)Helpers.Enum.EstadoOficio.Borrador;
                    oficio.FechaUltEstado = DateTime.Now;
                    oficio.EtapaId = (int)Helpers.Enum.EtapaOficio.RevisionProfesional;
                    oficio.FechaUltEtapa = DateTime.Now;

                    // Se graba en BD el nuevo oficio
                    resultado = _repoDespacho.NewOficio(oficio);

                    resultado.Mensaje = resultado.Codigo < 0
                        ? resultado.Mensaje
                        : $"Se ha creado satisfactoriamente el oficio. <br/>ID de Oficio: {oficio.Id}.";

                    // Se graba log de sistema
                    logSistema.Accion = "CREACION";
                    logSistema.OrigenId = oficio.Id;
                    logSistema.OrigenFecha = oficio.FechaCreacion;
                    logSistema.RequerimientoId = null;
                    logSistema.EstadoId = oficio.EstadoId;
                    logSistema.EtapaId = oficio.EtapaId;
                    logSistema.UnidadTecnicaId = null;
                    // NO: De momento, según la necesidad, se graba solamente el log de Eliminar el oficio
                    resultLog = _repoMant.SaveLogSistema(logSistema);
                }
                else
                {
                    // Se edita el oficio:
                    oficio.FechaUltEstado = DateTime.Now;
                    oficio.FechaUltEtapa = DateTime.Now;
                    if (string.IsNullOrWhiteSpace(oficio.Accion) || oficio.Accion == AccionOficio.EDITAR.ToString())
                    {   // Es Editar Oficio
                        var reqIds = (oficio.Requerimiento ?? new List<GenericoDto>()).Select(r => r.IdInt).ToList();
                        oficio.RequerimientoPrincipalId = GetDatosRequerimientoMainOficio(0, reqIds, false)?.Id;
                        resultado = _repoDespacho.UpdateOficio(oficio);
                        logSistema.Accion = "EDICION";
                        logSistema.OrigenFecha = oficio.FechaCreacion;
                    }
                    else
                    {
                        oficio.Eliminado = false;
                        oficio.UltimoUsuarioFlujoId = oficio.UsuarioModificacionId;
                        switch (oficio.Accion)
                        {
                            case nameof(AccionOficio.AENCARGADO):
                                logSistema.Accion = "ENVIAR A ENCARGADO UT";
                                oficio.EstadoId = (int)EstadoOficio.EnviadoEncargadoUT;
                                oficio.EtapaId = (int)EtapaOficio.RevisionEncargado;
                                break;
                            case nameof(AccionOficio.DEVOLVERPROF):
                                logSistema.Accion = "DEVOLVER A PROFESIONAL UT";
                                oficio.EstadoId = (int)EstadoOficio.CorreccionesEncargadoUT;
                                oficio.EtapaId = (int)EtapaOficio.RevisionProfesional;
                                oficio.NuevaObservacion = true;
                                break;
                            case nameof(AccionOficio.AVISADOR):
                                logSistema.Accion = "ENVIAR A VISADOR GENERAL";
                                oficio.EstadoId = (int)EstadoOficio.AprobadoEncargadoUT;
                                oficio.EtapaId = (int)EtapaOficio.RevisionVisadorGen;
                                break;
                            case nameof(AccionOficio.DEVOLVERENC):
                                logSistema.Accion = "DEVOLVER A ENCARGADO UT";
                                oficio.EstadoId = (int)EstadoOficio.CorreccionesVisadorGen;
                                oficio.EtapaId = (int)EtapaOficio.RevisionEncargado;
                                oficio.NuevaObservacion = true;
                                break;
                            case nameof(AccionOficio.AJEFATURA):
                                logSistema.Accion = "ENVIAR A JEFATURA CMN";
                                oficio.EstadoId = (int)EstadoOficio.AprobadoVisadorGen;
                                oficio.EtapaId = (int)EtapaOficio.RevisionSecTecn;
                                break;
                            case nameof(AccionOficio.DEVOLVERVIS):
                                logSistema.Accion = "DEVOLVER A VISADOR GENERAL";
                                oficio.EstadoId = (int)EstadoOficio.CorreccionesJefatura;
                                oficio.EtapaId = (int)EtapaOficio.RevisionVisadorGen;
                                oficio.NuevaObservacion = true;
                                break;
                            case nameof(AccionOficio.EDITFIRMADO):
                                logSistema.Accion = "EDICION FIRMADO";
                                break;
                            case nameof(AccionOficio.BORRAR):
                                if (oficio.EstadoId != (int)EstadoOficio.Borrador)
                                {
                                    // return new ResultadoOperacion(-1, "El oficio no está en estado Borrador, no se puede eliminar.", null);
                                }
                                logSistema.Accion = "ELIMINADO";
                                oficio.Eliminado = true;
                                oficio.EliminacionFecha = DateTime.Now;
                                oficio.UsuarioEliminacionId = oficio.DatosUsuarioActual?.UsuarioId;
                                oficio.EstadoId = -1; // Para conservar el estado actual
                                oficio.EtapaId = -1; // Para conservar la etapa actual
                                break;
                            case nameof(AccionOficio.URGENTE):
                                var dataOficio = _repoMant.GetOficioById(oficio.Id);
                                if (_repoMant.IsAdminUser(oficio.DatosUsuarioActual?.UsuarioId ?? 0) && dataOficio.Urgente)
                                {
                                    logSistema.Accion = "DESMARCADO COMO URGENTE";
                                    oficio.Urgente = false;
                                    oficio.EstadoId = dataOficio.EstadoId;
                                    oficio.EtapaId = dataOficio.EtapaId;
                                }
                                else
                                {
                                    logSistema.Accion = "MARCADO COMO URGENTE";
                                    oficio.Urgente = true;
                                    oficio.EstadoId = dataOficio.EstadoId;
                                    oficio.EtapaId = dataOficio.EtapaId;
                                }
                                break;
                            case nameof(AccionOficio.AFIRMA):
                                // Se obtiene el Rut y Pin de firma digital del usuario
                                var datosUser = _repoMant.GetUsuarioById(oficio.DatosUsuarioActual?.UsuarioId ?? 0);
                                if (string.IsNullOrWhiteSpace(datosUser.Rut) ||
                                    string.IsNullOrWhiteSpace(datosUser.FirmaDigitalPin))
                                {
                                    return new ResultadoOperacion(-1, "No es posible realizar la firma digital del oficio.<br/>Usted no tiene registrado en Gedoc sus datos para el acceso a la plataforma de firma digital.", null);
                                }

                                var signerInfo = new SignerInfo(datosUser.Rut, datosUser.FirmaDigitalPin,
                                    datosUser.Email);

                                logSistema.Accion = "FIRMADO JEFATURA CMN";
                                oficio.EstadoId = (int)EstadoOficio.EnviadoFirma;
                                oficio.EtapaId = (int)EtapaOficio.FirmadoSecTecn;
                                oficio.Urgente = false;

                                // Datos del archivo adjunto al Despacho
                                oficio.datosArchivo = new DatosArchivo();
                                oficio.datosArchivo.TipoArchivo = TiposArchivo.Oficio;

                                // Delegate para subir el archivo adjunto
                                ProcesaArchivo procesaArch = (DatosArchivo datosArchivo, bool subirARepoGedoc, bool eliminar) =>
                                {
                                    if (eliminar)
                                    {
                                        // Se indica eliminar el archivo
                                        var fileHandler = new FileHandler();
                                        var resultDel = fileHandler.EliminarArchivo(datosArchivo.FilePath);
                                        return new ResultadoOperacion(resultDel ? 1 : -1, "", null);
                                    }

                                    //// Se genera el pdf a partir del contenido del oficio guardado en datosArchivo.FileTextContent
                                    //var filePdf = GetPdfFromHtml(datosArchivo.FileTextContent, oficio.BaseUrl);
                                    // Se genera el documento binario en base a la representación Base64 almacenado en FileTextContent
                                    var filePdf = Convert.FromBase64String(datosArchivo.FileTextContent);

                                    var postedFile = new CustomHttpPostedFile(filePdf, datosArchivo.FileName);
                                    datosArchivo.File = postedFile;

                                    // Se guarda en repositorio el archivo del oficio
                                    var resultadoArch = new ResultadoOperacion(1, "OK", null);
                                    if (!subirARepoGedoc) return resultadoArch;

                                    if (filePdf?.Length > 0) // (datosArchivo.File != null)
                                    {
                                        var fileHandler = new FileHandler();
                                        datosArchivo.renombraSiExiste = true;
                                        var filePath = fileHandler.SubirArchivoRepositorio(datosArchivo);

                                        if (string.IsNullOrWhiteSpace(filePath))
                                        { // ocurrió error al subir el archivo al repositorio
                                            resultadoArch.Codigo = -1;
                                            resultadoArch.Mensaje = "Error subiendo el archivo.";
                                        }
                                        else
                                        {
                                            datosArchivo.FilePath = filePath;
                                            resultadoArch.Extra = new[] { filePath, datosArchivo.FileName ?? datosArchivo.File.FileName };
                                        }
                                    }
                                    else
                                    {
                                        return new ResultadoOperacion(-1, "Error, no se generó el PDF a almacenar.", null);
                                    }

                                    return resultadoArch;
                                };

                                // Delegate para crear el nuevo Despacho en base al oficio
                                CreaNuevoDespacho creaNuevoDespacho = (DespachoDto despacho, DespachoIniciativaDto despachoInic) =>
                                {
                                    // Se genera un nuevo despacho en base al oficio
                                    var resultadoDesp = new ResultadoOperacion(1, "OK", null);

                                    try
                                    {
                                        // TODO: dejar esta implementación de comprimir en una sola rutina para ocuparla en sección "AccionOficio.FIRMADODIGITAL"
                                        // Comprimir en Zip los adjuntos del oficio y el pdf del oficio, para adjuntar al despacho
                                        // Se obtienen los adjuntos del oficio
                                        var adjuntosOf = _adjSvc.GetAdjuntosOficio(oficio.Id);
                                        if (adjuntosOf.Resultado.Codigo < 0)
                                        {
                                            return new ResultadoOperacion(-1, "No se ha creado el oficio, ha ocurrido un error al procesar<br/> los adjuntos del oficio para obtener el archivo comprimido.", null);
                                        }
                                        var adjList = new List<DatosArchivo>();
                                        var datosAdj = despachoInic == null
                                            ? despacho.DatosArchivo
                                            : despachoInic.DatosArchivo;
                                        if (datosAdj.File?.InputStream != null)
                                            datosAdj.File.InputStream.Position = 0;
                                        adjList.Add(datosAdj);
                                        foreach (var adj in adjuntosOf.Data)
                                        {
                                            var adjFile = _adjSvc.GetArchivoOficio(adj.Id);
                                            if (adjFile == null || adjFile.OrigenId <= 0)
                                            {
                                                return new ResultadoOperacion(-1, "No se ha creado el oficio, ha ocurrido un error al procesar<br/> los adjuntos del oficio para obtener el archivo comprimido.", null);
                                            }
                                            adjList.Add(adjFile);
                                        }

                                        // Se comprimen los adjuntos del oficio junto al pdf firmado del oficio
                                        var zipStream = new MemoryStream();
                                        using (ZipArchive zip = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
                                        {
                                            foreach (var datosArch in adjList)
                                            {
                                                var entry = zip.CreateEntry(datosArch.File?.FileName ?? datosArch?.FileName);

                                                using (var entryStream = entry.Open())
                                                {
                                                    if (datosArch.FileStream != null)
                                                        datosArch.FileStream.CopyTo(entryStream);
                                                    else if (datosArch.File?.InputStream != null)
                                                        datosArch.File.InputStream.CopyTo(entryStream);
                                                }
                                            }
                                        }
                                        zipStream.Position = 0;

                                        // Se especifica el Zip como archivo del despacho
                                        var zipName = $"Despacho_{despachoInic?.NumeroDespacho ?? despacho?.NumeroDespacho}.zip";
                                        if (despachoInic == null) despacho.DatosArchivo.FileName = zipName; else despachoInic.DatosArchivo.FileName = zipName;
                                        var postedFileList = new List<CustomHttpPostedFile>
                                        {
                                            new CustomHttpPostedFile(zipStream, "application/zip", zipName)
                                        };

                                        if (despacho != null)
                                        {
                                            despacho.DesdeOficio = true;
                                            despacho.UsuarioActual = logSistema.Usuario;
                                            despacho.UsuarioCreacionId = logSistema.UsuarioId;
                                            resultadoDesp = _srvDespacho.Save(despacho, postedFileList);
                                        }
                                        else
                                        {
                                            despachoInic.DesdeOficio = true;
                                            despachoInic.UsuarioActual = logSistema.Usuario;
                                            despachoInic.UsuarioCreacionId = logSistema.UsuarioId;
                                            resultadoDesp = _srvDespacho.SaveDespachoInic(despachoInic, postedFileList);
                                        }
                                    }
                                    catch (Exception exc)
                                    {
                                        Logger.LogError(exc);
                                        resultadoDesp = new ResultadoOperacion(-1, "No se ha creado el oficio, ha ocurrido un error al crear<br/> el archivo Zip.", null);
                                    }

                                    return resultadoDesp;
                                };

                                // Delegate para firma digital del oficio
                                FirmaDigitalOficio firmaDigitalOficio = (DatosArchivo datosArchivo, bool eliminar) =>
                                {
                                    if (!eliminar)
                                    {
                                        // Se genera el pdf a partir del contenido del oficio guardado en datosArchivo.FileTextContent
                                        var filePdf = GetPdfFromHtml(datosArchivo.FileTextContent, oficio.BaseUrl);
                                        var postedFile = new CustomHttpPostedFile(filePdf, datosArchivo.FileName);
                                        datosArchivo.File = postedFile;

                                        var firmador = new FirmaDigital.Firmador();
                                        var resultadoFirma = firmador.Firmar(datosArchivo.FileName ?? datosArchivo.File.FileName,
                                            filePdf, signerInfo);

                                        return resultadoFirma;
                                    } else
                                    {
                                        // Se elimina el documento del firmador digital
                                        var firmador = new FirmaDigital.Firmador();
                                        var resultadoElim = firmador.EliminarDocumento(datosArchivo.OrigenCodigo);
                                        return resultadoElim;
                                    }
                                };

                                // Se graban los datos
                                resultado = _repoDespacho.FirmarOficio(oficio, procesaArch, creaNuevoDespacho, firmaDigitalOficio);

                                break;
                            case nameof(AccionOficio.FIRMADODIGITAL):
                                // Cuando el oficio es firmado digitalmente luego de q fue enviado a firma pero no fue firmado en ese momento sino ahora
                                logSistema.Accion = "FIRMADO JEFATURA CMN";
                                oficio.EstadoId = (int)EstadoOficio.Firmado;
                                oficio.EtapaId = (int)EtapaOficio.FirmadoSecTecn;

                                // Datos del archivo adjunto al Despacho
                                oficio.datosArchivo = oficio.datosArchivo ?? new DatosArchivo();
                                oficio.datosArchivo.TipoArchivo = TiposArchivo.Oficio;

                                // Delegate para subir el archivo adjunto
                                ProcesaArchivo procesaArch2 = (DatosArchivo datosArchivo, bool subirARepoGedoc, bool eliminar) =>
                                {
                                    if (eliminar)
                                    {
                                        // Se indica eliminar el archivo
                                        var fileHandler = new FileHandler();
                                        var resultDel = fileHandler.EliminarArchivo(datosArchivo.FilePath);
                                        return new ResultadoOperacion(resultDel ? 1 : -1, "", null);
                                    }

                                    //// Se genera el pdf a partir del contenido del oficio guardado en datosArchivo.FileTextContent
                                    //var filePdf = GetPdfFromHtml(datosArchivo.FileTextContent, oficio.BaseUrl);
                                    // Se genera el documento binario en base a la representación Base64 almacenado en FileTextContent
                                    var filePdf = Convert.FromBase64String(datosArchivo.FileTextContent);

                                    var postedFile = new CustomHttpPostedFile(filePdf, datosArchivo.FileName);
                                    datosArchivo.File = postedFile;

                                    // Se guarda en repositorio el archivo del oficio
                                    var resultadoArch = new ResultadoOperacion(1, "OK", null);
                                    if (!subirARepoGedoc) return resultadoArch;

                                    if (filePdf?.Length > 0) // (datosArchivo.File != null)
                                    {
                                        var fileHandler = new FileHandler();
                                        datosArchivo.renombraSiExiste = true;
                                        var filePath = fileHandler.SubirArchivoRepositorio(datosArchivo);

                                        if (string.IsNullOrWhiteSpace(filePath))
                                        { // ocurrió error al subir el archivo al repositorio
                                            resultadoArch.Codigo = -1;
                                            resultadoArch.Mensaje = "Error subiendo el archivo.";
                                        }
                                        else
                                        {
                                            datosArchivo.FilePath = filePath;
                                            resultadoArch.Extra = new[] { filePath, datosArchivo.FileName ?? datosArchivo.File.FileName };
                                        }
                                    }
                                    else
                                    {
                                        return new ResultadoOperacion(-1, "Error, no se generó el PDF a almacenar.", null);
                                    }

                                    return resultadoArch;
                                };

                                // Delegate para crear el nuevo Despacho en base al oficio
                                CreaNuevoDespacho creaNuevoDespacho2 = (DespachoDto despacho, DespachoIniciativaDto despachoInic) =>
                                {
                                    // Se genera un nuevo despacho en base al oficio
                                    var resultadoDesp = new ResultadoOperacion(1, "OK", null);

                                    try
                                    {
                                        // Comprimir en Zip los adjuntos del oficio y el pdf del oficio, para adjuntar al despacho
                                        // Se obtienen los adjuntos del oficio
                                        var adjuntosOf = _adjSvc.GetAdjuntosOficio(oficio.Id);
                                        if (adjuntosOf.Resultado.Codigo < 0)
                                        {
                                            return new ResultadoOperacion(-1, "No se ha creado el oficio, ha ocurrido un error al procesar<br/> los adjuntos del oficio para obtener el archivo comprimido.", null);
                                        }
                                        var adjList = new List<DatosArchivo>();
                                        var datosAdj = despachoInic == null
                                            ? despacho.DatosArchivo
                                            : despachoInic.DatosArchivo;
                                        if (datosAdj.File?.InputStream != null)
                                            datosAdj.File.InputStream.Position = 0;
                                        adjList.Add(datosAdj);
                                        foreach (var adj in adjuntosOf.Data)
                                        {
                                            var adjFile = _adjSvc.GetArchivoOficio(adj.Id);
                                            if (adjFile == null || adjFile.OrigenId <= 0)
                                            {
                                                return new ResultadoOperacion(-1, "No se ha creado el oficio, ha ocurrido un error al procesar<br/> los adjuntos del oficio para obtener el archivo comprimido.", null);
                                            }
                                            adjList.Add(adjFile);
                                        }

                                        // Se comprimen los adjuntos del oficio junto al pdf firmado del oficio
                                        var zipStream = new MemoryStream();
                                        using (ZipArchive zip = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
                                        {
                                            foreach (var datosArch in adjList)
                                            {
                                                var entry = zip.CreateEntry(datosArch.File?.FileName ?? datosArch?.FileName);

                                                using (var entryStream = entry.Open())
                                                {
                                                    if (datosArch.FileStream != null)
                                                        datosArch.FileStream.CopyTo(entryStream);
                                                    else if (datosArch.File?.InputStream != null)
                                                        datosArch.File.InputStream.CopyTo(entryStream);
                                                }
                                            }
                                        }
                                        zipStream.Position = 0;

                                        // Se especifica el Zip como archivo del despacho
                                        var zipName = $"Despacho_{despachoInic?.NumeroDespacho ?? despacho?.NumeroDespacho}.zip";
                                        if (despachoInic == null) despacho.DatosArchivo.FileName = zipName; else despachoInic.DatosArchivo.FileName = zipName;
                                        var postedFileList = new List<CustomHttpPostedFile>
                                        {
                                            new CustomHttpPostedFile(zipStream, "application/zip", zipName)
                                        };

                                        //var postedFileList = new List<CustomHttpPostedFile>
                                        //{
                                        //    { despachoInic == null
                                        //        ? ((CustomHttpPostedFile)despacho.DatosArchivo.File)
                                        //        : ((CustomHttpPostedFile)despachoInic.DatosArchivo.File) }
                                        //};

                                        // TODO: hacer q la firma del oficio y la creación del nuevo despacho quede todo en una misma transacción por si hay error revertir todo
                                        if (despacho != null)
                                        {
                                            despacho.DesdeOficio = true;
                                            resultadoDesp = _srvDespacho.Save(despacho, postedFileList);
                                        }
                                        else
                                        {
                                            despachoInic.DesdeOficio = true;
                                            resultadoDesp = _srvDespacho.SaveDespachoInic(despachoInic, postedFileList);
                                        }
                                    }
                                    catch (Exception exc)
                                    {
                                        Logger.LogError(exc);
                                        resultadoDesp = new ResultadoOperacion(-1, "No se ha creado el oficio, ha ocurrido un error al crear<br/> el archivo Zip.", null);
                                    }

                                    return resultadoDesp;
                                };

                                // Se graban los datos
                                resultado = _repoDespacho.UpdateOficioFirmadoDigital(oficio, procesaArch2, creaNuevoDespacho2);

                                break;
                            default:
                                return new ResultadoOperacion(-1, "Acción desconocida a realizar.", null);
                        }

                        // Se graban los datos
                        if (oficio.Accion != AccionOficio.AFIRMA.ToString() &&
                            oficio.Accion != AccionOficio.FIRMADODIGITAL.ToString())
                        {
                            resultado = _repoDespacho.UpdateEstadoOficio(oficio);
                        }
                    }

                    // Se graba log de sistema
                    if (resultado.Codigo > 0
                       //// NO:  De momento, según la necesidad, se graba solamente el log de Eliminar el oficio
                       // && oficio.Accion == "BORRAR"
                    )
                    {
                        logSistema.OrigenId = oficio.Id;
                        logSistema.OrigenFecha = oficio.FechaCreacion;
                        if (oficio.Requerimiento?.Count > 0)
                        {  //Se graba un log para cada requerimiento asociado al oficio, en caso de ser oficio de Despacho
                            var idsReq = oficio.Requerimiento.Select(r => r.IdInt).ToList();
                            var reqsDesp = _repoReq.GetRequerimientoByIds(idsReq, false);
                            var resultLogMulti = new List<ResultadoOperacion>();
                            foreach (var req in reqsDesp)
                            {
                                logSistema.RequerimientoId = req.Id;
                                logSistema.EstadoId = req.EstadoId;
                                logSistema.EtapaId = req.EtapaId;
                                logSistema.UnidadTecnicaId = req.UtAsignadaId;
                                resultLog = _repoMant.SaveLogSistema(logSistema);
                                resultLogMulti.Add(resultLog);
                            }

                            resultLog = resultLogMulti.Any(l => l.Codigo < 0)
                                ? new ResultadoOperacion(-1, "Error.",
                                    null)
                                : new ResultadoOperacion(1, "OK", null);
                        } else
                        {
                            logSistema.RequerimientoId = null;
                            logSistema.EstadoId = oficio.EstadoId;
                            logSistema.EtapaId = oficio.EtapaId;
                            logSistema.UnidadTecnicaId = oficio.UnidadTecnicaId;
                            logSistema.ExtraData = oficio.Observaciones;
                            resultLog = _repoMant.SaveLogSistema(logSistema);
                        }
                    }
                }

                resultado.Mensaje += resultLog.Codigo > 0 ? "" : " (Ha ocurrido un error al grabar el log de transacciones, por favor, revise el fichero log de errores.)";
            }
            catch (Exception ex)
            {
                LogError(null, ex, resultado.Mensaje);
                if (resultado.Codigo > 0)
                {
                    resultado = new ResultadoOperacion(-1,
                        "Lo sentimos, ha ocurrido un error al grabar los datos.<br/>Por favor, chequee el log de error de la aplicación.<br/>{ID de Error: " + errorId + "}", null);
                }
            }
            return resultado;
        }

        public ResultadoOperacion UpdateOficiosPendienteFirma(UsuarioActualDto datosUsuario)
        {
            var resultado = new ResultadoOperacion(1, "", null);
            var resultados = new List<ResultadoOperacion>();

            try
            {
                // Se obtienen los oficios q están pendientes de firma (estado "Enviado a Firma), es decir,
                // fueron enviados al firmador digital pero por alguna razón no se firmaron en ese momento
                var oficios = _repoDespacho.GetOficiosPendienteFirma();
                // Para cada oficio se valida en el firmador el estado
                var firmador = new Firmador();
                foreach (var ofic in oficios)
                {
                    var docInfoDigital = firmador.ConsultaInfoDoc(ofic.CodigoDocFirmado, null, true);
                    if (docInfoDigital.Codigo > 0 && (docInfoDigital.Extra is DocumentResult docDetail) &&
                        docDetail.state == 2 && !string.IsNullOrWhiteSpace(docDetail.File))
                    {
                        // El documento del oficio está firmado. Se sube el documento al repositorio de Gedoc, se crea el despacho, y se cambia el estado del oficio
                        ofic.Accion = "FIRMADODIGITAL";
                        ofic.DatosUsuarioActual = datosUsuario;
                        ofic.datosArchivo = new DatosArchivo();
                        ofic.datosArchivo.FileTextContent = docDetail.File;
                        resultado = SaveOficio(ofic, null);
                        resultado.Mensaje = resultado.Codigo < 0 ? ofic.NumeroOficio : resultado.Mensaje;
                        resultados.Add(resultado);
                    }
                }

                var textoNoOk = resultados.Any(r => r.Codigo < 0)
                    ? "No fue posible actualizar el estado de los siguientes oficios ya firmados:<br/>" +
                      string.Join("<br/>", resultados.Where(r => r.Codigo < 0).Select(r => r.Mensaje).ToList()) +
                      "<br/>Se reintentará la operación más adelante."
                    : "";
                var textoOk = resultados.Any(r => r.Codigo > 0)
                    ? "Se actualizó el estado de los siguientes oficios ya firmados:<br/>" +
                      string.Join("<br/>", resultados.Where(r => r.Codigo > 0).Select(r => r.Mensaje).ToList())
                    : "";

                resultado = string.IsNullOrEmpty(textoNoOk) && string.IsNullOrEmpty(textoOk)
                    ? new ResultadoOperacion(0, "", null)
                    : new ResultadoOperacion(1, textoOk + "<br/>" + textoNoOk, null);

            }
            catch (Exception exc)
            {
                resultado = new ResultadoOperacion(-1, "Ha ocurrido un error al actualizar los oficios pendientes de firma.", null);
            }
            return resultado;
        }

        public DatosArchivo GetArchivo(int oficioId)
        {
            var result = new DatosArchivo
            {
                FileStream = null,
                Mensaje = "Error"
            };
            try
            {
                // Datos del oficio
                var oficio = _repoDespacho.GetOficoById(oficioId);
                if (oficio == null)
                {
                    result.Mensaje = "No se encontró el Oficio especificado.";
                    return result;
                }

                result.OrigenId = oficioId;
                result.OrigenCodigo = /*despacho.NumeroOficio ?? */ oficio.Id.ToString();
                result.FileName = oficio.NombreArchivo;
                result.FilePath = oficio.UrlArchivo;
                result.TipoArchivo = TiposArchivo.Oficio;
                var fileHandler = new FileHandler();
                fileHandler.GetFileStream(result);
            }
            catch (Exception ex)
            {
                LogError(ex);
                result.Mensaje = "Ha ocurrido un error al descargar el archivo, por favor chequee el log de errores de la aplicación.";
            }
            return result;
        }

        public byte[] GetOficioPdfFromHtmlById(int oficioId, string baseUrl)
        {
            var oficio = _repoDespacho.GetOficoById(oficioId);
            if (oficio != null)
            {
                return GetPdfFromHtml(oficio.Contenido, baseUrl);
            }

            return new byte[0];
        }

        public byte[] GetPdfFromHtml(string contenido, string baseUrl)
        {
            if (!string.IsNullOrWhiteSpace(contenido))
            {
                var oficio = new OficioDto
                {
                    Contenido = contenido
                };
                SeparaEncabezadoPiePagina(oficio);
                var pdfData = new PdfData
                {
                    HeaderHtml = oficio.Encabezado,
                    FooterHtml = oficio.Pie,
                    Html = contenido,
                    BaseUrl = baseUrl,
                    PageSize = string.IsNullOrWhiteSpace(WebConfigValues.PdfOficioPaperSize) ? "A4" : WebConfigValues.PdfOficioPaperSize,
                    MargenSup = string.IsNullOrWhiteSpace(WebConfigValues.PdfOficioMargenSup) ? "5pt" : WebConfigValues.PdfOficioMargenSup,
                    MargenDer = string.IsNullOrWhiteSpace(WebConfigValues.PdfOficioMargenDer) ? "40pt" : WebConfigValues.PdfOficioMargenDer,
                    MargenInf = string.IsNullOrWhiteSpace(WebConfigValues.PdfOficioMargenInf) ? "5pt" : WebConfigValues.PdfOficioMargenInf,
                    MargenIzq = string.IsNullOrWhiteSpace(WebConfigValues.PdfOficioMargenIzq) ? "40pt" : WebConfigValues.PdfOficioMargenIzq
                };
                var pdfArr = _pdfSvc.HtmlToPdf(pdfData);
                return pdfArr;
            }

            return new byte[0];
        }

        #endregion

        #region Historial de oficios
        public DatosAjax<List<LogSistemaDto>> HistorialOficio(int oficioId)
        {
            var resultadoOper = new ResultadoOperacion();
            var resultado = new DatosAjax<List<LogSistemaDto>>(new List<LogSistemaDto>(), resultadoOper);
            try
            {
                var datos = _repoDespacho.HistorialOficio(oficioId);
                resultado.Data = datos;
                resultado.Total = datos?.Count ?? 0;
            }
            catch (Exception ex)
            {
                LogError(resultadoOper, ex, "Lo sentimos, ha ocurrido un error al obtener los datos.");
            }
            resultado.Resultado = resultadoOper;
            return resultado;

        }

        #endregion


    }
}
