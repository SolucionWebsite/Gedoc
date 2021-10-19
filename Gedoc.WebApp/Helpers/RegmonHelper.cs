using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Gedoc.Helpers.Dto;
using Gedoc.Helpers.Logging;
using Gedoc.Service.DataAccess.Interfaces;
using Gedoc.WebApp.WssRegMonSrvReference;

namespace Gedoc.WebApp.Helpers
{
    public class RegmonHelper
    {
        private readonly IMantenedorService _mantenedorSrv;

        public RegmonHelper(IMantenedorService mantenedorSrv)
        {
            _mantenedorSrv = mantenedorSrv;
        }

        public MonumentoNacionalDto GetDatosMn(string codigoMn)
        {
            var datosMn = new MonumentoNacionalDto();
            try
            {
                var regMonCli = new MonumentoServiceSoapClient();
                System.Net.ServicePoint servicePoint =
                    System.Net.ServicePointManager.FindServicePoint(regMonCli.Endpoint.Address.Uri);
                servicePoint.Expect100Continue = false;
                var datoRegmon = regMonCli.Consultar(codigoMn);

                if (datoRegmon.Id < 0)
                {
                    return datosMn;
                }
                datosMn.CategoriaMonumentoNacCod = datoRegmon.CategoriaMonumento.NombreCorto;
                datosMn.CodigoMonumentoNac = datoRegmon.Codigo;
                datosMn.DenominacionOficial = datoRegmon.DenominacionOficial;
                datosMn.OtrasDenominaciones = datoRegmon.OtrasDenominaciones;
                datosMn.NombreUsoActual = datoRegmon.NombreUsoActual;
                var tipoVia = GetValorAgrupadorRegmon(datoRegmon.Agrupadores, "ubicacion", "tipo_de_via");
                tipoVia = tipoVia.Contains("]=") ? tipoVia.Replace(tipoVia.Substring(0, tipoVia.IndexOf("]=") + 2), "") : tipoVia;
                var nombreVia = GetValorAgrupadorRegmon(datoRegmon.Agrupadores, "ubicacion", "nombre_via");
                var numero = GetValorAgrupadorRegmon(datoRegmon.Agrupadores, "ubicacion", "n°");
                datosMn.DireccionMonumentoNac = tipoVia + " " + nombreVia + " " + numero;
                datosMn.ReferenciaLocalidad = GetValorAgrupadorRegmon(datoRegmon.Agrupadores, "ubicacion", "referencia_de_localizacion_o_localidad");
                var region = GetValorAgrupadorRegmon(datoRegmon.Agrupadores, "ubicacion", "nombre_region");
                datosMn.RegionCod = GetIdFromDatoRegmon(region);
                var provincia = GetValorAgrupadorRegmon(datoRegmon.Agrupadores, "ubicacion", "provincia");
                datosMn.ProvinciaCod = GetIdFromDatoRegmon(provincia);
                var comuna = GetValorAgrupadorRegmon(datoRegmon.Agrupadores, "ubicacion", "comuna");
                datosMn.ComunaCod = GetIdFromDatoRegmon(comuna);
                datosMn.RolSii = GetRolSiiRegmon(datoRegmon.Grillas);
                // Se necesita obtener los id de Región, Provincia y Comuna en base al código genérico
                datosMn.RegionCod = _mantenedorSrv.GetRegionesByCodigos(datosMn.RegionCod).Select(r => r.Id).ToList();
                datosMn.ProvinciaCod = _mantenedorSrv.GetProvinciasByCodigos(datosMn.ProvinciaCod).Select(r => r.Id).ToList();
                datosMn.ComunaCod = _mantenedorSrv.GetComunasByCodigos(datosMn.ComunaCod).Select(r => r.Id).ToList();
            }
            catch (Exception exc)
            {
                Logger.LogError(exc);
                return null;
            }
            return datosMn;
        }

        private static List<string> GetIdFromDatoRegmon(string datoRegmon)
        {
            // datoRegmon debe venir en la forma "[101]=Llanquihue;[104]=Palena"
            var result = new List<string>();
            if (!string.IsNullOrEmpty(datoRegmon))
            {
                var datoArr = datoRegmon.Split(';');
                for (var i = 0; i < datoArr.Length; i++)
                {
                    var cod = datoArr[i].Substring(0, datoArr[i].IndexOf("="));
                    cod = cod.Replace("[", "").Replace("]", "");
                    result.Add(cod);
                }
            }

            return result;
        }

        public static string GetValorAgrupadorRegmon(AgrupadorDto[] agrupadores, string nombreAgrupador, string nombreValor)
        {
            var valor = "";
            if (agrupadores == null) return valor;
            foreach (var agrup in agrupadores)
            {
                if (nombreAgrupador == agrup.NombreUnico)
                {
                    foreach (var valorC in agrup.Valores)
                    {
                        if (nombreValor == valorC.NombreUnico)
                        {
                            return valorC.Valor;
                        }
                    }
                }
            }
            return valor;
        }

        public static string GetRolSiiRegmon(GrillaDto[] grillas)
        {
            var rol = "";
            if (grillas == null) return "";
            foreach (var grilla in grillas)
            {
                if (grilla.Filas == null) continue;
                foreach (var fila in grilla.Filas)
                {
                    if (fila.Valores == null) continue;
                    foreach (var valor in fila.Valores)
                    {
                        if (valor.NombreUnico == "rol_sii_propiedad") return valor.Valor;
                    }
                }
            }
            return rol;
        }




    }
}