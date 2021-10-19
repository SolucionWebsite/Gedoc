<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="VistaReporteProcMasivos.aspx.cs" Inherits="Gedoc.WebReport.VistaReporteProcMasivos" %>

<%@ Register Assembly="Microsoft.ReportViewer.WebForms, Version=11.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91"
    Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    

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

        .ms-formvalidation {
            color: red;
        }
    </style>

</head>
<body>
    <form id="form1" runat="server">

        <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>

        <asp:HiddenField ID="hfUnidadTecnica" runat="server" />
        <asp:HiddenField ID="hfProfUt" runat="server" />
        <asp:HiddenField ID="hfEstado" runat="server" />
        <asp:HiddenField ID="hfEtapa" runat="server" />
        <asp:HiddenField ID="hfEtiqueta" runat="server" />
        <asp:HiddenField ID="hfTipoMon" runat="server" />
        <asp:HiddenField ID="hfRegion" runat="server" />

        <h4>
            <span>Reporte Procesos Masivos</span>
        </h4>

        <asp:Panel ID="onFiltrosTram" runat="server" GroupingText="Filtros" Font-Bold="False" CssClass="paneles">
            <div class="container-fluid" style="width: auto !important;">


                <div class="row">
                    <div class="col-md-9">

                        <!--Fecha desde y hasta-->

                        <div class="row">
                            <div class="offset-md-4 col-sm-3">
                                <label class="gdoc-formlabel">
                                    Fecha Desde
                                </label>
                                <div class="div-inline">
                                    <input id="txtFechaDesde" runat="server" style="width: 100%" />
                                </div>
                            </div>
                            <div class="col-sm-3">
                                <label class="gdoc-formlabel">
                                    Fecha Hasta
                                </label>
                                <div class="div-inline">
                                    <input id="txtFechaHasta" runat="server" style="width: 100%" />
                                </div>
                            </div>
                        </div>

                        <!--UT-->
                        <div class="row">
                            <div class="col-sm-4 text-right" style="white-space: nowrap">
                                <label class="gdoc-formlabel" id="lbUtAsignada" runat="server">
                                    Unidad Técnica Asignada<span class="ms-formvalidation" title="Este campo es obligatorio."> *</span>
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

                        <!--Estado -->
                        <div class="row">
                            <div class="col-sm-4 text-right">
                                <label class="gdoc-formlabel">
                                    Estado<span class="ms-formvalidation" title="Este campo es obligatorio."> *</span>
                                </label>
                            </div>
                            <div class="col-sm-6">
                                <asp:TextBox ID="tbEstado" runat="server" Width="100%"></asp:TextBox>
                            </div>
                        </div>

                        <!--Etapa-->
                        <div class="row">
                            <div class="col-sm-4 text-right">
                                <label class="gdoc-formlabel">
                                    Etapa
                                </label>
                            </div>
                            <div class="col-sm-6">
                                <asp:TextBox ID="tbEtapa" runat="server" Width="100%"></asp:TextBox>
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

                        <!--Categoria Monumento nacional-->
                        <div class="row">
                            <div class="col-sm-4 text-right">
                                <label class="gdoc-formlabel">
                                    Categoría de Monumento Nacional
                                </label>
                            </div>
                            <div class="col-sm-6">
                                <asp:TextBox ID="tbCategoriaMonumento" runat="server" Width="100%"></asp:TextBox>
                            </div>
                        </div>

                        <!--Region, provincia y comuna-->
                        <div class="row">
                            <div class="col-sm-4 text-right">
                                <label class="gdoc-formlabel">
                                    Región
                                </label>
                            </div>
                            <div class="col-sm-6">
                                <asp:TextBox ID="tbRegion" runat="server" Width="100%"></asp:TextBox>
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-sm-4 text-right">
                                <label class="gdoc-formlabel">
                                    Acción Proceso Masivo<span class="ms-formvalidation" title="Este campo es obligatorio."> *</span>
                                </label>
                            </div>
                            <div class="col-sm-6">
                                <asp:DropDownList ID="ddlAccion" runat="server" CssClass="form-control"></asp:DropDownList>
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
            $("#tbUnidadTecnica").kendoComboBox({
                placeholder: "Seleccione Unidad Técnica...",
                messages: getMensajeSelect(),
                dataTextField: "Nombre",
                dataValueField: "Nombre",
                dataSource: {
                    transport: {
                        dataType: "json",
                        read: Siteurl + "/Api/Generico/<%= (int) Gedoc.WebReport.Enums.ListasGenericas.UnidadTecnia %>?todos=0"
                    }
                },
                filter: "contains"
            });
            if (!$('#hfUnidadTecnica').val())
                $("#tbUnidadTecnica").data("kendoComboBox").value("");

            $("#tbProfesionalArea").kendoComboBox({
                placeholder: "Seleccione Profesional...",
                messages: getMensajeSelect(),
                dataTextField: "Nombre",
                dataValueField: "Nombre",
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
                                    extraParam: [utK ? utK.value() : 0],
                                    idPadre: -1
                                }
                            }
                        }
                    }
                },
               <%-- dataSource: {
                    transport: {
                        dataType: "json",
                        read: Siteurl + "/Api/Generico/<%= (int) Gedoc.WebReport.Enums.ListasGenericas.Profesionales %>?todos=0"
                    }
                },--%>
                filter: "contains"
            });
            if (!$('#hfProfUt').val())
                $("#tbProfesionalArea").data("kendoComboBox").value("");

            $("#tbEstado").kendoComboBox({
                placeholder: "Seleccione Estado...",
                messages: getMensajeSelect(),
                dataTextField: "Nombre",
                dataValueField: "Nombre",
                dataSource: {
                    transport: {
                        dataType: "json",
                        read: Siteurl + "/Api/Generico/<%= (int) Gedoc.WebReport.Enums.ListasGenericas.Estados %>?todos=0"
                    }
                },
                filter: "contains"
            });
            if (!$('#hfEstado').val())
                $("#tbEstado").data("kendoComboBox").value("");

            $("#tbEtapa").kendoComboBox({
                placeholder: "Seleccione Etapa...",
                messages: getMensajeSelect(),
                dataTextField: "Nombre",
                dataValueField: "Nombre",
                dataSource: {
                    transport: {
                        dataType: "json",
                        read: Siteurl + "/Api/Generico/<%= (int) Gedoc.WebReport.Enums.ListasGenericas.Etapas %>?todos=0"
                    }
                },
                filter: "contains"
            });
            if (!$('#hfEtapa').val())
                $("#tbEtapa").data("kendoComboBox").value("");

            $("#tbEtiqueta").kendoComboBox({
                placeholder: "Seleccione Etiqueta...",
                messages: getMensajeSelect(),
                dataTextField: "Nombre",
                dataValueField: "Nombre",
                dataSource: {
                    transport: {
                        dataType: "json",
                        read: Siteurl + "/Api/Generico/<%= (int) Gedoc.WebReport.Enums.ListasGenericas.Etiqueta %>?todos=0"
                    }
                },
                filter: "contains"
            });
            if (!$('#hfEtiqueta').val())
                $("#tbEtiqueta").data("kendoComboBox").value("");

            $("#tbCategoriaMonumento").kendoComboBox({
                placeholder: "Seleccione Categoría de Monumento Nacional...",
                messages: getMensajeSelect(),
                dataTextField: "Nombre",
                dataValueField: "Nombre",
                dataSource: {
                    transport: {
                        dataType: "json",
                        read: Siteurl + "/Api/Generico/<%= (int) Gedoc.WebReport.Enums.ListasGenericas.TipoMonumento %>?todos=0"
                    }
                },
                filter: "contains"
            });
            if (!$('#hfTipoMon').val())
                $("#tbCategoriaMonumento").data("kendoComboBox").value("");

            $("#tbRegion").kendoComboBox({
                placeholder: "Seleccione Región...",
                messages: getMensajeSelect(),
                dataTextField: "Nombre",
                dataValueField: "Nombre",
                dataSource: {
                    transport: {
                        dataType: "json",
                        read: Siteurl + "/Api/Generico/<%= (int) Gedoc.WebReport.Enums.ListasGenericas.Region %>?todos=0"
                    }
                },
                filter: "contains"
            });
            if (!$('#hfRegion').val())
                $("#tbRegion").data("kendoComboBox").value("");


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

            var valor = $("#tbUnidadTecnica").data("kendoComboBox").text();
            $('#<%=hfUnidadTecnica.ClientID%>').val(valor);
            valor = $("#tbProfesionalArea").data("kendoComboBox").text();
            $('#<%=hfProfUt.ClientID%>').val(valor);
            valor = $("#tbEstado").data("kendoComboBox").text();
            $('#<%=hfEstado.ClientID%>').val(valor);
            valor = $("#tbEtapa").data("kendoComboBox").text();
            $('#<%=hfEtapa.ClientID%>').val(valor);
            valor = $("#tbEtiqueta").data("kendoComboBox").text();
            $('#<%=hfEtiqueta.ClientID%>').val(valor);
            valor = $("#tbCategoriaMonumento").data("kendoComboBox").text();
            $('#<%=hfTipoMon.ClientID%>').val(valor);
            valor = $("#tbRegion").data("kendoComboBox").text();
            $('#<%=hfRegion.ClientID%>').val(valor);

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
            // Unidad Técnica
            $('#<%= tbUnidadTecnica.ClientID %>').data("kendoComboBox").value("");
            // Profesional en área
            $('#<%= tbProfesionalArea.ClientID %>').data("kendoComboBox").value("");
            // Estado
            $('#<%= tbEstado.ClientID %>').data("kendoComboBox").value("");
            // Etapa
            $('#<%= tbEtapa.ClientID %>').data("kendoComboBox").value("");
            // Etiqueta
            $('#<%= tbEtiqueta.ClientID %>').data("kendoComboBox").value("");
            // Categoría MN
            $('#<%= tbCategoriaMonumento.ClientID %>').data("kendoComboBox").value("");
            // Región
            $('#<%= tbRegion.ClientID %>').data("kendoComboBox").value("");

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

    </script>

</body>
</html>
