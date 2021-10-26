
var Main = Main || {

    formatoFechaMom: "D/M/YYYY",
    formatoFechaLargoMom: "D/M/YYYY HH:mm",
    formatoFechaMYMom: "M/YYYY",
    formatoFecha: "dd/MM/yyyy",
    formatoFechaLargo: "dd/MM/yyyy HH:mm",
    formatoFechaMY: "MM/yyyy",

    idWin: 0,
    kendoWin: {},


    getFormatoLargoFechaGrid: function () {
        return "{0:dd/MM/yyyy HH:mm}";
    },

    getFormatoCortoFechaGrid: function () {
        return "{0:dd/MM/yyyy}";
    },

    getTemplateFicha: function (docIngreso, idIngreso, texto) {
        texto = texto || "Ficha";
        var baseUrl = $('base').attr('href');
        var urlFicha = baseUrl + "Requerimiento/Ficha?id=" + idIngreso;
        var template = "<a href='#' class='enlace-grilla' onclick='Main.muestraVentanaModal(\"Ficha de Requerimiento - " + docIngreso + "\",\"" + urlFicha + "\", 900, \"100vp\", false, \"winFormFichaIng\"); return false;'>" + texto + "</a>"
        return template;
    },

    getTemplateResumen: function (docIngreso, idIngreso) {
        var baseUrl = $('base').attr('href');
        var urlResumen = baseUrl + "Requerimiento/FichaResumen?id=" + idIngreso;
        var template = "<a href='#' class='enlace-grilla' onclick='Main.muestraVentanaModal(\"Vista Resumen - " + docIngreso + "\",\"" + urlResumen + "\", null, \"100vp\", false, \"winFormFichaIngRes\"); return false;'> Resumen </a>"
        return template;
    },

    getMensajeMultiselect: function () {
        var msg = {
            clear: "limpiar",
            noData: "No se encontraron datos",
            deleteTag: "borrar",
            singleTag: "item(s) seleccionados"
        };
        return msg;
    },

    getMensajeCombobox: function () {
        var msg = {
            clear: "limpiar",
            noData: "No se encontraron datos"
        };
        return msg;
    },

    getMultiSelectConfig: function (ctrol, url, txtField, idField, value, pageSize) {
        txtField = txtField || "Titulo";
        idField = idField || "Id";
        var cfg = {
            dataTextField: txtField,
            dataValueField: idField,
            messages: Main.getMensajeMultiselect(),
            tagTemplate: kendo.template($("#tagMultiTemplate").html()),
            dataSource: Main.getDataSourceKendoGenerico(ctrol, url, pageSize),
            value: value,
            format: "contains"
        };
        return cfg;
    },

    getDataSourceKendoGenerico: function (ctrol, url, pageSize, onRequestEnd, data) {
        var ds = new kendo.data.DataSource({
            transport: {
                read: {
                    type: "POST",
                    dataType: "json",
                    url: url,
                    data: data
                }
            },
            schema: {
                data: "Data",
            },
            pageSize: pageSize,
            requestStart: function () {
                Main.enEspera(ctrol, true);
            },
            requestEnd: function (e) {
                Main.enEspera(ctrol, false);
                Main.notificaResultado(e, true);
                if (onRequestEnd) {
                    onRequestEnd(e);
                }
            }
        });
        //// Si es un ComboBox se le asigna el mensaje en español para el botón de Clear y el mensaje de q no hay datos 
        //if (ctrol.data("kendoComboBox")) {
        //    ctrol.data("kendoComboBox").setOptions({
        //        messages: {
        //            clear: "limpiar",
        //            noData: "No se encontraron datos"
        //        }
        //    });
        //}
        return ds;
    },

    getDataSourceKendoGenericoVirtual: function (ctrol, url, pageSize, onRequestEnd, data) {
        pageSize = pageSize || 80;
        var ds = new kendo.data.DataSource({
            type: "aspnetmvc-ajax",
            transport: {
                read: {
                    type: "POST",
                    dataType: "json",
                    url: url,
                    data: data
                }
            },
            schema: {
                data: "Data",
                total: "Total",
            },
            pageSize: pageSize,
            serverPaging: true,
            serverFiltering: true,
            requestStart: function () {
                Main.enEspera(ctrol, true);
            },
            requestEnd: function (e) {
                Main.enEspera(ctrol, false);
                Main.notificaResultado(e, true);
                if (onRequestEnd) {
                    onRequestEnd(e);
                }
            }
        });
        return ds;
    },

    getMultiSelectConfigVirtual: function (ctrol, url, urlValueMapped, txtField, idField, value, pageSize, soloReqCerrados) {
        txtField = txtField || "Titulo";
        idField = idField || "Id";
        pageSize = pageSize || 80;
        var cfg = {
            dataTextField: txtField,
            dataValueField: idField,
            messages: Main.getMensajeMultiselect(),
            tagTemplate: kendo.template($("#tagMultiTemplate").html()),
            dataSource: Main.getDataSourceKendoGenericoVirtual(ctrol, url, pageSize),
            value: value,
            filter: "contains",
            height: 520,
            virtual: {
                itemHeight: 26,
                mapValueTo: "dataItem",
                valueMapper: function (options) {
                    $.ajax({
                        url: urlValueMapped,
                        type: "POST",
                        data: $.param({ ids: options.value, cerrado: soloReqCerrados || false }, true),
                        success: function (data) {
                            if (data.Resultado && data.Resultado.Codigo < 0) {
                                Main.showError(data.Resultado.Mensaje);
                            } else {
                                options.success(data.Data);
                            }
                        }
                    });
                }
            }
        };
        return cfg;
    },

    onOpenModal: function (e) {
        // deshabilitar scroll en parent. TODO: hacerlo solo para cuando la ventana se abre ocupando el 100% del viewport
        // $("html, body").css("overflow", "hidden");
        kendo.ui.progress(e.sender.element, true);
    },

    onRefreshModal: function (e) {
        kendo.ui.progress(e.sender.element, false);

        var winId = e.sender.element[0].id;
        Main.idWin = winId;

        //var win = e.sender; //.wrapper; // $("#" + e.sender.id).data("kendoWindow");//get window
        //win.setOptions({
        //});
        //win.maximize(); //maximize window

        //var btn = $("#" + e.sender.element[0].id).find(".confirm_yes");
        //$('.close-modal').click(function () {
        //    result = false;
        //    win.close();
        //});

        //btn.click(function () {
        //    $("#" + e.sender.element[0].id).data('kendoWindow').close(event, result);
        //});

    },

    muestraVentanaModal: function (titulo, url, width, height, ocultarClose, nombreWin, idByName, ocultarMaximize) {
        var result = false;
        var ajustarHeight = false;
        var ajustarTop = false;
        var dfd = $.Deferred();
        var pinned = false; // true; // 
        var enIFrame = false; // (typeof enIFrame === 'boolean' && enIFrame === false) ? false : true; // false; // 
        var enIFrame2 = true; // false; // 
        var parent = "";
        var maximizar = height === "MAX";
        var minHeight = null;
        ocultarMaximize = maximizar ? maximizar : ocultarMaximize;
        var acciones = ocultarMaximize ? [] : ["Maximize"];
        if (!ocultarClose) {
            acciones.push("Close");
        }

        var id = "";
        if (idByName) 
            id = "window_" + nombreWin;
        else
            id = "window_" + (new Date()).getTime();

        if (!$("#" + id).length) {
            if (!parent) {
                //$("#divNotificacion").prepend('<div id="' + id + '"></div>');
                $(document.body).prepend('<div id="' + id + '"></div>');
            } else {
                parent.append('<div id="' + id + '"></div>');
            }
        }
        width = width || "800px";
        if (height && height.toString().startsWith("MIN")) {
            // Si se define en el height la altura minima de la forma "MINxxx". Se toma la altura minima definida en height y se hace height="" para q la altura total se ajuste de acuerdo al contenido
            minHeight = height.replace("MIN", "");
            height = "";
        }
        if (height !== "") { // si height == "" se deja tal cual y la ventana modal asume el tamaño de su contenido
            if (height == "100vp") { // si es mostrar la ventana al 100% del tamaño del viewport
                height = document.documentElement.clientHeight;
                ajustarHeight = true;
            } else {
                height = (height != "MAX" ? height : "") || "95%";
            }
        } else {
            ajustarTop = true; // cuando height es igual a "" la ventana modal asume el tamaño de su contenido pero queda ubicada del centrao de la pantalla hacia abajo, es necesario ubciarla en en la parte de arriba de la pantalla para q se vea mejor
        }

        minHeight = minHeight || height;
        var wnd = $("#" + id).kendoWindow({
            modal: true,
            draggable: true,
            pinned: pinned,
            visible: false,
            iframe: enIFrame, // false, //------------
            resizable: true,
            width: width,
            height: height,
            minWidth: width,
            minHeight: minHeight,
            open: Main.onOpenModal,
            refresh: Main.onRefreshModal,
            title: titulo || "",
            close: function (e, resultValue) {
                // habilitar scroll en parent
                // setTimeout(function () { $("html, body").css("overflow", ""); }, 100);
                Main.kendoWin[nombreWin] = null;
                this.destroy();
                return dfd.resolve(result);
            },
            actions: acciones,
            //content: {
            //    url: url,
            //    data: { customerId: 100 }
            //}
        }).data("kendoWindow");

        wnd.bind('returnvalue',
            function (v) {
                result = v.resultado || v;
                this.close();
            });

        wnd.refresh({ url: url,/* iframe: enIFrame2  */ });
        wnd.center();
        wnd.open();
        if (maximizar) {
            wnd.maximize();
        }

        if (ajustarHeight) { // se ajusta la altura, se le resta la altura del título de la ventana
            var heightTitulo = $("#" + id).siblings(".k-window-titlebar").outerHeight();
            height = height - heightTitulo;
            wnd.setOptions({
                minHeight: height,
                height: height,
                //position: {
                //    top: 0
                //},
                pinned: true
            });
        }
        if (ajustarTop) { // se ajusta la posición vertical de la ventana
            wnd.setOptions({
                position: {
                    top: window.pageYOffset + 10
                }
            });
        }

        if (nombreWin) {
            Main.kendoWin[nombreWin] = wnd;
        }

        //return wnd;
        return dfd.promise();
    },

    muestraVentanaRegmonModal: function (titulo, url, nombreWin) {
        var id = "window_" + (new Date()).getTime();
        if ($("#" + id).length == 0) {
            $("#divNotificacion").prepend('<div id="' + id + '"></div>');
            //$(document.body).append('<div id="' + id + '"></div>');
        }

        var wnd = $("#" + id).kendoWindow({
            modal: true,
            visible: false,
            iframe: true,
            width: "900px",
            height: "600px",
            open: Main.onOpenModal,
            refresh: Main.onRefreshModal,
            title: titulo || "",
            close: function (e) {
                this.destroy();
            },
            actions: ["Close"]
        }).data("kendoWindow");

        wnd.refresh({ url: url });
        wnd.open().center();
        wnd.maximize();

        if (nombreWin) {
            Main.kendoWin[nombreWin] = wnd;
        }

        return wnd;
    },

    cierraModal: function (ctrol, returnData, nombreWin) {
        var kwin;
        if (nombreWin) {
            kwin = Main.kendoWin[nombreWin];
        }
        if (!kwin) {
            var winCtrolJq = ctrol.closest("[data-role=window]");
            if (!winCtrolJq.length) {
                winCtrolJq = $("[data-role=window]");
                if (!winCtrolJq.length) {
                    winCtrolJq = window.parent.$("[data-role=window]");
                    if (!winCtrolJq.length) {
                        return;
                    }
                }
            }
            var kwin = winCtrolJq.data("kendoWindow");
        }
        if (kwin) {
            kwin.trigger("returnvalue", returnData);
            //kwin.close();
        }
    },

    maximizaModal: function (ctrol, nombreWin, ocultaBtnMax) {
        var kwin;
        if (nombreWin) {
            kwin = Main.kendoWin[nombreWin];
        }
        if (!kwin) {
            var winCtrolJq = ctrol.closest("[data-role=window]");
            if (!winCtrolJq.length) {
                winCtrolJq = $("[data-role=window]");
                if (!winCtrolJq.length) {
                    winCtrolJq = window.parent.$("[data-role=window]");
                    if (!winCtrolJq.length) {
                        return;
                    }
                }
            }
            var kwin = winCtrolJq.data("kendoWindow");
        }
        if (kwin) {
            kwin.maximize();
            if (ocultaBtnMax) {

            }
        }
    },

    muestraDialog: function (titulo, mensaje, tipo) {
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
    },

    showError: function (mensaje) {
        return Main.muestraDialog("Error", mensaje, "ERROR");
    },

    showInfo: function (mensaje, titulo) {
        return Main.muestraDialog(titulo || "Atención", mensaje, "INFO");
    },

    showMensaje: function (mensaje, esInfo) {
        if (esInfo)
            return Main.showInfo(mensaje);
        else
            return Main.showError(mensaje);
    },

    showAlert: function (mensaje) {
        return Main.muestraDialog("Atención", mensaje, "ALERT");
    },

    showConfirm: function (mensaje, titulo, esSiNo) {
        return Main.muestraDialog(titulo || "Confirmación", mensaje, esSiNo == true ? "CONFIRM_SINO"  : "CONFIRM");
    },

    muestraNotificacionPopup: function (mensaje, tipo) {
        if ($("#divNotificacion").length == 0) {
            $(document.body).append("<div id='divNotificacion'></div>");
        }

        var notif = $("#divNotificacion").kendoNotification({
            position: {
                top: 50
            }
        }).data("kendoNotification");
        notif.show(mensaje, tipo);
    },

    showInfoPopup: function (mensaje) {
        Main.muestraNotificacionPopup(mensaje, "info");
    },

    showAlertPopup: function (mensaje) {
        Main.muestraNotificacionPopup(mensaje, "warning");
    },

    showErrorPopup: function (mensaje) {
        Main.muestraNotificacionPopup(mensaje, "error");
    },

    showSuccessPopup: function (mensaje) {
        Main.muestraNotificacionPopup(mensaje, "success");
    },

    showPrompt: function (titulo, mensaje, textoGrabar, esTextbox, maxLength, esObligatorio) {
        var result = true;
        var dfd = $.Deferred();
        // Botón Grabar
        var acciones = [
            {
                text: textoGrabar || "Grabar",
                cssClass: "button-dlg",// no funciona. El cssClass no está implementado hasta la versión Kendo UI 2020 R2 SP1
                primary: true,
                action: function (e) {
                    var texto = $("#promptDialog #promptText").val();
                    if (esObligatorio && !texto.length) {
                        Main.showError("El campo es obligatorio, por favor, especifique un valor.");
                        return false; //e.preventDefault();
                    }
                    result = { result: true, texto: texto };
                    e.sender.close();
                }
            }];
        // botón Cancelar
        acciones.push({
            text: "Cancelar",
            cssClass: "button-dlg ml-2", // no funciona. El cssClass no está implementado hasta la versión Kendo UI 2020 R2 SP1
            action: function (e) {
                result = false;
                e.sender.close();
            }
        });
        // Asterisco de campo obligatorio
        if (esObligatorio) {
            mensaje += '<span class="marca-obligatorio" title="Este campo es obligatorio."> *</span>';
        }

        var minWidth = '600px';
        if (esTextbox) {
            mensaje += "<input type='text' id='promptText' class='form-control' " +
                "   style='width: 100% !important;'></input>";
        } else {
            maxLength = maxLength || 255;
            mensaje += "<textarea rows='4' cols='20' id='promptText' class='form-control texto-largo' " +
                " data-maxlength='" + maxLength + "' style='width: 100% !important; resize: both; " +
                " min-height:50px; min-width:" + minWidth + ";' data-spanleyenda='leyenda-pnPromptObservaciones'></textarea>" +
                " <span class='help-block-right' id='leyenda-pnPromptObservaciones'> </span>";
        }
        var claseDiv = ""; // "info-msg";
        var dialog = $("<div id='promptDialog' class='" + claseDiv + "'></div>").kendoDialog({
            //width: "500px",
            minWidth: minWidth,
            buttonLayout: "normal",
            title: titulo,
            content: mensaje,
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
                    // $(wr).find(".k-dialog-buttongroup button.k-button").addClass("button-dlg"); // Funciona, pero a pesar de agregarle la clase al botón no toma el width definido en la clase button-dlg
                    $(wr).find(".k-dialog-buttongroup button.k-button:not('.k-primary')").addClass("k-danger");
                }
            }
        });
        var dlg = dialog.data("kendoDialog").open();
        setTimeout(function () {
            if (Form) Form.configAreaEscritura();
            $("#promptDialog #promptText").focus();
        },
        500);
        return dfd.promise();
    },

    enEspera: function (control, mostrarEspera) {
        kendo.ui.progress(control, mostrarEspera);
    },

    wait: function (text, elm) {
        var opt = { text: text, textResizeFactor: 0.2, textColor: "#bc370f", imageColor: "#bc370f" };
        if (!elm) {
            $.LoadingOverlay("show", opt);
        } else {
            $(elm).LoadingOverlay("show", opt);
        }
    },

    unwait: function (elm) {
        if (!elm) {
            $.LoadingOverlay("hide");
        } else {
            $(elm).LoadingOverlay("hide");
        }
    },

    notificaResultado: function (e, enPopup) {
        var mensaje = "Lo sentimos, ha ocurrido un error inesperado;";
        if (e && e.response && e.response.Resultado && e.response.Resultado.Codigo < 0) {
            mensaje = e.response.Resultado.Mensaje;
            if (enPopup) {
                Main.showErrorPopup(mensaje);
            } else {
                Main.showError(mensaje);
            }
        }
    },

    acortaTexto: function (texto, long) {
        if (!texto || texto == 'null' || texto == null)
            return "";

        long = long || 10;
        if (texto && texto.length > long) {
            texto = texto.substring(0, long) + "...";
        }
        return texto;
    },

    escapeHtml: function (html) {
        var texto = $('<div/>').text(html).html();
        return texto;
    },

    fixPaletaColorPickerKendoEditor: function ($editor) {
        /* En la configuración del Editor de Kendo MVC no funciona asignar el Palette(Kendo.Mvc.UI.ColorPickerPalette.None)
           en el tool de FontColor ni de BackColor, al generarse el control al cargar la página siempre se asigna la paleta 
           Websafe.
           href. del bug: https://github.com/telerik/kendo-ui-core/issues/4996
           En esta misma URL se propone la sgte. solución alternativa:
           */
        var editor = $editor.getKendoEditor();
        editor.setOptions({
            tools: editor.options.tools.map(function (tool) {
                if (tool.name === "foreColor" || tool.name === "backColor") {
                    return { name: tool.name, palette: null };
                }

                return tool;
            })
        });
    },

    exportaKendoEditorToPdf: function ($editor, url, nombreArchivo) {
        $editor = $editor || $("#Contenido");
        if (!$editor) {
            console.error("exportaPdf(): Kendo UI Editor no encontrado o no especificado.");
            return false;
        }
        var editorK = $editor.data("kendoEditor");
        var contenido = editorK ? editorK.encodedValue() : "";
        var $formTmp = $('<form method="post" action="' +
                url +
            '" target="_blank"> ' +
                '<input type="hidden" name="contenido" id="hfContenidoTmp" value=""> ' +
                '<input type="hidden" name="nombreArchivo" id="hfNombreArchivoTmp" value=""> ' +
                '</form>')
            .appendTo('body');
        $("#hfContenidoTmp").val(contenido);
        $("#hfNombreArchivoTmp").val(nombreArchivo);
        $formTmp.submit().remove();
    }, 

    abreUrlTramite: function (urlAuth, user, secretKey, urlSolicitud, idSolicitudTram) {

        /* Abre la URL de la solicitud del sistema de trámites asociada al ingreso*/
        if (idSolicitudTram <= 0) {
            Main.showAlert('El ingreso no está asociado a una solicitud en el Sistema de Trámites.');
            return false;
        }
        //kendo.ui.progress($("#btTramites"), true);
        Main.wait("", "#btTramites");

        var data = { "UserName": user, "SecretKey": secretKey };

        $.ajax({
            type: "POST",
            url: urlAuth,
            data: data,
            dataType: 'text'
        }).done(function (token) {
            //kendo.ui.progress($("#btTramites"), false);
            Main.unwait("#btTramites");
            if (token) {
                urlSolicitud = urlSolicitud.replace('%token%', token);
                window.open(urlSolicitud, '_blank');
            } else {
                Main.showAlert('No se recibió respuesta del sistema de trámites, por favor, reintente la operación.');
            }
        }).fail(function (xhr, result, status) {
            //kendo.ui.progress($("#btTramites"), false);
            Main.unwait("#btTramites");
            if (xhr.status == 0) {
                Main.showAlert('No fue posible contactar la URL del sistema de trámite.');
            } else if (xhr.status == 404) {
                Main.showAlert('No es posible abrir la página. El sistema de trámite ha devuelto Error 404.');
            } else if (xhr.status == 500) {
                Main.showAlert('No es posible abrir la página. El sistema de trámite ha devuelto Error 500.');
            } else {
                Main.showAlert('No es posible abrir la página. Error desconocido devuelto por el sistema de trámite:\n' + xhr.responseText);
            }
        });

    }

};

// Flattens the Array produced by .serializeArray() into an Object
// with names as keys and values as their values.
$.fn.serializeObject = function () {
    var obj = {};
    var arr = this.serializeArray();

    $.each(arr, function () {
        var splt = this.name.split(".", 2);
        if (splt.length === 2) {
            if (obj[splt[0]] === undefined) {
                obj[splt[0]] = {};
            }
            obj[splt[0]][splt[1]] = this.value || '';
        }
        else if (obj[this.name] !== undefined) {
            if (!obj[this.name].push) {
                obj[this.name] = [obj[this.name]];
            }
            obj[this.name].push(this.value || '');
        }
        else {
            obj[this.name] = this.value || '';
        }
    });
    return obj;
};