<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="VistaReporteTramites.aspx.cs" Inherits="Gedoc.WebReport.VistaReporteTramites" %>

<%@ Register Assembly="Microsoft.ReportViewer.WebForms, Version=11.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91"
    Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title></title>
    <style type="text/css">
        .tablafiltros {
            border-spacing: 10px;
            border-collapse: separate;
        }

        .paneles {
            padding-left: 10px;
        }

        #tabla-reportes, #tabla-reportes th, #tabla-reportes td {
            border: 1px solid whitesmoke;
            font-size: 10pt !important;
        }

        #tabla-reportes {
            width: 100%;
        }

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

        .categ-mon {
            max-width: 350px;
        }

        .ms-formvalidation {
            color: red;
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

   <%-- <link href="libs/fontawesome5-pro/css/all.min.css" rel="stylesheet" />--%>
    <link href="Content/kendo/2020.1.114/kendo.bootstrap-v4.min.css" rel="stylesheet" type="text/css" />
    <link href="Content/bootstrap.min.css" rel="stylesheet" type="text/css" />
    <link href="Content/Site.css" rel="stylesheet" type="text/css" />

    <script src="Scripts/kendo.modernizr.custom.js"></script>

    <script src="Scripts/kendo/2020.1.114/cultures/kendo.culture.es-CL.min.js"></script>
    <script src="Scripts/kendo/2020.1.114/messages/kendo.messages.es-CL.min.js"></script>

    <script src="libs/moment/moment-with-locales.min.js"></script>

    <style>
        .div-inline {
            display: inline-block;
        }

        .gdoc-formlabel {
            vertical-align: top;
        }

        .row {
            padding-bottom: 8px;
        }
    </style>

</head>
<body>
    <form id="frReporteTramites" runat="server">

        <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>

        <asp:HiddenField ID="hfTipoTramite" runat="server" />
        <asp:HiddenField ID="hfCanalTramite" runat="server" />
        <asp:HiddenField ID="hfNumeroCaso" runat="server" />
        <asp:HiddenField ID="hfNombreCaso" runat="server" />
        <asp:HiddenField ID="hfDocumentoIngreso" runat="server" />
        <asp:HiddenField ID="hfEstadoEtapa" runat="server" />
        <asp:HiddenField ID="hfRemitente" runat="server" />
        <asp:HiddenField ID="hfTipoMon" runat="server" />
        <asp:HiddenField ID="hfComuna" runat="server" />
        <asp:HiddenField ID="hfEtiqueta" runat="server" />
        <asp:HiddenField ID="hfUnidadTecnica" runat="server" />
        <asp:HiddenField ID="hfProfUt" runat="server" />

        <h4>
            <span>Reporte de Trámites y Casos</span>
        </h4>

        <asp:Panel ID="onFiltrosTram" runat="server" GroupingText="Filtros" Font-Bold="False" CssClass="paneles">
            <div class="container-fluid" style="width: auto !important;">


                <div class="row">
                    <div class="col-md-9">

                        <!--Fecha desde y hasta-->

                        <div class="row">
                            <div class="offset-md-4 col-sm-3">
                                <label class="gdoc-formlabel">
                                    Fecha Desde<span class="ms-formvalidation" title="Este campo es obligatorio."> *</span>
                                </label>
                                <div class="div-inline">
                                    <input id="txtFechaDesde" runat="server" style="width: 100%" />
                                </div>
                            </div>
                            <div class="col-sm-3">
                                <label class="gdoc-formlabel">
                                    Fecha Hasta<span class="ms-formvalidation" title="Este campo es obligatorio."> *</span>
                                </label>
                                <div class="div-inline">
                                    <input id="txtFechaHasta" runat="server" style="width: 100%" />
                                </div>
                            </div>
                        </div>

                        <!--Tipo de Tramite-->
                        <div class="row">
                            <div class="col-sm-4 text-right">
                                <label class="gdoc-formlabel">
                                    Tipo de trámite
                                </label>
                            </div>
                            <div class="col-sm-6">
                                <asp:TextBox ID="tbTipoTramite" runat="server" Width="100%"></asp:TextBox>
                            </div>
                        </div>

                        <!--Canal de llegada de tramite-->
                        <div class="row">
                            <div class="col-sm-4 text-right">
                                <label class="gdoc-formlabel">
                                    Canal de llegada del trámite
                                </label>
                            </div>
                            <div class="col-sm-6">
                                <asp:TextBox ID="tbCanalLlega" runat="server" Width="100%"></asp:TextBox>
                            </div>
                        </div>

                        <!--N° de Caso-->
                        <div class="row">
                            <div class="col-sm-4 text-right">
                                <label class="gdoc-formlabel">
                                    N° de Caso
                                </label>
                            </div>
                            <div class="col-sm-6">
                                <asp:TextBox ID="tbNcaso" runat="server" Width="100%"></asp:TextBox>
                            </div>
                        </div>
                        <!--Nombre del caso-->
                        <div class="row">
                            <div class="col-sm-4 text-right">
                                <label class="gdoc-formlabel">
                                    Nombre del caso
                                </label>
                            </div>
                            <div class="col-sm-6">
                                <asp:TextBox ID="tbNombreCaso" runat="server" Width="100%"></asp:TextBox>
                            </div>
                        </div>
                        <!--Documento de ingreso-->
                        <div class="row">
                            <div class="col-sm-4 text-right">
                                <label class="gdoc-formlabel">
                                    Documento Ingreso
                                </label>
                            </div>
                            <div class="col-sm-6">
                                <asp:TextBox ID="tbDocingreso" runat="server" CssClass="search-docing" Width="100%"></asp:TextBox>
                            </div>
                        </div>
                        <!--Estado y Etapa-->
                        <div class="row">
                            <div class="col-sm-4 text-right">
                                <label class="gdoc-formlabel">
                                    Estado y Etapa
                                </label>
                            </div>
                            <div class="col-sm-6">
                                <asp:TextBox ID="tbEstadoyEtapa" runat="server" Width="100%"></asp:TextBox>
                            </div>
                        </div>
                        <!--Remitente-->
                        <div class="row">
                            <div class="col-sm-4 text-right">
                                <label class="gdoc-formlabel">
                                    Remitente
                                </label>
                            </div>
                            <div class="col-sm-6">
                                <asp:TextBox ID="tbRemitente" runat="server" Width="100%"></asp:TextBox>
                            </div>
                        </div>
                        <!--Institucion Remitente-->
                        <div class="row">
                            <div class="col-sm-4 text-right">
                                <label class="gdoc-formlabel">
                                    Institucion Remitente
                                </label>
                            </div>
                            <div class="col-sm-6">
                                <asp:TextBox ID="txtInstitucionRemitente" runat="server"  CssClass="form-control" Width="100%"></asp:TextBox>
                            </div>
                        </div>
                        <!--Categoria Monumento nacional-->
                        <div class="row">
                            <div class="col-sm-4 text-right">
                                <label class="gdoc-formlabel">
                                    Categoría de monumento nacional
                                </label>
                            </div>
                            <div class="col-sm-6">
                                <asp:TextBox ID="tbCategoriaMonumento" runat="server" Width="100%"></asp:TextBox>
                            </div>
                        </div>
                        <!--Codigo de monumento-->
                        <div class="row">
                            <div class="col-sm-4 text-right">
                                <label class="gdoc-formlabel">
                                    Código de monumento
                                </label>
                            </div>
                            <div class="col-sm-6">
                                <asp:TextBox ID="txtCodigoMonumento" runat="server" CssClass="form-control" Width="100%"></asp:TextBox>
                            </div>
                        </div>

                        <!--Denominacion oficial, otras denominaciones, nombre o uso actual-->
                        <div class="row">
                            <div class="col-sm-4 text-right">
                                <label class="gdoc-formlabel">
                                    Denominacion oficial, otras denominaciones, nombre o uso actual
                                </label>
                            </div>
                            <div class="col-sm-6">
                                <asp:TextBox ID="txtDenomiOfi" runat="server" CssClass="form-control" Width="100%"></asp:TextBox>
                            </div>
                        </div>

                        <!--Direccion de MN-->
                        <div class="row">
                            <div class="col-sm-4 text-right">
                                <label class="gdoc-formlabel">
                                    Dirección Monumento Nacional
                                </label>
                            </div>
                            <div class="col-sm-6">
                                <asp:TextBox ID="txtDireccionTxt" runat="server" CssClass="form-control" Width="100%"></asp:TextBox>
                            </div>
                        </div>
                        <!--Region, provincia y comuna-->
                        <div class="row">
                            <div class="col-sm-4 text-right">
                                <label class="gdoc-formlabel">
                                    Región, provincia y comuna
                                </label>
                            </div>
                            <div class="col-sm-6">
                                <asp:TextBox ID="tbRegionProvincia" runat="server" Width="100%"></asp:TextBox>
                            </div>
                        </div>
                        <!--Materia, nombre de proyecto o programa, proyecto o actividad-->
                        <div class="row">
                            <div class="col-sm-4 text-right">
                                <label class="gdoc-formlabel">
                                    Materia, nombre de proyecto o programa , proyecto o actividad
                                </label>
                            </div>
                            <div class="col-sm-6">
                                <asp:TextBox ID="txtMateria" runat="server" CssClass="form-control" Width="100%"></asp:TextBox>
                            </div>
                        </div>
                        <!--Etiqueta-->
                        <div class="row">
                            <div class="col-sm-4 text-right">
                                <label class="gdoc-formlabel">
                                    Etiqueta
                                </label>
                            </div>
                            <div class="col-sm-6">
                                <asp:TextBox ID="tbEtiqueta" runat="server" Width="100%"></asp:TextBox>
                            </div>
                        </div>
                        <!--UT-->
                        <div class="row">
                            <div class="col-sm-4 text-right" style="white-space: nowrap">
                                <label class="gdoc-formlabel" id="lbUtAsignada" runat="server">
                                    Unidad Técnica Asignada
                                </label>

                            </div>
                            <div class="col-sm-6">
                                <asp:TextBox ID="tbUnidadTecnica" runat="server" Width="100%"></asp:TextBox>
                            </div>
                        </div>
                        <!--Profesional UT-->
                        <div class="row">
                            <div class="col-sm-4 text-right" style="white-space: nowrap">
                                <label class="gdoc-formlabel">
                                    Profesional UT
                                </label>
                            </div>
                            <div class="col-sm-6">
                                <asp:TextBox ID="tbProfesionalArea" runat="server" Width="100%"></asp:TextBox>
                            </div>
                        </div>


                        <!--Botonera de Reporte-->

                        <div class="row">
                            <div class="offset-md-4 col-md-2">
                                <asp:Button ID="btnVerReporte" runat="server" CssClass="btn btn-primary animate" OnClick="btnVerReporte_Click" Text="Ver Reporte" OnClientClick="return verReporteClick();" />
                            </div>
                            <div class="offset-md-1 col-sm-1">
                                <asp:Button ID="btnVolver" runat="server" CssClass="btn btn-success animate" Text="Volver" OnClientClick="return btnVolverClick();" UseSubmitBehavior="False" />                              
                            </div>
                            <div class="offset-md-1 col-md-3">
                                <asp:Button ID="btnLimpiarFiltros" runat="server" CssClass="btn btn-success animate" Text="Limpiar Filtros" OnClientClick="limpiarFiltros(event); return false;" />
                            </div>
                        </div>

                    </div>
                </div>


            </div>
        </asp:Panel>


        <rsweb:ReportViewer ID="rpReporte" runat="server" Height="900px"
            Style="width: 100% !important; display: table !important; margin: 0px; overflow: auto !important;" AsyncRendering="true"
            KeepSessionAlive="true">
        </rsweb:ReportViewer>
    


    </form>


    <script type="text/javascript">

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
            var Siteurl = GetUrl();

            $("#txtFechaDesde").kendoDatePicker({
                culture: "es-CL",
                format: "dd/MM/yyyy"
            });

            $("#txtFechaHasta").kendoDatePicker({
                culture: "es-CL",
                format: "dd/MM/yyyy"
            });

            /**
             * Se asigna "Nombre" a dataValueField para q se envíe al servicio Wss.Gedoc (servicio q devuelve datos de los reportes)
             * el titulo de los filtros seleccionados en vez del id de los filtros 
             */
            $("#tbTipoTramite").kendoComboBox({
                placeholder: "Seleccione Tipo de Trámite...",
                messages: getMensajeSelect(),
                dataTextField: "Nombre",
                dataValueField: "Nombre",
                dataSource: getDataSourceGenerico("", <%= (int) Gedoc.WebReport.Enums.ListasGenericas.TipoTramite %>),
                <%--dataSource: {
                    transport: {
                        dataType: "json",
                        read: Siteurl + "/Api/Generico/<%= (int) Gedoc.WebReport.Enums.ListasGenericas.TipoTramite %>"
                    }
                },--%>
                filter: "contains"
            });
            $('#<%= tbTipoTramite.ClientID %>').data("kendoComboBox").value($('#<%= hfTipoTramite.ClientID %>').val());

            $("#tbCanalLlega").kendoComboBox({
                placeholder: "Seleccione Canal de llegada de Trámite...",
                messages: getMensajeSelect(),
                dataTextField: "Nombre",
                dataValueField: "Nombre",
                dataSource: getDataSourceGenerico("", <%= (int) Gedoc.WebReport.Enums.ListasGenericas.CanalLlegadaTramite %>, 10),
                <%--dataSource: {
                    transport: {
                        dataType: "json",
                        read: Siteurl +
                        "/Api/Generico/<%= (int) Gedoc.WebReport.Enums.ListasGenericas.CanalLlegadaTramite %>"
                    },
                    pageSize: 10
                },--%>
                filter: "contains"
            });
            $('#<%= tbCanalLlega.ClientID %>').data("kendoComboBox").value($('#<%= hfCanalTramite.ClientID %>').val());

            $("#tbNcaso").kendoComboBox({
                placeholder: "Seleccione Número de Caso...",
                messages: getMensajeSelect(),
                dataTextField: "Id",
                dataValueField: "Id",
                dataSource: getDataSourceGenerico("", <%= (int) Gedoc.WebReport.Enums.ListasGenericas.Caso %>),
                <%--dataSource: {
                    transport: {
                        dataType: "json",
                        read: Siteurl + "/Api/Generico/<%= (int) Gedoc.WebReport.Enums.ListasGenericas.Caso %>"
                    }
                },--%>
                filter: "contains"
            });
            $('#<%= tbNcaso.ClientID %>').data("kendoComboBox").value($('#<%= hfNumeroCaso.ClientID %>').val());

            $("#tbNombreCaso").kendoComboBox({
                placeholder: "Seleccione Nombre de Caso...",
                messages: getMensajeSelect(),
                dataTextField: "Nombre",
                dataValueField: "Id",
                dataSource: getDataSourceGenerico("", <%= (int) Gedoc.WebReport.Enums.ListasGenericas.Caso %>),
                <%--dataSource: {
                    transport: {
                        dataType: "json",
                        read: Siteurl + "/Api/Generico/<%= (int) Gedoc.WebReport.Enums.ListasGenericas.Caso %>"
                    }
                },--%>
                filter: "contains"
            });
            $('#<%= tbNombreCaso.ClientID %>').data("kendoComboBox").value($('#<%= hfNombreCaso.ClientID %>').val());

            $("#tbDocingreso").kendoMultiSelect({
                placeholder: "Seleccione Documento de Ingreso...",
                messages: getMensajeMultiselect(),
                dataTextField: "Nombre",
                dataValueField: "Nombre",
                dataSource: getDataSourceGenerico("", <%= (int) Gedoc.WebReport.Enums.ListasGenericas.DocIngreso %>, 10),
                <%--dataSource: {
                    transport: {
                        dataType: "json",
                        read: Siteurl + "/Api/Generico/<%= (int) Gedoc.WebReport.Enums.ListasGenericas.DocIngreso %>"
                    },
                    pageSize: 10
                },--%>
                filter: 'contains',
                value: $('#<%= hfDocumentoIngreso.ClientID %>').val()
                    ? $('#<%= hfDocumentoIngreso.ClientID %>').val().split(",")
                    : []
            });

            $("#tbEstadoyEtapa").kendoMultiSelect({
                placeholder: "Seleccione Estado y Etapa...",
                messages: getMensajeMultiselect(),
                dataTextField: "Nombre",
                dataValueField: "Id",
                dataSource: getDataSourceGenerico("", <%= (int) Gedoc.WebReport.Enums.ListasGenericas.EstadoEtapa %>),
                <%--dataSource: {
                    transport: {
                        dataType: "json",
                        read: Siteurl + "/Api/Generico/<%= (int) Gedoc.WebReport.Enums.ListasGenericas.EstadoEtapa %>"
                    }
                },--%>
                filter: 'contains',
                value: $('#<%= hfEstadoEtapa.ClientID %>').val() ? $('#<%= hfEstadoEtapa.ClientID %>').val().split(",") : []
            });

            $("#tbRemitente").kendoComboBox({
                placeholder: "Seleccione Remitente...",
                messages: getMensajeSelect(),
                dataTextField: "Nombre",
                dataValueField: "Nombre",
                dataSource: getDataSourceGenerico("", <%= (int) Gedoc.WebReport.Enums.ListasGenericas.Remitente %>, 10),
                <%--dataSource: {
                    transport: {
                        dataType: "json",
                        read: Siteurl + "/Api/Generico/<%= (int) Gedoc.WebReport.Enums.ListasGenericas.Remitente %>"
                    },
                    pageSize: 10
                },--%>
                filter: "contains"
            });
            $('#<%= tbRemitente.ClientID %>').data("kendoComboBox").value($('#<%= hfRemitente.ClientID %>').val());

            $("#tbCategoriaMonumento").kendoMultiSelect({
                placeholder: "Seleccione Categoría de Monumento Nacional...",
                messages: getMensajeMultiselect(),
                dataTextField: "Nombre",
                dataValueField: "Nombre",
                dataSource: getDataSourceGenerico("", <%= (int) Gedoc.WebReport.Enums.ListasGenericas.TipoMonumento %>),
                <%--dataSource: {
                    transport: {
                        dataType: "json",
                        read: Siteurl + "/Api/Generico/<%= (int) Gedoc.WebReport.Enums.ListasGenericas.TipoMonumento %>"
                    }
                },--%>
                filter: 'contains',
                value: $('#<%= hfTipoMon.ClientID %>').val() ? $('#<%= hfTipoMon.ClientID %>').val().split(",") : []
            });

            $("#tbRegionProvincia").kendoMultiSelect({
                placeholder: "Seleccione Comuna-Provincia-Region...",
                messages: getMensajeMultiselect(),
                dataTextField: "Nombre",
                dataValueField: "Id",
                dataSource: getDataSourceGenerico("", "<%= (int) Gedoc.WebReport.Enums.ListasGenericas.ComunaRegion %>?todos=0"),
               <%-- dataSource: {
                    transport: {
                        dataType: "json",
                        read: Siteurl + "/Api/Generico/<%= (int) Gedoc.WebReport.Enums.ListasGenericas.ComunaRegion %>?todos=0"
                    }
                },--%>
                filter: 'contains',
                value: $('#<%= hfComuna.ClientID %>').val() ? $('#<%= hfComuna.ClientID %>').val().split(",") : []
            });

            $("#tbUnidadTecnica").kendoMultiSelect({
                placeholder: "Seleccione Unidad Técnica...",
                messages: getMensajeMultiselect(),
                dataTextField: "Nombre",
                dataValueField: "Nombre",
                dataSource: getDataSourceGenerico("", <%= (int) Gedoc.WebReport.Enums.ListasGenericas.UnidadTecnia %>),
                <%--dataSource: {
                    transport: {
                        dataType: "json",
                        read: {
                            url: Siteurl + "/Api/Generico/<%= (int) Gedoc.WebReport.Enums.ListasGenericas.UnidadTecnia %>",
                            contentType: "application/json"
                        }
                    }
                },--%>
                filter: 'contains',
                value: $('#<%= hfUnidadTecnica.ClientID %>').val() ? $('#<%= hfUnidadTecnica.ClientID %>').val().split(",") : [],
                change: function () {
                    var profK = $("#tbProfesionalArea").data("kendoMultiSelect");
                    if (profK && profK.dataSource) {
                        profK.dataSource.read();
                    }
                }
            });

            $("#tbProfesionalArea").kendoMultiSelect({
                placeholder: "Seleccione Profesional...",
                messages: getMensajeMultiselect(),
                dataTextField: "Nombre",
                dataValueField: "Nombre",
                enable: false,
                dataSource: getDataSourceGenerico("", "<%= (int) Gedoc.WebReport.Enums.ListasGenericas.Profesionales %>", 10, true,
                    profUtParams
                ),
               <%-- dataSource: {
                    serverFiltering: true,
                    transport: {
                        dataType: "json",
                        read: {
                            url: Siteurl + "/Api/Generico/3",
                            traditional: true,
                            // type: "POST",
                            data: { id: 3, extraParam: ["aaaa", "bbbb"] } //$.param({ id: 3, extraParam: ["aaaa", "bbbb"] }, true)
                            data: function (e) {
                                var utK = $("#tbUnidadTecnica").data("kendoMultiSelect");
                                var value = utK && utK.value() ? utK.value() : [];
                                var param =  {
                                    id: <%= (int) Gedoc.WebReport.Enums.ListasGenericas.Profesionales %>,
                                    extraParam: value // JSON.stringify(value) // utK ? JSON.stringify(value) : ""
                                }
                                return $.param(param);
                            } 
                        }
                    },
                },--%>
                filter: 'contains',
                value: $('#<%= hfProfUt.ClientID %>').val() ? $('#<%= hfProfUt.ClientID %>').val().split(",") : []
            });

            $("#tbEtiqueta").kendoMultiSelect({
                placeholder: "Seleccione Etiqueta...",
                messages: getMensajeMultiselect(),
                dataTextField: "Nombre",
                dataValueField: "Nombre",
                dataSource: getDataSourceGenerico("", <%= (int) Gedoc.WebReport.Enums.ListasGenericas.Etiqueta %>),
               <%-- dataSource: {
                    transport: {
                        dataType: "json",
                        contentType: "application/json",
                        read: {
                            url: Siteurl + "/Api/Generico/<%= (int) Gedoc.WebReport.Enums.ListasGenericas.Etiqueta %>",
                            contentType: "application/json"

                        }
                    }
                },--%>
                filter: 'contains',
                value: $('#<%= hfEtiqueta.ClientID %>').val() ? $('#<%= hfEtiqueta.ClientID %>').val().split(",") : []

            });


        });

        function mostrarReporteTabla() {
        <%--        var btnID = '<%=btnVerReporte.UniqueID %>';
        __doPostBack(btnID, '');--%>
        }
        
        function verReporteClick(e) {

            // e.preventDefault();

            if (!validaRangoFechas()) {
                return false;
            }

            $.LoadingOverlay("show", {});

            //var _fechadesde = !$("#txtFechaDesde").length ? "" : $("#txtFechaDesde").data("kendoDatePicker").value();
            //var _fechahasta = !$("#txtFechaHasta").length ? "" : $("#txtFechaHasta").data("kendoDatePicker").value();
            var valor = $("#tbTipoTramite").data("kendoComboBox").value();
            $('#<%=hfTipoTramite.ClientID%>').val(valor);
            valor = $("#tbCanalLlega").data("kendoComboBox").value();
            $('#<%=hfCanalTramite.ClientID%>').val(valor);
            valor = $("#tbNcaso").data("kendoComboBox").value();
            $('#<%=hfNumeroCaso.ClientID%>').val(valor);
            valor = $("#tbNombreCaso").data("kendoComboBox").value();
            $('#<%=hfNombreCaso.ClientID%>').val(valor);
            valor = $("#tbDocingreso").data("kendoMultiSelect").value();
            $('#<%=hfDocumentoIngreso.ClientID%>').val(valor.join());
            valor = $("#tbEstadoyEtapa").data("kendoMultiSelect").value();
            $('#<%=hfEstadoEtapa.ClientID%>').val(valor.join());
            valor = $("#tbRemitente").data("kendoComboBox").value();
            $('#<%=hfRemitente.ClientID%>').val(valor);
            valor = $("#tbCategoriaMonumento").data("kendoMultiSelect").value();
            $('#<%=hfTipoMon.ClientID%>').val(valor.join());
            valor = $("#tbRegionProvincia").data("kendoMultiSelect").value();
            $('#<%=hfComuna.ClientID%>').val(valor.join());
            valor = $("#tbUnidadTecnica").data("kendoMultiSelect").value();
            $('#<%=hfUnidadTecnica.ClientID%>').val(valor.join());
            valor = $("#tbProfesionalArea").data("kendoMultiSelect").value();
            $('#<%=hfProfUt.ClientID%>').val(valor.join());
            valor = $("#tbEtiqueta").data("kendoMultiSelect").value();
            $('#<%=hfEtiqueta.ClientID%>').val(valor.join());

            //var pos = $(this).offset().top;

            //$('#SeccionReporte').animate({
            //    top: pos + $(this).height()
            //},
            //    1000);

        }

        function convertDate(fecha) {
            var match = /(\d+)\/(\d+)\/(\d+)/.exec(fecha)
            return new Date(match[3], match[2], match[1]);
        }

        function limpiarFiltros(e) {
            e.preventDefault();

            // Fecha Desde
            $("#txtFechaDesde").data("kendoDatePicker").value(null);
            $("#txtFechaDesde").data("kendoDatePicker").trigger("change");
            // Fecha Hasta
            $("#txtFechaHasta").data("kendoDatePicker").value(null);
            $("#txtFechaHasta").data("kendoDatePicker").trigger("change");
            // Tipo de Trámite
            $('#<%= tbTipoTramite.ClientID %>').data("kendoComboBox").value("");
            // Canal de llegada de Trámite
            $('#<%= tbCanalLlega.ClientID %>').data("kendoComboBox").value("");
            // Número de caso
            $('#<%= tbNcaso.ClientID %>').data("kendoComboBox").value("");
            // Nombre de caso
            $('#<%= tbNombreCaso.ClientID %>').data("kendoComboBox").value("");
            // Documento Ingreso
            $('#<%= tbDocingreso.ClientID %>').data("kendoMultiSelect").value([]);
            // Estado
            $('#<%= tbEstadoyEtapa.ClientID %>').data("kendoMultiSelect").value([]);
            // Remitente
            $('#<%= tbRemitente.ClientID %>').data("kendoComboBox").value("");
            // Categoría MN
            $('#<%= tbCategoriaMonumento.ClientID %>').data("kendoMultiSelect").value([]);
            // Comuna-provincia-region
            $('#<%= tbRegionProvincia.ClientID %>').data("kendoMultiSelect").value([]);
            // Etiqueta
            $('#<%= tbEtiqueta.ClientID %>').data("kendoMultiSelect").value([]);
            // Unidad Técnica
            $('#<%= tbUnidadTecnica.ClientID %>').data("kendoMultiSelect").value([]);
            // Profesional en área
            $('#<%= tbProfesionalArea.ClientID %>').data("kendoMultiSelect").value([]);

            // Institucion remitente
            $('#<%= txtInstitucionRemitente.ClientID %>').val('');
            // Codigo MN
            $('#<%= txtCodigoMonumento.ClientID %>').val('');
            // Denominacion oficial
            $('#<%= txtDenomiOfi.ClientID %>').val('');
            // Dirección MN
            $('#<%= txtDireccionTxt.ClientID %>').val('');
            // Materia
            $('#<%= txtMateria.ClientID %>').val('');


            return false;
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
                kendo.alert("Por favor, especifique una Fecha Desde válida.");
                return false;
            }

            if (fechaH && !moment(fechaH, "DD/MM/YYYY").isValid()) {
                kendo.alert("Por favor, especifique una Fecha Hasta válida.");
                return false;
            }

            if (fechaD && fechaH && moment(fechaH, "DD/MM/YYYY").diff(moment(fechaD, "DD/MM/YYYY"), "day") < 0) {
                kendo.alert("La Fecha Desde tiene que ser anterior a la Fecha Hasta.");
                return false;
            }

            return true;
        }

        function btnVolverClick() {
            parent.postMessage("volverReporte", "*");
            return false;
        }

        function getDataSourceGenerico(url, id, pageSize, serverFiltering, data) {
            var Siteurl = GetUrl();
            url = (url || (Siteurl + "/Api/Generico/")) + id;
            return {
                serverFiltering: serverFiltering,
                transport: {
                    dataType: "json",
                    pageSize: pageSize,
                    read: {
                        url: url,
                        //contentType: "application/json",
                        traditional: true,
                        //type: "POST",
                        data: data
                    }
                }
            }
        }

        function profUtParams() {
            var utK = $("#tbUnidadTecnica").data("kendoMultiSelect");
            var value = (utK && utK.value() && utK.value().length) ? utK.value() : [];
            var param = {
                extraParam: value,
                idPadre: -1,
                filtro: ""
            }
            var profK = $("#tbProfesionalArea").data("kendoMultiSelect");
            if (profK) {
                profK.enable(value.length);
                if (!value.length) {
                    profK.value("");
                } else {
                    param.filtro = profK.input.val() == profK.options.placeholder ? "" : profK.input.val();
                }
            }
            return param;
        } 

    </script>

</body>
</html>
