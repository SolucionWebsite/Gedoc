<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="VerAuditoria.aspx.cs" Inherits="Gedoc.WebReport.VerAuditoria" %>

<%@ Register Assembly="Microsoft.ReportViewer.WebForms, Version=11.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91"
    Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
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

    <%--<script src="Scripts/main.js"></script>--%>

   <%-- <link href="libs/fontawesome5-pro/css/all.min.css" rel="stylesheet" />--%>
    <link href="Content/kendo/2020.1.114/kendo.bootstrap-v4.min.css" rel="stylesheet" type="text/css" />
    <link href="Content/bootstrap.min.css" rel="stylesheet" type="text/css" />
    <link href="Content/Site.css" rel="stylesheet" type="text/css" />

    <script src="Scripts/kendo.modernizr.custom.js"></script>

    <script src="Scripts/kendo/2020.1.114/cultures/kendo.culture.es-CL.min.js"></script>
    <script src="Scripts/kendo/2020.1.114/messages/kendo.messages.es-CL.min.js"></script>

    <script src="libs/moment/moment-with-locales.min.js"></script>

    <style>
        /*.div-inline {
            display: inline-block;
        }*/

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

        <%--campos search de multiselecion--%>
        <asp:HiddenField ID="hfUnidadTecnica" runat="server" />
        <asp:HiddenField ID="hfUnidadTecnicaCopia" runat="server" />
        <asp:HiddenField ID="hfProfUt" runat="server" />
        <asp:HiddenField ID="hfEstado" runat="server" />
        <asp:HiddenField ID="hfEtapa" runat="server" />
        <asp:HiddenField ID="hfEtiqueta" runat="server" />
        <asp:HiddenField ID="hfRemitente" runat="server" />
        <asp:HiddenField ID="hfDestinatario" runat="server" />
        <asp:HiddenField ID="hfSesion" runat="server" />
        <asp:HiddenField ID="hfAlertaMensaje" runat="server" />
        <asp:HiddenField ID="hfReporte" runat="server" />
        <asp:HiddenField ID="hfPrioridad" runat="server" />
        <asp:HiddenField ID="hfRegion" runat="server" />
        <asp:HiddenField ID="hfComuna" runat="server" />
        <asp:HiddenField ID="hfMotivoCierre" runat="server" />
        <asp:HiddenField ID="hfTipoInstRem" runat="server" />
        <asp:HiddenField ID="hfTipoInstDes" runat="server" />
        <asp:HiddenField ID="hfSemaforo" runat="server" />
        <asp:HiddenField ID="hfPlazo" runat="server" />
        <asp:HiddenField ID="hfRelacionDesdeHasta" runat="server" />


        <h4><span>Reporte Auditoría</span></h4>
        <asp:Panel ID="pnDocumento" runat="server" GroupingText="Filtros" Font-Bold="False" CssClass="paneles">
            <div class="container-fluid" style="width: auto !important;">


            <div class="row">
                <div class="offset-md-3 col-sm-2">
                    <label class="gdoc-formlabel">
                        Fecha Desde
                    </label>
                    <div class="div-inline">
                        <input id="txtFechaDesde" runat="server" style="width: 100%" />
                    </div>
                </div>
                <div class="col-sm-2">
                    <label class="gdoc-formlabel">
                        Fecha Hasta
                    </label>
                    <div class="div-inline">
                        <input id="txtFechaHasta" runat="server" style="width: 100%" />
                    </div>
                </div>
            </div>

                <div class="row">
                    <div class="offset-md-3 col-sm-9" id="rowInfoFechas" style="display: none;">
                        <span id="lbFechas" class="label label-info" style="font-size: 100%; display: inline-block; width: 100%;">Atención. Se recomienda rangos de fechas menores a 6 meses.</span>
                    </div>
                </div>

                <div class="row">
                    <div class="col-sm-3 text-right">
                        <label class="gdoc-formlabel">
                            Nombre de proyecto o programa
                        </label>
                    </div>
                    <div class="col-md-5">
                        <asp:TextBox ID="tbNombreProyecto" runat="server" CssClass="form-control"></asp:TextBox>
                    </div>
                </div>

                <div class="row">
                    <div class="col-sm-3 text-right">
                        <label class="gdoc-formlabel">
                            Institución Remitente
                        </label>
                    </div>
                    <div class="col-md-5">
                        <asp:TextBox ID="tbInstitucionRem" runat="server" CssClass="form-control"></asp:TextBox>
                    </div>
                </div>

                <div class="row">
                    <div class="col-sm-3 text-right">
                        <label class="gdoc-formlabel">
                            Etiqueta
                        </label>
                    </div>
                    <div class="col-md-5">
                        <asp:TextBox ID="tbEtiqueta" runat="server" CssClass="search-etiqueta" Width="100%"></asp:TextBox>
                    </div>
                </div>

                <div class="row">
                    <div class="col-sm-3 text-right">
                        <label class="gdoc-formlabel">
                            Región
                        </label>
                    </div>
                    <div class="col-md-5">
                        <asp:TextBox ID="tbRegion" runat="server" CssClass="search-region" Width="100%"></asp:TextBox>
                    </div>
                </div>

                <div class="row">
                    <div class="col-sm-3 text-right">
                        <label class="gdoc-formlabel">
                            Comuna
                        </label>
                    </div>
                    <div class="col-md-5">
                        <asp:TextBox ID="tbComuna" runat="server" CssClass="search-comuna" Width="100%"></asp:TextBox>
                    </div>
                </div>

                <div class="row">
                    <div class="col-sm-3 text-right">
                        <label class="gdoc-formlabel">
                            Denominación Oficial
                        </label>
                    </div>
                    <div class="col-md-5">
                        <asp:TextBox ID="tbMonNacional" runat="server" CssClass="form-control" Width="100%"></asp:TextBox>
                    </div>
                </div>

                <div class="row">
                    <div class="col-sm-3 text-right" style="white-space: nowrap">
                        <label class="gdoc-formlabel" id="lbUtAsignada" runat="server">
                            Unidad Técnica Asignada
                        </label>
                    </div>
                    <div class="col-md-5">
                        <asp:TextBox ID="tbUnidadAsign" runat="server" CssClass="search-unidadasign" Width="100%"></asp:TextBox>
                    </div>
                </div>

                <div class="row">
                    <div class="col-sm-3 text-right" style="white-space: nowrap">
                        <label class="gdoc-formlabel">
                            Profesional UT 
                        </label>
                    </div>
                    <div class="col-md-5">
                        <asp:TextBox ID="tbProfesionalUt" runat="server" CssClass="search-profut" Width="100%"></asp:TextBox>
                    </div>
                </div>

                <div class="row">
                    <div class="col-sm-3 text-right">
                        <label class="gdoc-formlabel">
                            Unidad Técnica en copia
                        </label>
                    </div>
                    <div class="col-md-5">
                        <asp:TextBox ID="tbUtCopia" runat="server" CssClass="search-utcopia" Width="100%"></asp:TextBox>
                    </div>
                </div>

                <div class="row">
                    <div class="col-sm-3 text-right">
                        <label class="gdoc-formlabel">
                            Requiere respuesta
                        </label>
                    </div>
                    <div class="col-md-5">
                        <div class="row">
                            <div class="col-sm-2">
                                <asp:CheckBox ID="chbRequiereRespSi" runat="server" Text="Sí" />
                            </div>
                            <div class="col-sm-2">
                                <asp:CheckBox ID="chbRequiereRespNo" runat="server" Text="No" />
                            </div>
                        </div>
                    </div>
                </div>

                <div class="row">
                    <div class="col-sm-3 text-right">
                        <label class="gdoc-formlabel">
                            Motivo de cierre
                        </label>
                    </div>
                    <div class="col-md-5">
                        <asp:TextBox ID="tbMotivoCierre" runat="server" CssClass="search-motivocierre" Width="100%"></asp:TextBox>
                    </div>
                </div>

                <div class="row">
                    <div class="col-sm-3 text-right">
                        <label class="gdoc-formlabel">
                            Requiere timbraje de plano
                        </label>
                    </div>
                    <div class="col-md-5">
                        <div class="row">
                            <div class="col-sm-2">
                                <asp:CheckBox ID="chbTimbrajeSi" runat="server" Text="Sí" />
                            </div>
                            <div class="col-sm-2">
                                <asp:CheckBox ID="chbTimbrajeNo" runat="server" Text="No" />
                            </div>
                        </div>
                    </div>
                </div>

                <div class="row">
                    <div class="col-sm-3 text-right">
                        <label class="gdoc-formlabel">
                            Requiere Acuerdo / Acta
                        </label>
                    </div>
                    <div class="col-md-5">
                        <div class="row">
                            <div class="col-sm-2">
                                <asp:CheckBox ID="chbRequiereAcueSi" runat="server" Text="Sí" />
                            </div>
                            <div class="col-sm-2">
                                <asp:CheckBox ID="chbRequiereAcueNo" runat="server" Text="No" />
                            </div>
                        </div>
                    </div>
                </div>

                <div class="row">
                    <div class="col-sm-3 text-right">
                        <label class="gdoc-formlabel">
                            Estado
                        </label>
                    </div>
                    <div class="col-md-5">
                        <asp:TextBox ID="tbEstado" runat="server" CssClass="search-estado" Width="100%"></asp:TextBox>
                    </div>
                </div>

                <div class="row">
                    <div class="col-sm-3 text-right">
                        <label class="gdoc-formlabel">
                            Despacho
                        </label>
                    </div>
                    <div class="col-md-5">
                        <div class="row">
                            <div class="col-sm-2">
                                <asp:CheckBox ID="chbDespachoSi" runat="server" Text="Sí" />
                            </div>
                            <div class="col-sm-2">
                                <asp:CheckBox ID="chbDespachoNo" runat="server" Text="No" />
                            </div>
                        </div>
                    </div>
                </div>

                <div class="row">
                    <div class="col-sm-3 text-right">
                        <label class="gdoc-formlabel">
                            Institución del Destinatario
                        </label>
                    </div>
                    <div class="col-md-5">
                        <asp:TextBox ID="tbInstituciónDest" runat="server" CssClass="form-control"></asp:TextBox>
                    </div>
                </div>

                <div class="row">
                    <div class="col-sm-3 text-right">
                        <label class="gdoc-formlabel">
                            Tipo de Institución Remitente
                        </label>
                    </div>
                    <div class="col-md-5">
                        <asp:TextBox ID="tbTipoInstRemitente" runat="server" CssClass="search-tipoinstrem" Width="100%"></asp:TextBox>
                    </div>
                </div>

                <div class="row">
                    <div class="col-sm-3 text-right">
                        <label class="gdoc-formlabel">
                            Tipo de Institución Destinatario
                        </label>
                    </div>
                    <div class="col-md-5">
                        <asp:TextBox ID="tbTipoInstDestinatario" runat="server" CssClass="search-tipoinstdes" Width="100%"></asp:TextBox>
                    </div>
                </div>

                <div class="row">
                    <div class="col-sm-3 text-right">
                        <label class="gdoc-formlabel">
                            Semáforo
                        </label>
                    </div>
                    <div class="col-md-5">
                        <asp:TextBox ID="tbSemaforo" runat="server" CssClass="search-semaforo" Width="100%"></asp:TextBox>
                    </div>
                </div>

                <div class="row">
                    <div class="col-sm-3 text-right" style="white-space: nowrap">
                        <label class="gdoc-formlabel">
                            Prioridad de Requerimiento
                        </label>
                    </div>
                    <div class="col-md-5">
                        <asp:TextBox ID="tbPrioridadReq" runat="server" CssClass="search-prioridad" Width="100%"></asp:TextBox>
                    </div>
                </div>

                <div class="row">
                    <div class="col-sm-3 text-right" style="white-space: nowrap">
                        <label class="gdoc-formlabel">
                            Fecha de Resolución Estimada
                        </label>
                    </div>
                    <div class="col-sm-3">
                        <label class="gdoc-formlabel">
                            Desde
                        </label>
                        <div class="div-inline">
                            <%--                    <SharePoint:DateTimeControl ID="dtcFechaResolEstimDesde" runat="server" IsValid="True" ErrorMessage="Por favor, especifique una fecha correcta" DateOnly="True" Enabled="True" />--%>
                            <input type="text" id="dtcFechaResolEstimDesde" runat="server" />
                        </div>
                    </div>
                    <div class="col-sm-3">
                        <label class="gdoc-formlabel">
                            Hasta
                        </label>
                        <div class="div-inline">
                            <%--                    <SharePoint:DateTimeControl ID="dtcFechaResolEstimHasta" runat="server" IsValid="True" ErrorMessage="Por favor, especifique una fecha correcta" DateOnly="True" Enabled="True" />--%>
                            <input type="text" id="dtcFechaResolEstimHasta" runat="server" />

                        </div>
                    </div>
                </div>

                <div class="row" style="display: none;">
                    <div class="col-sm-3 text-right" style="white-space: nowrap">
                        <label class="gdoc-formlabel">
                            Plazo
                        </label>
                    </div>
                    <div class="col-md-5">
                        <asp:TextBox ID="tbPlazo" runat="server" CssClass="search-plazo" Width="100%"></asp:TextBox>
                    </div>
                </div>

                <div class="row">
                </div>

                <div class="row mt-4">
                    <div class="offset-md-3 col-sm-9">
                        <h5>Rangos de Fechas para sumatorias
                        </h5>
                    </div>
                </div>
                <div class="row">
                    <div class="offset-md-3 col-sm-4">
                        <label class="gdoc-formlabel">
                            Desde
                        </label>
                    </div>
                    <div class="col-sm-4">
                        <label class="gdoc-formlabel">
                            Hasta
                        </label>
                    </div>
                </div>
                <div class="row" style="margin-top: 15px; margin-bottom: 20px;">
                    <div class="offset-md-3 col-sm-4">
                        <select class="form-control" id="selFechaDesde" style="width: 100% !important">
                            <% 

                                foreach (string key in Gedoc.WebReport.VerAuditoria.FechasSumatorias.Keys)
                                {
                            %>
                            <option value="<%= key %>"><%= Gedoc.WebReport.VerAuditoria.FechasSumatorias[key] %></option>
                            <% } %>
                        </select>
                    </div>
                    <div class="col-sm-4">
                        <% 
                            foreach (string key in Gedoc.WebReport.VerAuditoria.FechasSumatorias.Keys)
                            {
                        %>
                        <div class="checkbox">
                            <label>
                                <input type="checkbox" name="fechaHastaSum" id="hasta<%= key %>" value="<%= key %>"><%=FechasSumatorias[key] %></label>
                        </div>
                        <% } %>
                    </div>
                </div>


            </div>
        </asp:Panel>



        <div class="row">
            <div class="offset-md-2 col-sm-3">
                <%--        <asp:Button ID="btnVerReporte" runat="server" CssClass="btn btn-primary animate" OnClick="btnVerReporte_Click1" Text="Ver Reporte" OnClientClick="return VerReporteClick();" />--%>

                <asp:Button ID="btnVerAuditoria" runat="server" CssClass="btn btn-primary animate" Text="Ver Reporte" OnClientClick="return VerReporteClick();" OnClick="btnVerAuditoria_Click" />
            </div>
            <div class="offset-md-2 col-sm-3">
                <asp:Button ID="btnVolver" runat="server" CssClass="btn btn-success animate" Text="Volver" OnClientClick="return btnVolverClick();" UseSubmitBehavior="False" />       
            </div>
        </div>

        <div id="SeccionReporte">
            <asp:UpdatePanel runat="server" UpdateMode="Always">
                <ContentTemplate>
                    <asp:Label runat="server" ID="errorMsgReporte" Visible="false"
                        CssClass="label-error"></asp:Label>
                </ContentTemplate>
            </asp:UpdatePanel>

            <rsweb:ReportViewer Visible="False" ID="rpReporte" runat="server" Height="900px"
                Style="width: 100% !important; display: table !important; margin: 0px; overflow: auto !important;" AsyncRendering="true">
            </rsweb:ReportViewer>

        </div>

        <script type="text/javascript">
            var relacionFechasSum = [];

            function getMensajeMultiselect() {
                var msg = {
                    clear: "limpiar",
                    noData: "No hay datos",
                    deleteTag: "borrar",
                    singleTag: "item(s) seleccionados"
                };
                return msg;
            };

            $(function () {

                $("#selFechaDesde").change(function () {
                    MarcarFechasHastaByDesde(this.value);
                });

                $("input[name='fechaHastaSum").change(function () {
                    var valorDesdeActual = $("#selFechaDesde option:selected").val();
                    var valorHastaActual = $(this).val();
                    var relacionDesde = GetRelacionByDesde(valorDesdeActual);
                    if ($(this).is(":checked")) {
                        // Se marca una opcion de fecha hasta
                        if (relacionDesde == null) {
                            relacionFechasSum.push({ Desde: valorDesdeActual, Hasta: [valorHastaActual] });
                        } else {
                            relacionDesde.Hasta.push(valorHastaActual);
                        }
                        $("#selFechaDesde option:selected").css("background-color", "lightblue");
                    } else {
                        // Se desmarca una opcion de fecha hasta
                        if (relacionDesde == null) // <<-- NO debe ocurrir, pero por si acaso
                            return;
                        else {
                            for (var j = 0; j < relacionDesde.Hasta.length; j++) {
                                if (relacionDesde.Hasta[j] == valorHastaActual) {
                                    relacionDesde.Hasta.splice(j, 1);
                                    break;
                                }
                            }
                            if (relacionDesde.Hasta.length == 0) {
                                $("#selFechaDesde option:selected").css("background-color", "");
                            }
                        }
                    }
                });

                MarcarDesdeHastaByHf("<%= this.hfRelacionDesdeHasta.Value %>");
                chequeaRangoFechas();


                var urlSite = GetUrl();

                $("#txtFechaDesde").kendoDatePicker({
                    culture: "es-CL",
                    format: 'dd/MM/yyyy',
                    change: chequeaRangoFechas
                });
                $("#txtFechaHasta").kendoDatePicker({
                    culture: "es-CL",
                    format: 'dd/MM/yyyy',
                    change: chequeaRangoFechas
                });

                $("#dtcFechaResolEstimDesde").kendoDatePicker({
                    culture: "es-CL",
                    format: 'dd/MM/yyyy'
                });

                $("#dtcFechaResolEstimHasta").kendoDatePicker({
                    culture: "es-CL",
                    format: 'dd/MM/yyyy'
                });

                $("#tbUnidadAsign").kendoMultiSelect({
                    placeholder: "Seleccione Unidad Técnica...",
                    messages: getMensajeMultiselect(),
                    dataTextField: "Nombre",
                    dataValueField: "Nombre",
                    dataSource: {
                        transport: {
                            dataType: "json",
                            read: urlSite + "/Api/Generico/<%=(int)Gedoc.WebReport.Enums.ListasGenericas.UnidadTecnia%>?todos=0"
                        }
                    },
                    filter: 'contains',
                    value: $('#<%= hfUnidadTecnica.ClientID %>').val()
                        ? $('#<%= hfUnidadTecnica.ClientID %>').val().split(",")
                        : []
                });

                /**
                 * Se asigna "Nombre" a dataValueField para q se envíe al servicio Wss.Gedoc (servicio q devuelve datos de los reportes)
                 * el titulo de los filtros seleccionados en vez del id de los filtros
                 */
                $("#tbProfesionalUt").kendoMultiSelect({
                    placeholder: "Seleccione Profesional...",
                    messages: getMensajeMultiselect(),
                    dataTextField: "Nombre",
                    dataValueField: "Nombre",
                    dataSource: {
                        transport: {
                            dataType: "json",
                            read: urlSite + "/Api/Generico/<%=(int)Gedoc.WebReport.Enums.ListasGenericas.Profesionales%>?todos=0"
                        }
                    },
                    filter: 'contains',
                    value: $('#<%= hfProfUt.ClientID %>').val()
                        ? $('#<%= hfProfUt.ClientID %>').val().split(",")
                        : []
                });

                $("#tbUtCopia").kendoMultiSelect({
                    placeholder: "Seleccione Unidad Técnica en Copia...",
                    messages: getMensajeMultiselect(),
                    dataTextField: "Nombre",
                    dataValueField: "Nombre",
                    dataSource: {
                        transport: {
                            dataType: "json",
                            read: urlSite + "/Api/Generico/<%=(int)Gedoc.WebReport.Enums.ListasGenericas.UnidadTecnicaCopia%>?todos=0"
                        }
                    },
                    filter: 'contains',
                    value: $('#<%= hfUnidadTecnicaCopia.ClientID %>').val()
                        ? $('#<%= hfUnidadTecnicaCopia.ClientID %>').val().split(",")
                        : []
                });


                $("#tbMotivoCierre").kendoMultiSelect({
                    placeholder: "Seleccione Motivo de Cierre...",
                    messages: getMensajeMultiselect(),
                    dataTextField: "Nombre",
                    dataValueField: "Nombre",
                    dataSource: {
                        transport: {
                            dataType: "json",
                            read: urlSite + "/Api/Generico/<%=(int)Gedoc.WebReport.Enums.ListasGenericas.MotivoCierre%>?todos=0"
                        }
                    },
                    filter: 'contains',
                    value: $('#<%= hfMotivoCierre.ClientID %>').val()
                        ? $('#<%= hfMotivoCierre.ClientID %>').val().split(",")
                        : []
                });


                $("#tbEstado").kendoMultiSelect({
                    placeholder: "Seleccione Estado...",
                    messages: getMensajeMultiselect(),
                    dataTextField: "Nombre",
                    dataValueField: "Nombre",
                    dataSource: {
                        transport: {
                            dataType: "json",
                            read: urlSite + "/Api/Generico/<%=(int)Gedoc.WebReport.Enums.ListasGenericas.Estados%>?todos=0"
                        }
                    },
                    filter: 'contains',
                    value: $('#<%= hfEstado.ClientID %>').val()
                        ? $('#<%= hfEstado.ClientID %>').val().split(",")
                        : []
                });


                $("#tbTipoInstRemitente").kendoMultiSelect({
                    placeholder: "Seleccione Tipo Institución...",
                    messages: getMensajeMultiselect(),
                    dataTextField: "Nombre",
                    dataValueField: "Nombre",
                    dataSource: {
                        transport: {
                            dataType: "json",
                            read: urlSite + "/Api/Generico/<%=(int)Gedoc.WebReport.Enums.ListasGenericas.TipoInstitucion%>?todos=0"
                        }
                    },
                    filter: 'contains',
                    value: $('#<%= hfTipoInstRem.ClientID %>').val()
                        ? $('#<%= hfTipoInstRem.ClientID %>').val().split(",")
                        : []
                });

                $("#tbTipoInstDestinatario").kendoMultiSelect({
                    placeholder: "Seleccione Tipo Institución...",
                    messages: getMensajeMultiselect(),
                    dataTextField: "Nombre",
                    dataValueField: "Nombre",
                    dataSource: {
                        transport: {
                            dataType: "json",
                            read: urlSite + "/Api/Generico/<%=(int)Gedoc.WebReport.Enums.ListasGenericas.TipoInstitucion%>?todos=0"
                        }
                    },
                    filter: 'contains',
                    value: $('#<%= hfTipoInstDes.ClientID %>').val()
                        ? $('#<%= hfTipoInstDes.ClientID %>').val().split(",")
                        : []
                });

                $("#tbSemaforo").kendoMultiSelect({
                    placeholder: "Seleccione Semáforo...",
                    messages: getMensajeMultiselect(),
                    dataTextField: "Nombre",
                    dataValueField: "Nombre",
                    dataSource: {
                        transport: {
                            dataType: "json",
                            read: urlSite + "/Api/Generico/<%=(int)Gedoc.WebReport.Enums.ListasGenericas.Semaforo%>?todos=0"
                        },
                        pageSize: 10
                    },
                    value: $('#<%= hfSemaforo.ClientID %>').val()
                        ? $('#<%= hfSemaforo.ClientID %>').val().split(",")
                        : []
                });

                $("#tbPrioridadReq").kendoMultiSelect({
                    placeholder: "Seleccione Prioridad...",
                    messages: getMensajeMultiselect(),
                    dataTextField: "Nombre",
                    dataValueField: "Nombre",
                    dataSource: {
                        transport: {
                            dataType: "json",
                            read: urlSite + "/Api/Generico/<%=(int)Gedoc.WebReport.Enums.ListasGenericas.Prioridad%>?todos=0"
                        }
                    },
                    filter: 'contains',
                    value: $('#<%= hfPrioridad.ClientID %>').val()
                        ? $('#<%= hfPrioridad.ClientID %>').val().split(",")
                        : []
                });

                $("#tbEtiqueta").kendoMultiSelect({
                    placeholder: "Seleccione Etiqueta...",
                    messages: getMensajeMultiselect(),
                    dataTextField: "Nombre",
                    dataValueField: "Nombre",
                    dataSource: {
                        transport: {
                            dataType: "json",
                            read: urlSite + "/Api/Generico/<%=(int)Gedoc.WebReport.Enums.ListasGenericas.Etiqueta%>?todos=0"
                        }
                    },
                    filter: 'contains',
                    value: $('#<%= hfEtiqueta.ClientID %>').val()
                        ? $('#<%= hfEtiqueta.ClientID %>').val().split(",")
                        : []
                });

                $("#tbRegion").kendoMultiSelect({
                    placeholder: "Seleccione Región...",
                    messages: getMensajeMultiselect(),
                    dataTextField: "Nombre",
                    dataValueField: "Nombre",
                    dataSource: {
                        transport: {
                            dataType: "json",
                            read: urlSite + "/Api/Generico/<%=(int)Gedoc.WebReport.Enums.ListasGenericas.Region%>"
                        }
                    },
                    change: function (e) {
                        var value = this.value();
                        $("#tbComuna").data("kendoMultiSelect").dataSource.read();
                    },
                    value: $('#<%= hfRegion.ClientID %>').val()
                        ? $('#<%= hfRegion.ClientID %>').val().split(",")
                        : [],
                    filter: 'contains',
                });

                $("#tbComuna").kendoMultiSelect({
                    placeholder: "Seleccione Comuna...",
                    messages: getMensajeMultiselect(),
                    dataTextField: "Nombre",
                    dataValueField: "Nombre",
                    dataSource: {
                        transport: {
                            dataType: "json",
                            read: urlSite + "/Api/Generico/<%=(int)Gedoc.WebReport.Enums.ListasGenericas.Comuna%>"
                        }
                    },
                    value: $('#<%= hfComuna.ClientID %>').val()
                        ? $('#<%= hfComuna.ClientID %>').val().split(",")
                        : [],
                    filter: 'contains',
                });



            });

            function onSelect(e) {
                if ("kendoConsole" in window) {
                    var dataItem = e.dataItem;
                    kendoConsole.log("event :: select (" + dataItem.text + " : " + dataItem.value + ")");
                }
            };

            function onDeselect(e) {
                if ("kendoConsole" in window) {
                    var dataItem = e.dataItem;
                    kendoConsole.log("event :: deselect (" + dataItem.text + " : " + dataItem.value + ")");
                }
            };

            function chequeaRangoFechas() {
                var fechaD = $("#txtFechaDesde").val();
                var fechaH = $("#txtFechaHasta").val();
                $("#rowInfoFechas").hide();
                if (moment(fechaD, "DD/MM/YYYY").isValid() && moment(fechaH, "DD/MM/YYYY").isValid()) {
                    if (moment(fechaH, "DD/MM/YYYY").diff(moment(fechaD, "DD/MM/YYYY"), "month") > 6) {
                        $("#rowInfoFechas").show();
                    }
                }
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

            function MarcarDesdeHastaByHf(relaciones) {
                relaciones = relaciones.split(";");
                for (var i = 0; i < relaciones.length; i++) {
                    var parejaRelacion = relaciones[i].split(",");
                    if (parejaRelacion.length != 2)
                        continue;
                    var valorDesde = parejaRelacion[0];
                    var valorHasta = parejaRelacion[1];
                    var relacionDesde = GetRelacionByDesde(valorDesde);
                    if (relacionDesde == null) {
                        relacionFechasSum.push({ Desde: valorDesde, Hasta: [valorHasta] });
                    } else {
                        relacionDesde.Hasta.push(valorHasta);
                    }
                    $("#selFechaDesde option[value=" + valorDesde + "]").css("background-color", "lightblue");
                }
                MarcarFechasHastaByDesde(1);
            }

            function MarcarFechasHastaByDesde(valorDesde) {
                $("input[name='fechaHastaSum").prop("checked", false);
                var relacionDesde = GetRelacionByDesde(valorDesde);
                if (relacionDesde == null)
                    return;
                for (var j = 0; j < relacionDesde.Hasta.length; j++) {
                    idHastaMarcado = "#hasta" + relacionDesde.Hasta[j];
                    $(idHastaMarcado).prop("checked", true);
                }
            }

            function GetRelacionByDesde(valorDesde) {
                for (var i = 0; i < relacionFechasSum.length; i++) {
                    if (relacionFechasSum[i].Desde == valorDesde)
                        return relacionFechasSum[i];
                }
                return null;
            }

            function limpiarFiltros() {
                return false;
            }

            function VerReporteClick() {

                if (!validaRangoFechas()) {
                    return false;
                }

                $.LoadingOverlay("show", {});

                var valor = $("#tbEtiqueta").data("kendoMultiSelect").value();
                $('#<%=hfEtiqueta.ClientID%>').val(valor.join());
                valor = $("#tbRegion").data("kendoMultiSelect").value();
                $('#<%=hfRegion.ClientID%>').val(valor.join());
                valor = $("#tbComuna").data("kendoMultiSelect").value();
                $('#<%=hfComuna.ClientID%>').val(valor.join());
                valor = $("#tbUnidadAsign").data("kendoMultiSelect").value();
                $('#<%=hfUnidadTecnica.ClientID%>').val(valor.join());
                valor = $("#tbProfesionalUt").data("kendoMultiSelect").value();
                $('#<%=hfProfUt.ClientID%>').val(valor.join());
                valor = $("#tbUtCopia").data("kendoMultiSelect").value();
                $('#<%=hfUnidadTecnicaCopia.ClientID%>').val(valor.join());
                valor = $("#tbMotivoCierre").data("kendoMultiSelect").value();
                $('#<%=hfMotivoCierre.ClientID%>').val(valor.join());
                valor = $("#tbEstado").data("kendoMultiSelect").value();
                $('#<%=hfEstado.ClientID%>').val(valor.join());
                valor = $("#tbTipoInstRemitente").data("kendoMultiSelect").value();
                $('#<%=hfTipoInstRem.ClientID%>').val(valor.join());
                valor = $("#tbTipoInstDestinatario").data("kendoMultiSelect").value();
                $('#<%=hfTipoInstDes.ClientID%>').val(valor.join());
                valor = $("#tbSemaforo").data("kendoMultiSelect").value();
                $('#<%=hfSemaforo.ClientID%>').val(valor.join());
                valor = $("#tbPrioridadReq").data("kendoMultiSelect").value();
                $('#<%=hfPrioridad.ClientID%>').val(valor.join());

                var relacionDHStr = "";
                for (var i = 0; i < relacionFechasSum.length; i++) {
                    for (var j = 0; j < relacionFechasSum[i].Hasta.length; j++) {
                        relacionDHStr += (relacionDHStr == "" ? "" : ";") + relacionFechasSum[i].Desde + "," + relacionFechasSum[i].Hasta[j];
                    }
                }
                $("#<%= hfRelacionDesdeHasta.ClientID %>").val(relacionDHStr);

            }


            function GetUrl() {
                //var urlSite = "http://" + window.location.href.split('/')[2] + "/" + window.location.href.split('/')[3];
                //var urlSite = window.location.origin;
                var urlSite = window.location.href.substring(0, window.location.href.indexOf("VerAuditoria"));
                return urlSite;
            }

            function btnVolverClick() {
                parent.postMessage("volverReporte", "*");
                return false;
            }

        </script>


    </form>
</body>
</html>
