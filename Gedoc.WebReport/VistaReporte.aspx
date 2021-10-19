<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="VistaReporte.aspx.cs" Inherits="Gedoc.WebReport.VistaReporte" %>

<%@ Register Assembly="Microsoft.ReportViewer.WebForms, Version=11.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91"
Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
    <style type="text/css">
        .tablafiltros {
            border-spacing: 10px;
            border-collapse: separate;
        }

        .paneles { padding-left: 10px; }

        #tabla-reportes, #tabla-reportes th, #tabla-reportes td {
            border: 1px solid whitesmoke;
            font-size: 10pt !important;
        }

        #tabla-reportes { width: 100%; }

        #tabla-reportes th {
            padding-bottom: 10px;
            padding-top: 10px;
            text-align: center;
        }

        #tabla-reportes td {
            padding-bottom: 10px;
            padding-top: 10px;
            line-height: 125%;
        }

        /*.s4-ca .s4-notdlg {
        display: none !important;
    }*/

        .categ-mon { max-width: 350px; }

        .gdoc-formlabel { vertical-align: top; }

        .help-block { font-size: 0.8em; }

        div.k-widget.k-window.k-dialog {
            top: 50px !important;
        }
    </style>

    <script src="scripts/kendo/2020.1.114/jquery.min.js"></script>
    <script src="Scripts/kendo/2020.1.114/jszip.min.js"></script>
    <script src="Scripts/kendo/2020.1.114/kendo.all.min.js"></script>
    <script src="Scripts/kendo/2020.1.114/kendo.aspnetmvc.min.js"></script>
    <script src="Scripts/bootstrap.min.js"></script>
    <script src="Scripts/kendo/2020.1.114/cultures/kendo.culture.es-ES.min.js"></script>
    <script src="Scripts/kendo/2020.1.114/messages/kendo.messages.es-ES.min.js"></script>
    <script src="libs/moment/moment-with-locales.min.js"></script>
    <script src="libs/loading-overlay/loadingoverlay.min.js"></script>

    <%-- <script src="Scripts/main.js"></script>--%>

    <%--<link href="libs/fontawesome5-pro/css/all.min.css" rel="stylesheet" />--%>
    <link href="Content/kendo/2020.1.114/kendo.bootstrap-v4.min.css" rel="stylesheet" type="text/css"/>
    <link href="Content/bootstrap.min.css" rel="stylesheet" type="text/css"/>
    <link href="Content/Site.css" rel="stylesheet" type="text/css"/>

    <script src="Scripts/kendo.modernizr.custom.js"></script>

    <script src="Scripts/kendo/2020.1.114/cultures/kendo.culture.es-CL.min.js"></script>
    <script src="Scripts/kendo/2020.1.114/messages/kendo.messages.es-CL.min.js"></script>

    <script src="libs/moment/moment-with-locales.min.js"></script>

</head>
<body>
<form id="form1" runat="server">

<asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>


<%--campos search de multiselecion--%>


<br/>
<br/>
<h4>
    <asp:Label ID="lbTituloReporte" runat="server" Text=""></asp:Label>
</h4>
<asp:Panel ID="pnDocumento" runat="server" GroupingText="Filtros" Font-Bold="False" CssClass="paneles" Visible="False">
    <div id="rowTitulosFechas" class="row">
        <div class="col-sm-2" runat="server" id="tdFechaDesde" visible="false">
            <label class="gdoc-formlabel">Fecha Desde</label>
        </div>
        <div class="col-sm-2" runat="server" id="tdFechaHasta" visible="false">
            <label class="gdoc-formlabel">Fecha Hasta</label>
        </div>
    </div>
    <div id="rowFechas" class="row">

        <div class="col-sm-2" runat="server" id="tdfFechaDesde" visible="false">

            <input id="txtFechaDesde" runat="server" style="width: 100%"/>

            <div id="txtFechaRango" title="Rango de fecha"></div>


        </div>
        <div class="col-sm-2" runat="server" id="tdfFechaHasta" visible="false">

            <input id="txtFechaHasta" runat="server" style="width: 100%"/>

        </div>
        <div class="col-sm-7" id="txtMensajeFecha" runat="server" visible="false" style="line-height: 1;">
            <span class="help-block">
                Debido a la cantidad de información en el sistema, se debe utilizar trazos de tiempo inferiores a los 5 meses en los filtros. En caso de solicitar sólo un día se recomienda dejar marcados dos, por ejemplo Fecha desde 14/05/2018 al 15/05/2018
            </span>
        </div>
    </div>
    <div id="rowTitulos" class="row">
        <div class="col-sm-3" runat="server" id="tdUnidad" visible="false">
            <label class="gdoc-formlabel">Unidad Técnica</label>
        </div>
        <div class="col-sm-3" runat="server" id="tdUnidadCopia" visible="false">
            <label class="gdoc-formlabel">UT en Copia</label>
        </div>
        <div class="col-sm-3" runat="server" id="tdProfesional" visible="false">
            <label class="gdoc-formlabel">Profesional</label>
        </div>
        <div class="col-sm-2" runat="server" id="tdEstado" visible="false">
            <label class="gdoc-formlabel">Estado</label>
        </div>
        <div class="col-sm-3" runat="server" id="tdReq" visible="false">
            <label class="gdoc-formlabel">Requerimiento</label>
        </div>
        <div class="col-sm-3" runat="server" id="tdSesion" visible="false">
            <label class="gdoc-formlabel">Sesión</label>
        </div>
        <div class="col-sm-3" runat="server" id="tdEtapa" visible="false">
            <label class="gdoc-formlabel">Etapa</label>
        </div>
        <div class="col-sm-3" runat="server" id="tdEtiqueta" visible="false">
            <label class="gdoc-formlabel">Etiqueta</label>
        </div>
        <div class="col-sm-3" runat="server" id="tdRemitente" visible="false">
            <label class="gdoc-formlabel">Remitente</label>
        </div>
        <div class="col-sm-3" runat="server" id="tdDestinatario" visible="false">
            <label class="gdoc-formlabel">Destinatario</label>
        </div>
    </div>
    <div id="rowControles" class="row">
        <div class="col-sm-3" runat="server" id="tdfUnidad" visible="false">
            <asp:TextBox ID="tbUnidadTecnica" runat="server" CssClass="search-unidad form-control" Width="100%" Visible="False"></asp:TextBox>
        </div>
        <div class="col-sm-3" runat="server" id="tdfUnidadCopia" visible="false">
            <asp:TextBox ID="tbUnidadTecnicaCopia" runat="server" CssClass="search-unidadcopia form-control" Width="100%" Visible="False"></asp:TextBox>
        </div>
        <div class="col-sm-3" runat="server" id="tdfProfesional" visible="false">
            <asp:TextBox ID="tbProfesionalArea" runat="server" CssClass="search-profesional form-control" Width="100%" Visible="False"></asp:TextBox>
        </div>
        <div class="col-sm-2" runat="server" id="tdfEstado" visible="false">
            <asp:TextBox ID="tbEstado" runat="server" CssClass="search-estado form-control" Width="100%" Visible="False"></asp:TextBox>
        </div>
        <%-- <input id="tbEstado" runat="server" style="width: 100%" />--%>
        <div class="col-sm-3" id="tdfReq" runat="server" visible="false">
            <asp:TextBox ID="tbRequerimiento" runat="server" CssClass="search-requerimiento form-control" Width="100%"></asp:TextBox>
        </div>
        <div class="col-sm-3" id="tdfSesion" runat="server" visible="false">
            <asp:TextBox ID="tbSesion" runat="server" CssClass="search-sesion form-control" Width="100%"></asp:TextBox>
        </div>

        <div class="col-sm-3" runat="server" id="tdfEtapa" visible="false">
            <asp:TextBox ID="tbEtapa" runat="server" CssClass="search-etapa form-control" Width="100%" Visible="False"></asp:TextBox>
        </div>
        <div class="col-sm-3" runat="server" id="tdfEtiqueta" visible="false">
            <asp:TextBox ID="tbEtiqueta" runat="server" CssClass="search-etiqueta form-control" Width="100%" Visible="False"></asp:TextBox>
        </div>
        <div class="col-sm-3" runat="server" id="tdfRemitente" visible="false">
            <asp:TextBox ID="tbRemitente" runat="server" CssClass="search-remitente form-control" Width="100%" Visible="False"></asp:TextBox>
        </div>
        <div class="col-sm-3" runat="server" id="tdfDestinatario" visible="false">
            <asp:TextBox ID="tbDestinatario" runat="server" CssClass="search-destinatario form-control" Width="100%" Visible="False"></asp:TextBox>
        </div>
    </div>

    <div id="rowTitulosFiltrosMn" class="row" visible="false" runat="server">
        <div class="col-sm-3" runat="server" id="titCatgMn">
            <label class="gdoc-formlabel">Categoría de Monumento</label>
        </div>
        <div class="col-sm-3" runat="server" id="titNombreMn">
            <label class="gdoc-formlabel">Nombre de Monumento</label>
        </div>
    </div>
    <div id="rowFiltrosMn" class="row" visible="false" runat="server">
        <div class="col-sm-3">
            <asp:TextBox ID="tbTipoMon" runat="server" CssClass="search-tipomon categ-mon" Width="100%"></asp:TextBox>
        </div>

        <div class="col-sm-3">
            <asp:TextBox ID="tbNombreMn" runat="server" Width="100%" CssClass="form-control"></asp:TextBox>
        </div>
    </div>

    <div class="row" id="rowPieReportes" style="margin-top: 25px; min-width: 1200px;">
        <div class="col-sm-offset-3 col-sm-2">

            <asp:Button ID="btnVer" CssClass="btn btn-primary animate" runat="server" Text="Ver Reporte" OnClientClick="return ValidacionForm();" OnClick="btnVer_Click"/>
            <%--            <asp:Button ID="btnVerReporte" runat="server" CssClass="btn btn-primary animate" OnClick="btnVerReporte_Click" Text="Ver Reporte" />--%>
        </div>
        <div class="col-sm-offset-1 col-sm-1">
            <asp:Button ID="btnVolver" runat="server" CssClass="btn btn-success animate" Text="Volver" OnClientClick="return btnVolverClick();" UseSubmitBehavior="False"/>
        </div>
        <div class="col-sm-2">
            <asp:Button ID="btnLimpiarFiltros" runat="server" CssClass="btn btn-success animate" Text="Limpiar Filtros" OnClientClick="limpiarFiltros(event);return false;" CausesValidation="True"/>
        </div>
    </div>
</asp:Panel>

<rsweb:ReportViewer ID="rpReporte" runat="server" Height="900px"
                    Style="        width: 100% !important;
        display: table !important;
        margin: 0px;
        overflow: auto !important;"
                    AsyncRendering="false"
                    KeepSessionAlive="true">
</rsweb:ReportViewer>

<div runat="server" id="divError" visible="False" class="mt-5">
    <b style="color: red;">Ha ocurrido un error al obtener el reporte, por favor revise el fichero log de la aplicación.</b>
</div>

<%--<div id="SeccionReporte" style="overflow: visible;">
    
  


      


</div>--%>


<asp:HiddenField ID="hfReporte" runat="server"/>
<asp:HiddenField ID="hfFechaDesde" runat="server"/>
<asp:HiddenField ID="hfFechaHasta" runat="server"/>
<asp:HiddenField ID="hfSesionTabla" runat="server"/>
<asp:HiddenField ID="hfUnidadTecnica" runat="server"/>
<asp:HiddenField ID="hfUnidadTecnicaCopia" runat="server"/>
<asp:HiddenField ID="hfEstado" runat="server"/>
<asp:HiddenField ID="hfTipoMon" runat="server"/>
<asp:HiddenField ID="hfProfesional" runat="server"/>
<asp:HiddenField ID="hfEtiqueta" runat="server"/>
<asp:HiddenField ID="hfErrorDetail" runat="server"/>


<script type="text/javascript">


    //var siteUrl = "http://gedoc.monumentos.cl";
    //var url = siteUrl + "/_layouts/dibam.cmn/AjaxHandler.ashx";

    function getMensajeMultiselect() {
        var msg = {
            clear: "limpiar",
            noData: "No hay datos",
            deleteTag: "borrar",
            singleTag: "item(s) seleccionados"
        };
        return msg;
    };

    function getMensajeSelect() {
        var msg = {
            clear: "limpiar",
            noData: "No hay datos"
        };
        return msg;
    };


    $(document).ready(function () {
        var error = $('#<%= hfErrorDetail.ClientID %>').val();
        if (error) {
            console.error(error);
        }

        reporte = $('#<%= hfReporte.ClientID %>').val();

        var Siteurl = GetUrl();

        $("#txtFechaDesde").kendoDatePicker({
            culture: "es-CL",
            format: "dd/MM/yyyy"
        });

        $("#txtFechaHasta").kendoDatePicker({
            culture: "es-CL",
            format: "dd/MM/yyyy"
        });

        $("#tbTipoMon").kendoMultiSelect({
            placeholder: "Seleccione Tipo Monumento...",
            messages: getMensajeMultiselect(),
            dataTextField: "Nombre",
            dataValueField: "Id",
            dataSource: {
                transport: {
                    dataType: "json",
                    read: Siteurl +
                        "/Api/Generico/<%= (int) Gedoc.WebReport.Enums.ListasGenericas.TipoMonumento %>"
                }
            },
            filter: 'contains',
            value: $('#<%= hfTipoMon.ClientID %>').val() ? $('#<%= hfTipoMon.ClientID %>').val().split(",") : []
        });

        $("#tbEtapa").kendoMultiSelect({
            placeholder: "Seleccione Etapa...",
            messages: getMensajeMultiselect(),
            dataTextField: "Nombre",
            dataValueField: "Id",
            dataSource: {
                transport: {
                    dataType: "json",
                    read: Siteurl + "/Api/Generico/<%= (int) Gedoc.WebReport.Enums.ListasGenericas.Etapas %>"
                }
            },
            filter: 'contains',

        });


        $("#tbEstado").kendoMultiSelect({
            placeholder: "Seleccione Estado...",
            messages: getMensajeMultiselect(),
            dataTextField: "Nombre",
            dataValueField: "Id",
            dataSource: {
                transport: {
                    dataType: "json",
                    read: Siteurl + "/Api/Generico/<%= (int) Gedoc.WebReport.Enums.ListasGenericas.Estados %>"
                }
            },
            filter: 'contains',
            value: $('#<%= hfEstado.ClientID %>').val() ? $('#<%= hfEstado.ClientID %>').val().split(",") : []

        });

        if (reporte == "Reporte_Tabla_de_Comision.rdl" || reporte == "Reporte_Requerimientos.rdl") {
            $("#tbUnidadTecnica").kendoComboBox({
                placeholder: "Seleccione Unidad Técnica...",
                messages: getMensajeSelect(),
                dataTextField: "Nombre",
                dataValueField: "Id",
                dataSource: {
                    transport: {
                        dataType: "json",
                        read: Siteurl +
                            "/Api/Generico/<%= (int) Gedoc.WebReport.Enums.ListasGenericas.UnidadTecnia %>"
                    }
                },
                filter: "contains",
                change: function (e) {
                    var sesionK = $("#tbSesion").data("kendoComboBox");
                    if (sesionK) {
                        $("#tbSesion").data("kendoComboBox").value(null);
                        sesionK.dataSource.read();
                    }
                }
            });
            if ($('#hfUnidadTecnica').val())
                $("#tbUnidadTecnica").data("kendoComboBox").value($('#hfUnidadTecnica').val());
        } else {
            $("#tbUnidadTecnica").kendoMultiSelect({
                placeholder: "Seleccione Unidad Técnica...",
                messages: getMensajeMultiselect(),
                dataTextField: "Nombre",
                dataValueField: "Id",
                dataSource: {
                    transport: {
                        dataType: "json",
                        read: Siteurl +
                            "/Api/Generico/<%= (int) Gedoc.WebReport.Enums.ListasGenericas.UnidadTecnia %>"
                    }
                },
                filter: 'contains',
                value: $('#<%= hfUnidadTecnica.ClientID %>').val()
                    ? $('#<%= hfUnidadTecnica.ClientID %>').val().split(",")
                    : []
            });
        }
        if (reporte == "Reporte_Tabla_de_Comision.rdl") {
            var inicializando = true;
            $("#tbSesion").kendoComboBox({
                //placeholder: "",
                messages: getMensajeSelect(),
                dataTextField: "Nombre",
                dataValueField: "Id",
                dataSource: {
                    transport: {
                        dataType: "json",
                        read: {
                            url: Siteurl + "/Api/Generico",
                            data: function (e) {
                                var utK = $("#tbUnidadTecnica").data("kendoComboBox");
                                var utValue = -1;
                                if (inicializando) {
                                    inicializando = false;
                                    if ($('#hfUnidadTecnica').val())
                                        utValue = $('#hfUnidadTecnica').val();
                                } else {
                                    utValue = utK && utK.selectedIndex >= 0 ? utK.value() : -1
                                }
                                return {
                                    id: <%= (int) Gedoc.WebReport.Enums.ListasGenericas.SesionTabla %>,
                                    idPadre: utValue
                                }
                            }
                        }
                    }
                },
                filter: "contains"
            });
            if ($('#hfSesionTabla').val()) {
                $("#tbSesion").data("kendoComboBox").value($('#hfSesionTabla').val());
            }
        }

        if (reporte == "Reporte_Requerimientos.rdl") {
            $("#tbProfesionalArea").kendoComboBox({
                placeholder: "Seleccione Profesional...",
                messages: getMensajeSelect(),
                dataTextField: "Nombre",
                dataValueField: "Id",
                cascadeFrom: "tbUnidadTecnica",
                autoBind: false,
                dataSource: {
                    serverFiltering: true, // tiene q ser true para q funcione el "cascadeFrom"
                    transport: {
                        dataType: "json",
                        read: {
                            url: Siteurl + "/Api/Generico",
                            data: function (e) {
                                var utK = $("#tbUnidadTecnica").data("kendoComboBox");
                                return {
                                    id: <%= (int) Gedoc.WebReport.Enums.ListasGenericas.Profesionales %>,
                                    idPadre: utK ? utK.value() : 0
                                }
                            }
                        }
                    }
                },
                filter: "contains"
            });
            if (!$('#hfProfesional').val())
                $("#tbProfesionalArea").data("kendoComboBox").value("");
        } else {
            $("#tbProfesionalArea").kendoMultiSelect({
                placeholder: "Seleccione Profesional...",
                messages: getMensajeMultiselect(),
                dataTextField: "Nombre",
                dataValueField: "Id",
                dataSource: {
                    transport: {
                        dataType: "json",
                        read: Siteurl +
                            "/Api/Generico/<%= (int) Gedoc.WebReport.Enums.ListasGenericas.Profesionales %>"
                    },
                },
                filter: 'contains',
                value: $('#<%= hfProfesional.ClientID %>').val()
                    ? $('#<%= hfProfesional.ClientID %>').val().split(",")
                    : []

            });
        }

        $("#tbUnidadTecnicaCopia").kendoMultiSelect({
            placeholder: "Seleccione Unidad Técnica en Copia...",
            messages: getMensajeMultiselect(),
            dataTextField: "Nombre",
            dataValueField: "Id",
            dataSource: {
                transport: {
                    dataType: "json",
                    read: Siteurl +
                        "/Api/Generico/<%= (int) Gedoc.WebReport.Enums.ListasGenericas.UnidadTecnicaCopia %>"
                }
            },
            filter: 'contains',
            value: $('#<%= hfUnidadTecnicaCopia.ClientID %>').val()
                ? $('#<%= hfUnidadTecnicaCopia.ClientID %>').val().split(",")
                : []

        });

        $("#tbEtiqueta").kendoMultiSelect({
            placeholder: "Seleccione Etiqueta...",
            messages: getMensajeMultiselect(),
            dataTextField: "Nombre",
            dataValueField: "Id",
            dataSource: {
                transport: {
                    dataType: "json",
                    read: Siteurl + "/Api/Generico/<%= (int) Gedoc.WebReport.Enums.ListasGenericas.Etiqueta %>"
                }
            },
            filter: 'contains',
            value: $('#<%= hfEtiqueta.ClientID %>').val()
                ? $('#<%= hfEtiqueta.ClientID %>').val().split(",")
                : []

        });


        //$('.search-unidad').attr("hidden-field", "hfUnidadTecnica");
        //$('.search-unidadcopia').attr("hidden-field", "hfUnidadTecnicaCopia");
        //$('.search-profesional').attr("hidden-field", "hfProfesional");
        //$('.search-estado').attr("hidden-field", "hfEstado");
        //$('.search-etapa').attr("hidden-field", "hfEtapa");
        //$('.search-etiqueta').attr("hidden-field", "hfEtiqueta");
        //$('.search-remitente').attr("hidden-field", "hfRemitente");
        //$('.search-destinatario').attr("hidden-field", "hfDestinatario");
        //$('.search-sesion').attr("hidden-field", "hfSesion");
        //$('.search-requerimiento').attr("hidden-field", "hfRequerimiento");
        //$('.search-tipomon').attr("hidden-field", "hfTipoMon");


        if (reporte == "Reporte_Tabla_de_Comision.rdl" || reporte == "Reporte_Tabla_de_Sesion.rdl") {
            //seteaSearchSimple('.search-unidad', url + "?Opcion=Lista&Tipo=UTReportes&all=NO&PrimerItem=(Seleccione una unidad)", 'Id', '%Titulo%', ['Titulo'], UnidadChange);

        } else if (reporte == "Reporte_Requerimientos.rdl") {
            //seteaSearchSimple('.search-unidad', url + "?Opcion=Lista&Tipo=UTReportes&all=SI", 'Id', '%Titulo%', ['Titulo'], UnidadRespChange);

        } else {
            //seteaSearchMulti('.search-unidad', url + "?Opcion=Lista&Tipo=UTReportes&all=SI", 'Id', '%Titulo%', ['Titulo'], asignaHiddenField);
        }

        <%-- //seteaSearchMulti('.search-profesional', url + "?Opcion=Lista&Tipo=Usuarios&UnidadId=<%= tbUnidadTecnica.Text %>&all=SI", 'Id', asignaHiddenField);
        seteaSearchMulti('.search-estado', url + "?Opcion=Lista&Tipo=Estado&all=SI", 'Id', '%Titulo%', ['Titulo'], asignaHiddenField);
        seteaSearchMulti('.search-etapa', url + "?Opcion=Lista&Tipo=Etapa", 'Id', '%Titulo%', ['Titulo'], asignaHiddenField);
        seteaSearchMulti('.search-etiqueta', url + "?Opcion=Lista&Tipo=Etiqueta", 'Id', '%Titulo%', ['Titulo'], asignaHiddenField);
        seteaSearchMulti('.search-remitente', url + "?Opcion=Lista&Tipo=Remitentes", 'Id', '%Nombre%', asignaHiddenField);
        seteaSearchMulti('.search-destinatario', url + "?Opcion=Lista&Tipo=DestCopia", 'Id', '%NConInst%', asignaHiddenField);
        seteaSearchMulti('.search-unidadcopia', url + "?Opcion=Lista&Tipo=UtCopia", 'Id', '%Titulo%', ['Titulo'], asignaHiddenField);
        // Search de Tipo de monumento 
        $('.search-tipomon').css('max-width', $('.search-tipomon').parent().width());
        seteaSearchMulti('.search-tipomon', url + "?Opcion=Lista&Tipo=TipoMonu&PrimerItem=[TODOS]", 'Titulo', '%Titulo%', ['Titulo'], successCategoriaMon, false, false, afterDeleteCategoriaMon);

        ConfigSearchSesion();
            // Search de Requerimiento
        ConfigSearchReq();
            //}
            //else {
            //    if (reporte == "Reporte_Requerimientos.rdl") {
            //        seteaSearchMulti('.search-estado', url + "?Opcion=Lista&Tipo=Estado", 'Id', '%Titulo%', ['Titulo'], asignaHiddenField);
            //        seteaSearchSimple('.search-unidad', url + "?Opcion=Lista&Tipo=UTReportes", 'Id', '%Titulo%', ['Titulo'], UnidadRespChange);
        ConfigSearchProfUtResp(false);
            //    }

            //}

        $("input[id*='txtFechaDesdeDate']").width(125);
        $("input[id*='txtFechaHasta']").width(125);
        $("input[id*='tbProfesionalArea']").width('auto');
        $("#s4-ca").width('100%');--%>

        //ActualizaEstadoEtl();
    });

    function mostrarReporteTabla() {
        <%--        var btnID = '<%=btnVerReporte.UniqueID %>';
        __doPostBack(btnID, '');--%>
    }


    function ValidacionForm2() {
        //Validaciones de Reportes

        $("#divError").hide();

        if (!validaRangoFechas()) {
            return false;
        }

        $.LoadingOverlay("show", {});





        return true;

    }


    function ValidacionForm() {
        //Validaciones de Reportes

        $("#divError").hide();

        if (!validaRangoFechas()) {
            return false;
        }

        var _reporte = $('#hfReporte').val();
        var _fechadesde = !$("#txtFechaDesde").length
            ? ""
            : $("#txtFechaDesde").data("kendoDatePicker").value() || ""
        var _fechahasta = !$("#txtFechaHasta").length
            ? ""
            : $("#txtFechaHasta").data("kendoDatePicker").value() || "";
        var _ut = !$("#tbUnidadTecnica").length
            ? []
            : ((reporte == "Reporte_Tabla_de_Comision.rdl" || reporte == "Reporte_Requerimientos.rdl")
                ? $("#tbUnidadTecnica").data("kendoComboBox").value().split('*')
                : $("#tbUnidadTecnica").data("kendoMultiSelect").value()) || [];
        var _utCopia = !$("#tbUnidadTecnicaCopia").length
            ? []
            : $("#tbUnidadTecnicaCopia").data("kendoMultiSelect").value() || [];
        var _profesional = !$("#tbProfesionalArea").length
            ? []
            : ((reporte == "Reporte_Requerimientos.rdl")
                ? $("#tbProfesionalArea").data("kendoComboBox").value().split('*')
                : $("#tbProfesionalArea").data("kendoMultiSelect").value()) || [];
        var _etiqueta = !$("#tbEtiqueta").length ? [] : $('#tbEtiqueta').data("kendoMultiSelect").value() || [];
        var _session = !$("#tbSesion").length
            ? 0
            : $("#tbSesion").data("kendoComboBox").value() || 0;
        var _estado = !$("#tbEstado").length ? [] : $("#tbEstado").data("kendoMultiSelect").value() || [];
        var _tipomon = !$("#tbTipoMon").length ? [] : $("#tbTipoMon").data("kendoMultiSelect").value() || [];

        //var _etapa = $("#hfEtapa").val();

        $('#<%= hfFechaDesde.ClientID %>').val(_fechadesde);
        $('#<%= hfFechaHasta.ClientID %>').val(_fechahasta);
        $('#<%= hfEstado.ClientID %>').val(_estado.join());
        $('#<%= hfUnidadTecnica.ClientID %>').val(_ut.join());
        $('#<%= hfUnidadTecnicaCopia.ClientID %>').val(_utCopia.join());
        $('#<%= hfTipoMon.ClientID %>').val(_tipomon.join());
        $('#<%= hfProfesional.ClientID %>').val(_profesional.join());
        $('#<%= hfEtiqueta.ClientID %>').val(_etiqueta.join());
        $('#<%= hfSesionTabla.ClientID %>').val(_session);


        if (_reporte == "Reporte_Requerimientos.rdl") {
            if (_fechadesde == "" || _fechahasta == "" || _profesional == "" || _estado == "" || _ut == "") {
                muestraDialog("Error", "Debe ingresar correctamente todos los filtros para ver este reporte", "ERROR");
                return false;
            }
        }

        if (_reporte == "Reporte_Graficos_Estadisticas.rdl") {
            if (_fechadesde == "" || _fechahasta == "" || _estado == "" || _ut == "") {
                muestraDialog("Error", "Debe ingresar correctamente todos los filtros para ver este reporte", "ERROR");
                return false;
            }
        }

        if (_reporte == "Reporte_Requerimientos_Etiquetas.rdl") {
            if (_fechadesde == "" || _fechahasta == "" || _etiqueta == "") {
                muestraDialog("Error", "Debe ingresar correctamente todos los filtros para ver este reporte", "ERROR");
                return false;
            }
        }


        if (_reporte == "Reporte_Requerimientos_EntregasDiarias.rdl" || _reporte == "Reporte_Requerimientos_TimbrajedePlanos.rdl" || _reporte == "Reporte_Requerimientos_Respondidos.rdl") {

            if (_fechadesde == "" || _fechahasta == "" || _ut == "") {
                muestraDialog("Error", "Debe ingresar correctamente todos los filtros para ver este reporte", "ERROR");
                return false;
            }
        }

        if (_reporte == "Reporte_Requerimientos_GrupoAuditoria.rdl") {

            if (_fechadesde == "" || _fechahasta == "" || _ut == "" || _estado == "") {
                muestraDialog("Error", "Debe ingresar correctamente todos los filtros para ver este reporte", "ERROR");
                return false;
            }
        }

        if (_reporte == "Reporte_Requerimientos_DocumentosContablesDiario.rdl" || _reporte == "Reporte_Despachos.rdl" ||
            _reporte == "Reporte_Despachos_IniciativasCMN.rdl" || _reporte == "Reporte_Requerimientos_IngresosDiarios.rdl" ||
            _reporte == "Reporte_Log_Transacciones.rdl" || _reporte == "Reporte_Requerimientos_EnCopia.rdl") {

            if (_fechadesde == "" || _fechahasta == "") {
                muestraDialog("Error", "Debe ingresar correctamente todos los filtros para ver este reporte", "ERROR");
                return false;
            }


            if (_reporte == "Reporte_Requerimientos_EnCopia.rdl" && _utCopia == "") {
                muestraDialog("Error", "Tiene que seleccionar al menos una UT en Copia", "ERROR");
                return false;
            }

        }

        if (_reporte == "Reporte_Tabla_de_Comision.rdl" || _reporte == "Reporte_Tabla_de_Sesion.rdl") {

            if (_ut == "" || _session == "") {
                muestraDialog("Error", "Debe ingresar correctamente todos los filtros para ver este reporte", "ERROR");
                return false;
            }

            fechaDValida = true;
            fechaHValida = true;
            fechaDMayor = false;
        }



        $.LoadingOverlay("show", {});
        return true;

    }


    $('#btnVerReporte').click(function(e) {

        e.preventDefault();

        var pos = $(this).offset().top;

        $('#SeccionReporte').animate({
                top: pos + $(this).height()
            },
            1000);

    });

    function convertDate(fecha) {
        var match = /(\d+)\/(\d+)\/(\d+)/.exec(fecha)
        return new Date(match[3], match[2], match[1]);
    }

    function limpiarFiltros(e) {
        $("#divError").hide();
        e.preventDefault();

        // Fecha Desde
        if ($("#txtFechaDesde").length) {
            $("#txtFechaDesde").data("kendoDatePicker").value(null);
            $("#txtFechaDesde").data("kendoDatePicker").trigger("change");
        }
        // Fecha Hasta
        if ($("#txtFechaHasta").length) {
            $("#txtFechaHasta").data("kendoDatePicker").value(null);
            $("#txtFechaHasta").data("kendoDatePicker").trigger("change");
        }
        // Unidad Técnica
        if ($("#tbUnidadTecnica").length) {
            if (reporte == "Reporte_Tabla_de_Comision.rdl" || reporte == "Reporte_Requerimientos.rdl") {
                $("#tbUnidadTecnica").data("kendoComboBox").value("");
            } else {
                $("#tbUnidadTecnica").data("kendoMultiSelect").value([]);
            }
        }
        // Unidad Técnica en copia
        if ($("#tbUnidadTecnicaCopia").length) {
            $('#<%= tbUnidadTecnicaCopia.ClientID %>').data("kendoMultiSelect").value([]);
        }
        // Profesional en área
        if ($("#tbProfesionalArea").length) {
            if (reporte == "Reporte_Requerimientos.rdl") {
                $("#tbProfesionalArea").data("kendoComboBox").value("");
            } else {
                $("#tbProfesionalArea").data("kendoMultiSelect").value([]);
            }
        }
        // Estado
        if ($("#tbEstado").length) {
            $('#<%= tbEstado.ClientID %>').data("kendoMultiSelect").value([]);
        }
        // Etapa
        if ($("#tbEtapa").length) {
            $('#<%= tbEtapa.ClientID %>').data("kendoMultiSelect").value([]);
        }
        // Categoría MN
        if ($("#tbTipoMon").length) {
            $('#<%= tbTipoMon.ClientID %>').data("kendoMultiSelect").value([]);
        }
        // Etiqueta
        if ($("#tbEtiqueta").length) {
            $('#<%= tbEtiqueta.ClientID %>').data("kendoMultiSelect").value([]);
        }

        // Nombre MN
        $('#<%= tbNombreMn.ClientID %>').val('');
        // Remitente
        $('#<%= tbRemitente.ClientID %>').val('');
        // Destinatario
        $('#<%= tbDestinatario.ClientID %>').val('');
        // Sesion
        $('#<%= tbSesion.ClientID %>').val('');
        // Requerimiento
        $('#<%= tbRequerimiento.ClientID %>').val('');

        return false;
    }

    function CargaDatosEtl() {
        siteUrl = "";
        query = siteUrl + "/_layouts/dibam.cmn/AjaxHandler.ashx?Opcion=CargaDatosEtl";

        $("#estadoSrv").html("Ejecutando carga de datos.");
        $("#cargaEtl").attr("disabled", "disabled");

        $.ajax({
            type: "GET",
            cache: false,
            url: query,
            contentType: "application/json; charset=utf-8"
        }).done(function (resultquery) {
            $("#cargaEtl").removeAttr("disabled");
            ActualizaEstadoEtl();
            showAlert("Carga de datos ejecutada.", null);
        }).fail(function (xhr, result, status) {
            $("#cargaEtl").removeAttr("disabled");
            alert('Advertencia: ' + xhr.statusText + ' ' + xhr.responseText + ' ' + xhr.status);
        });
    }

    function ActualizaEstadoEtl() {
        siteUrl = "";
        query = siteUrl + "/_layouts/dibam.cmn/AjaxHandler.ashx?Opcion=EstadoEtl";

        $.ajax({
            type: "GET",
            cache: false,
            url: query,
            contentType: "application/json; charset=utf-8"
        }).done(function (resultquery) {
            $("#estadoSrv").html(resultquery);
        }).fail(function (xhr, result, status) {
            alert('Advertencia: ' + xhr.statusText + ' ' + xhr.responseText + ' ' + xhr.status);
        });
    }


    function GetUrl() {
        //var urlSite = "http://" + window.location.href.split('/')[2] + "/" + window.location.href.split('/')[3];
        //var urlSite = window.location.origin;
        var urlSite = window.location.href.substring(0, window.location.href.indexOf("VistaReporte"));
        return urlSite;
    }

    function validaRangoFechas() {
        var fechaD = $("#txtFechaDesde").val();
        var fechaH = $("#txtFechaHasta").val();

        if (fechaD && !moment(fechaD, "DD/MM/YYYY").isValid()) {
            muestraDialog("Error", "Por favor, especifique una Fecha Desde válida.", "ERROR");
            return false;
        }

        if (fechaH && !moment(fechaH, "DD/MM/YYYY").isValid()) {
            muestraDialog("Error", "Por favor, especifique una Fecha Hasta válida.", "ERROR");
            return false;
        }

        if (fechaD && fechaH && moment(fechaH, "DD/MM/YYYY").diff(moment(fechaD, "DD/MM/YYYY"), "day") < 0) {
            muestraDialog("Error", "La Fecha Desde tiene que ser anterior a la Fecha Hasta.", "ERROR");
            return false;
        }

        return true;
    }

    function btnVolverClick() {
        // window.history.back();
        // parent.document.location.href = '/Reporte'; // CORS error
        //window.open("/Reporte", "_top");
        parent.postMessage("volverReporte", "*");
        return false;
    }

    function muestraDialog(titulo, mensaje, tipo) {
        var result = true;
        var dfd = $.Deferred();
        var acciones = [
            {
                text: tipo == "CONFIRM_SINO" ? "<span class='button-dlg'>Sí</span>" : "Aceptar",
                cssClass: "button-dlg",
                primary: true,
                action: function (e) {
                    result = true;
                    e.sender.close();
                }
            }];

        if (tipo == "CONFIRM" || tipo == "CONFIRM_SINO") {
            acciones.push({
                text: tipo == "CONFIRM_SINO" ? "<span class='button-dlg'>No</span>" : "Cancelar",
                cssClass: "button-dlg ml-2",
                action: function (e) {
                    result = false;
                    e.sender.close();
                }
            });
        }

        var claseDiv = "modal-msg ";
        var icono = "";
        switch (tipo) {
            case "INFO":
                claseDiv += "info-msg";
                icono = "<span class='k-icon k-icon-48 k-i-information'></span>"; // + mensaje;
                break;
            case "ERROR":
                claseDiv += "error-msg";
                icono = "<span class='k-icon k-icon-48 k-i-close-outline'></span>"; // + mensaje;
                break;
            case "ALERT":
                claseDiv += "alert-msg";
                icono = "<span class='k-icon k-icon-48 k-i-warning'></span>"; // + mensaje;
                break;
            case "CONFIRM":
            case "CONFIRM_SINO":
                claseDiv += "confirm-msg";
                icono = "<span class='k-icon k-icon-48 k-i-question'></span>"; // + mensaje;
                break;

            default:
        }
        var contenido = "<div class='row'> <div class='col col-icono-msg'>" + icono + "</div> <div class='col'>" + mensaje + "</div> </div>";
        var dialog = $("<div id='dialog-msg' class='" + claseDiv + "'></div>").kendoDialog({
            //width: "500px",
            minWidth: 450,
            buttonLayout: "normal",
            title: titulo,
            content: contenido,
            actions: acciones,
            closable: false,
            modal: true,
            visible: false,
            animation: {
                open: {
                    effects: "slideIn:down"
                }
            },
            close: function (e) {
                this.destroy();
                return dfd.resolve(result);
            },
            open: function (e) {
                // cssClass no está funcionando en los botones, por eso esto:
                var wr = $(this)[0].wrapper;
                if (wr) {
                    // $(wr).find(".k-dialog-buttongroup button.k-button").addClass("button-dlg"); // Funciona, pero a pesar de agregarle la clase al botón no toma el width definido en la clase button-dlg, por eso arriba el texto del botón lo pongo en un span con esta clase
                    $(wr).find(".k-dialog-buttongroup button.k-button:not('.k-primary')").addClass("k-danger");
                }
            }
        });
        var dlg = dialog.data("kendoDialog").open();
        return dfd.promise();
    }


</script>
</form>
</body>
</html>