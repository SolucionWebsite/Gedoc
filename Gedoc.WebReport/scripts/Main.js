var soloLectura = false;
var mostrandoAlert = false;
var comunasRegiones = [];
// var provinciasRegiones = [];
var formatoFecha = "D/M/YYYY";
var formatoFechaGrilla = "DD/MM/YYYY";
var formatoFechaLargo = "DD/MM/YYYY HH:mm";
var formatoFechaMY = "M/YYYY";
var fechaDocFull = "";
var validandoFecha = false;
var categoriasBusq = [];

var hayError = false;

function inicializaForm() {
    if (window.parent.waitDialog != null) {
        window.parent.waitDialog.close();
    }

    var valor = $(".search-comuna").attr("data-comunas");
    if (valor && valor != "") {
        comunasRegiones = stringToJson(valor);
    } else {
        comunasRegiones = [];
    }

    asignaEventos();

    $("div.text-right").removeClass("text-right");

    cambiaEstadoAdjuntos();

    $("input[name$='AdjuntaDoc'").click(function () {
        cambiaEstadoAdjuntos();
    });

    $("input[name$='tbCantAdjuntos'").keydown(keyDownSoloNumeros);
    $("input[name$='tbTelefono'").keydown(keyDownSoloNumeros);
    $("input[name$='tbTelefono'").on("paste", pegarSoloNumeros);
    $("input[name$='tbRequeNoReg'").keydown(keyDownSoloNumeros);
    $("input[name$='tbRemitenteEdit'").keydown(keyDownSoloLetras);
    $("input[name$='tbRemitenteEdit'").on("paste", pegarSoloLetras);
    $("input[name$='tbCargo'").keydown(keyDownSoloLetras);
    $("input[name$='tbCargo'").on("paste", pegarSoloLetras);
    $("input[name$='tbInstitucion'").keydown(keyDownSoloLetras);
    $("input[name$='tbInstitucion'").on("paste", pegarSoloLetras);


    creaReglasValidacion();

    fechaDocumentoChange();

    asignaLeyendas();

    // Control de algunos estilos desde JS
    var labelsEnFichaPdf = $(".form-pdf-label");
    if (false && labelsEnFichaPdf.length > 0) {
        $("#s4-workspace").css({ "height": "800px; important!" });
        //$("#s4-workspace").height(800);
    }

    //    if (($('h2:contains(Archivar Requerimientos cerrados)').length > 0 && $('table[id$="gridRequerimientos"]').length == 0) ||
    //        ($('h2:contains(Restaurar requerimientos archivados)').length > 0 && $('table[id$="gridRequerimientos"]').length == 0) ||
    //        ($('h2:contains(Requerimientos archivados)').length > 0 && $('table[id$="gridRequerimientos"]').length == 0)  ) {
    //       $('.container-fluid').width('800');
    //    }

    $("input[id*='chbTransparencia']").css("margin-top", "0px");
    $("label[for*='chbTransparencia']").css("margin-left", "5px");
    $("input[id*='chbSoloMesAnno']").css("margin-top", "0px");
    $("label[for*='chbSoloMesAnno']").css("margin-left", "5px");

    //quitando tags Html del campo Proyecto o Actividad
    var textoProy = $("textarea[id$='tbDescripcionProyAct'").val();
    if (textoProy && textoProy.indexOf('<') >= 0) {
        $("<div id='divOculto' style='display: none;'></div>").insertAfter("textarea[id$='tbDescripcionProyAct'");
        $("#divOculto").html(textoProy);
        textoProy = $("#divOculto").text();
        $("textarea[id$='tbDescripcionProyAct'").val(textoProy);
    }

    $("input[id*='tbCanalLlegadaTramite']").prop("disabled", true);

    $("#waitBuscarMn").hide();
    cambiaEstadoBusqRegmon();

}

function detectZoomBrowser(mostrarAlerta) {
    var browserZoomLevel = Math.round(window.devicePixelRatio * 100);
    var hayZoom = browserZoomLevel && browserZoomLevel != 100;
    if (mostrarAlerta && hayZoom) {
        showAlert("Atención. Se ha detectado que el zoom del navegador está en un valor diferente al 100%. Esto puede ocasionar un comportamiento incorrecto en los campos de búsqueda.", undefined, undefined, true);
    }
    return hayZoom;
}

function creaReglasValidacion() {

    try {
        $.validator.addMethod('searchValido', function (value, element, param) {
            if (param == true || param.chequear == true) {
                var valor = $(element).attr("data-id");
                var texto = $(element).val();
                var esVisible = $(element).is(':visible');
                var ignorarMenosUno = param.ignorarMenosUno && param.ignorarMenosUno == true;

                return !esVisible || ((valor != "" || (valor == "" && texto == "")) && (valor !== "-1" || ignorarMenosUno));
            }
            else
                return true;
        }, 'Valor incorrecto');

        $.validator.addMethod('numReqValido', function (value, element, param) {
            var valor = $(element).val();
            if (param == false || valor == '')
                return true;
            var minAnno = param.minAnno == undefined ? 0 : param.minAnno;
            var maxAnno = param.maxAnno == undefined ? 9999 : param.maxAnno;
            return isValidNumRequerimiento(valor, minAnno, maxAnno);
        }, 'Valor incorrecto');

        $.validator.addMethod('numOficValido', function (value, element, param) {
            var valor = $(element).val();
            if (param == false || valor == '')
                return true;
            var minAnno = param.minAnno == undefined ? 0 : param.minAnno;
            var maxAnno = param.maxAnno == undefined ? 9999 : param.maxAnno;
            return isValidNumOficio(valor, minAnno, maxAnno);
        }, 'Valor incorrecto');

        $.validator.addMethod("rangoFecha", function (value, element, params) {
            try {
                var date = moment(value, formatoFecha, true);
                var fromDate = moment((!params.from ? '01/01/1900' : params.from), formatoFecha)
                var toDate = moment((!params.to ? '01/01/2099' : params.to), formatoFecha)
                if (date.isValid() && date.isSameOrAfter(fromDate) && date.isSameOrBefore(toDate)) {
                    return true;
                }
            } catch (e) {
            }
            return false;
        }, function (params, element) {
            if (params.from && params.to)
                return 'Fecha incorrecta. Tiene que estar entre ' + params.from + ' y ' + params.to;
            else if (params.from)
                return 'Fecha incorrecta. Tiene que ser mayor o igual a ' + params.from;
            else if (params.to)
                return 'Fecha incorrecta. Tiene que ser menor o igual a ' + params.to;
        });

        $.validator.addMethod("menorQue", function (value, element, params) {
            if (validandoFecha)
                return true;
            try {
                validandoFecha = true;
                var date = moment(value, formatoFecha, true);
                var maxDate = moment(params.val(), formatoFecha, true);
                if ((date.isValid() && date.isSameOrBefore(maxDate)) || !maxDate.isValid()) {  // Es válido
                    var divErrorOtro = $('#divErrorTextFechaHasta'); // TODO: asociar el div de error con el control mediante attr data-
                    if (maxDate.isValid() &&
                        divErrorOtro.html().indexOf('La fecha tiene que ser mayor a') != -1) { // Se elimina en fecha hasta el mensaje de q la fecha tiene q ser mayor a ...
                        $("#" + params[0].id).removeClass('error');
                        divErrorOtro.html('');
                    }
                    validandoFecha = false;
                    return true;
                } else { // No es válido
                }

            } catch (e) {
                alert(e);
            }
            validandoFecha = false;
            return false;
        }, function (params, element) {
            return 'La fecha tiene que ser menor a ' + params.val();
        });

        $.validator.addMethod("mayorQue", function (value, element, params) {
            if (validandoFecha)
                return true;
            try {
                validandoFecha = true;
                var date = moment(value, formatoFecha, true);
                var minDate = moment(params.val(), formatoFecha, true);
                if ((date.isValid() && date.isSameOrAfter(minDate)) || !minDate.isValid()) { // Es válido
                    var divErrorOtro = $('#divErrorTextFechaDesde'); // TODO: asociar el div de error con el control mediante attr data-
                    if (minDate.isValid() &&
                        divErrorOtro.html().indexOf('La fecha tiene que ser menor a') != -1) { // Se elimina en fecha hasta el mensaje de q la fecha tiene q ser mayor a ...
                        $("#" + params[0].id).removeClass("error");
                        divErrorOtro.html("");
                    }
                    validandoFecha = false;
                    return true;
                } else { // No es válido
                }

            } catch (e) {
                alert(e);
            }
            validandoFecha = false;
            return false;
        }, function (params, element) {
            return 'La fecha tiene que ser mayor a ' + params.val();
        });

        $.validator.addMethod("formatearFecha", function (value, element, params) {
            try {
                if (element.id.endsWith('dtcFechaDocumento_dtcFechaDocumentoDate')) {
                    var chkSoloMesAnno = $("input[id$='chbSoloMesAnno']");
                    if (chkSoloMesAnno.is(":checked")) {
                        value = '01/' + value;
                    }
                }
                return moment(value, formatoFecha, true).isValid() && moment(value, formatoFecha, true).isSameOrBefore('2100-12-31');
            } catch (e) {
                console.log(e);
            }
            return false;
        }, function (params, element) {
            return 'Formato o valor de fecha incorrecto';
        });



    }
    catch (exc) {

    }

}

function keyDownSoloNumeros(e) {
    var permiteGuion = e.currentTarget.id.indexOf('tbRequeNoReg') != -1 &&
        e.currentTarget.value.indexOf('-') == -1;
    var longPermitida = e.currentTarget.id.indexOf('tbCantAdjuntos') != -1 ? 4 : 20;
    // Permitir: backspace, delete, tab, escape, enter /*y .*/
    if (($.inArray(e.keyCode, [46, 8, 9, 27, 13/*, 110, 190*/]) !== -1 ||
        // Permitir : Ctrl+A, Ctrl+C, Ctrl+V, Ctrl+X, Command+A
        ((e.keyCode === 65 || e.keyCode === 67 || e.keyCode === 86 || e.keyCode === 88) && (e.ctrlKey === true || e.metaKey === true)) ||
        // Permitir: home, end, left, right, down, up
        (e.keyCode >= 35 && e.keyCode <= 40) ||
        // Permitir: Guión
        ((e.keyCode == 189 || e.keyCode == 109) && permiteGuion))) {
        // se permite el caracter
        return;
    }
    // Si no es número se evita ingresar el caracter
    if (e.currentTarget.value.length >= longPermitida ||
        ((e.shiftKey || (e.keyCode < 48 || e.keyCode > 57)) && (e.keyCode < 96 || e.keyCode > 105))) {
        e.preventDefault();
    }
}

function pegarSoloLetras(e) {
    var texto = e.originalEvent.clipboardData.getData('text');
    if (!soloLetras(texto)) {
        e.preventDefault();
    }
}

function soloLetras(texto) {
    var letras = /^[A-Za-z]+$/;
    if (texto.match(letras)) {
        return true;
    }
    else {
        return false;
    }
}

function pegarSoloNumeros(e) {
    var texto = e.originalEvent.clipboardData.getData('text');
    if (!soloNumeros(texto)) {
        e.preventDefault();
    }
}

function soloNumeros(texto) {
    var numeros = /^[0-9]+$/;
    if (texto.match(numeros)) {
        return true;
    }
    else {
        return false;
    }
}


function keyDownSoloLetras(e) {
    // Permitir: backspace, tab, enter, escape, space,delete 
    if ($.inArray(e.keyCode, [8, 9, 13, 27, 32, 46]) !== -1 ||
        // Permitir: Ctrl+A, Command+A
        (e.keyCode === 65 && (e.ctrlKey === true || e.metaKey === true)) ||
        // Permitir: home, end, left, right, down, up
        (e.keyCode >= 35 && e.keyCode <= 40)) {
        // Si no es letra se evita ingresar el caracter
        return;
    }
    // Si no es letra se evita ingresar el caracter
    if ((e.keyCode < 65 || e.keyCode > 90) && e.keyCode != 192 /* ñ */) {
        e.preventDefault();
    }
}

function cambiaEstadoAdjuntos() {
    var desactivar = $("input[id$='rbAdjuntaDocNo'").is(":checked");
    $(".datos-adjuntos").prop("disabled", desactivar);
    $(".datos-adjuntos[contenteditable]").attr("contenteditable", !desactivar);
    $(".datos-adjuntos[contenteditable]").css("background-color", desactivar ? "lightgrey" : "");
}

function ToJavaScriptDate(value) {

    return fechaToStr(value);

    //if (value == null) return "";
    //var pattern = /Date\(([^)]+)\)/;
    //var results = pattern.exec(value);
    //var dt = new Date(parseFloat(results[1]));
    //return dt.getDate() + "/" + (dt.getMonth() + 1) + "/" + dt.getFullYear();
}


function getParameterByName(name, url) {
    if (!url) url = window.location.href;
    name = name.replace(/[\[\]]/g, "\\$&");
    var regex = new RegExp("[?&]" + name + "(=([^&#]*)|&|#|$)"),
        results = regex.exec(url);
    if (!results) return null;
    if (!results[2]) return '';
    return decodeURIComponent(results[2].replace(/\+/g, " "));
}

function asignaEventos() {
    $("button[data-enlace]").on("click", function (e) {
        var ctrolOrigId = "#" + $("." + $(this).attr("data-enlace")).attr("id");
        var itemSelected = $(ctrolOrigId + " option:selected");
        var valor = $(ctrolOrigId).val();
        var texto = itemSelected.text();
        if (valor != "" && texto != "") {
            var ctrolDstId = "#" + $("." + $(ctrolOrigId).attr("data-enlace")).attr("id");
            $(ctrolDstId).append($("<option>", { value: valor, text: texto }));
            itemSelected.remove();
        }
    });

    // Para check de Considerar sólo Mes y Año en fecha de documento
    $("input[id$='chbSoloMesAnno']").change(function () {
        fechaDocumentoChange();
    });

    // Para limitar la cantidad de caracteres de un div editable o un textarea
    $('div[contenteditable="true"][maxlength], textarea[data-maxlength]').on({
        'keydown': function (e) {
            var event = e.originalEvent;
            var maxLengh = parseInt($(this).attr('maxlength'));
            if (!maxLengh) {
                maxLengh = parseInt($(this).data('maxlength'));
            }

            var longTexto = $(this).is("textarea") ? $(this).val().length : $(this).text().length;
            if (event == undefined ||
                (longTexto >= maxLengh &&
                    event.keyCode != 8 &&    /*Backspace*/
                    event.keyCode != 46 &&   /*Delete*/
                    event.keyCode != 37 &&   /*Tecla izquieda*/
                    event.keyCode != 38 &&   /*Tecla izquieda*/
                    event.keyCode != 39 &&   /*Tecla arriba */
                    event.keyCode != 40))   /*Tecla abajo */ {
                event.preventDefault();
                return false;
            }
            //if ($(this).text().length >= maxLengh) {
            //   alert("maximo length");
            //}      
        },
        'keyup': function (e) {
            var maxLengh = parseInt($(this).attr('maxlength'));
            if (!maxLengh) {
                maxLengh = parseInt($(this).data('maxlength'));
            }
            var spanLeyenda = $(this).data('spanleyenda');
            if (!spanLeyenda) {
                var thisId = getOriginalIdAspControl(this);
                spanLeyenda = "leyenda-" + thisId;
            }

            // Info sobre la cantidad de caracteres restantes
            var longTexto = $(this).is("textarea") ? $(this).val().length : $(this).text().length;
            $("#" + spanLeyenda).text('Caracteres restantes:' + (maxLengh - longTexto));
        },
        'paste': function (e) {
            var maxLengh = parseInt($(this).attr('maxlength'));
            if (!maxLengh) {
                maxLengh = parseInt($(this).data('maxlength'));
            }

            var len = $(this).is("textarea") ? $(this).val().length : this.innerText.length,
                cp = e.originalEvent.clipboardData.getData('text');
            if ((len + cp.length) > maxLengh) {
                if ($(this).is("textarea"))
                    $(this).val($(this).val() + cp.substring(0, maxLengh - len));
                else
                    this.innerHTML += cp.substring(0, maxLengh - len);
                return false;
            }
        },
        'drop': function (e) {
            e.preventDefault();
            e.stopPropagation();
        }
    });

    // dar formato a nuevos números de Ingreso Histórico
    $("input[data-formatearHist='true']").blur(function () {
        // Ej formato correcto: 00001-2015
        var valor = $(this).val();
        if (isValidNumRequerimiento(valor)) {
            // sólo se da formato al número si es válido el formato
            var numero = valor.substring(0, valor.indexOf("-"));
            if (numero.length < 5) {
                var ceros = Array(5 - numero.length + 1).join("0");
                valor = ceros + valor;
                $(this).val(valor);
            }
        }
    });

    // dar formato a nuevos números de Ingreso (por ej en Requerimiento No Registrado)
    $("input[data-formatearReq='true']").blur(function () {
        // Ej formato correcto: 00001-2018
        var valor = $(this).val();
        if (isValidNumRequerimiento(valor)) {
            // sólo se da formato al número si es válido el formato
            var numero = valor.substring(0, valor.indexOf("-"));
            if (numero.length < 5) {
                var ceros = Array(5 - numero.length + 1).join("0");
                valor = ceros + valor;
                $(this).val(valor);
            }
        }
    });

    // dar formato a nuevos números de Despachos y Despachos Iniciativa
    $("input[data-formatearDesp='true']").blur(function () {
        // Ej formato correcto: 00001-2018
        var valor = $(this).val();
        if (isValidNumOficio(valor)) {
            // sólo se da formato al número si es válido el formato
            var numero = valor.substring(0, valor.indexOf("-"));
            if (numero.length < 5) {
                var ceros = Array(5 - numero.length + 1).join("0");
                valor = ceros + valor;
                $(this).val(valor);
            }
        }
    });

    // Para limpiar los datos en el modal de nuevo remitente cuando se abre el modal
    $('#modalNuevoRemitente').on('show.bs.modal', function (e) {
        limpiaRemitente();

        $("input[id$=tbRemitenteRut]").rut({ useThousandsSeparator: false })
            .on('rutInvalido', function (e) {
                if ($("input[id$=tbRemitenteRut]").val()) {
                    $("#lbErrorRemitenteRut").show();
                } else {
                    $("#lbErrorRemitenteRut").hide();
                }
            })
            .on('rutValido', function (e) {
                $("#lbErrorRemitenteRut").hide();
            });

        $("#waitGrabarNuevoRemit").hide();
        $("#waitGrabarNuevoRemit").prop("btnGrabarRemit", false);
        $("#waitGrabarNuevoRemit").prop("btnGCancelarRemit", false);

    })
}

function seteaSearchSimple(control, url, id, format, fields, success, afterDelete) {
    if (success == undefined) {
        success = asignaHiddenField;
    }
    if (afterDelete == undefined) {
        if (control == '.search-remitente' &&
            ($("input[id$='tbRemitente']").length > 0 || $("input[id$='tbDestinatario']").length > 0)) {
            afterDelete = afterDeleteSearchRemitente
        }
        else {
            afterDelete = afterDeleteSearchSimple;
        }
    }
    $(control).attr("autocomplete", "off");
    var controlDisabled = $(control).prop("disabled");
    $(control).magicsearch({
        dataSource: url,
        id: id,
        format: format,
        fields: fields,
        isClear: false,
        type: 'ajax',
        focusShow: true,
        maxShow: 10,
        ajaxOptions: {
        },
        success: success,
        afterDelete: afterDelete,
        noResult: "No se encontraron datos"
    });
    if (controlDisabled) {
        $(control).prop("disabled", true);
    }
    $(control).parent().css("width", "100%");
    $(control).css("width", "100%");

}

function seteaSearchMulti(control, url, id, format, fields, success, multiStyle, multiField, afterDelete) {
    if (!multiStyle) {
        multiStyle = {
            space: 5,
            width: 80
        }
    }
    // Fix de problema al mostrar multiples valores en search de Destinatarios en Copia de Despachos
    var maxWidth = $(control).css('max-width');
    if (!maxWidth || maxWidth == 'none' || maxWidth == '')
        maxWidth = 500;
    if (maxWidth < multiStyle.width + multiStyle.space) {
        maxWidth = multiStyle.width + multiStyle.space + 10;
        $(control).css('max-width', maxWidth);
    }

    if (!multiField)
        multiField = "Titulo";
    if (afterDelete == undefined) {
        afterDelete = afterDeleteSearchMulti;
    }

    var controlDisabled = $(control).prop("disabled");
    $(control).magicsearch({
        dataSource: url,
        id: id,
        format: format,
        fields: fields,
        isClear: false,
        multiple: true,
        type: 'ajax',
        focusShow: true,
        multiField: multiField,
        multiStyle: multiStyle,
        maxShow: 10,
        ajaxOptions: {
            success: function (data) {
                if (soloLectura == true) {
                    $(".magicsearch-wrapper *").attr("disabled", "disabled").off('click');
                    $(".multi-item-close").hide();
                }
                // Cuando es búsqueda desde Regmon se deshabilita el botón de cerrar q tienen los items en Región, Provincia y Comuna
                var closeCtrol = $("input[class^='search-'].datos-mn.search-ro").parent('div').find('.multi-item-close');
                if (closeCtrol) {
                    closeCtrol.hide();
                }
            }
        },
        success: success,
        afterDelete: afterDelete,
        noResult: "No se encontraron datos"
    });
    if (controlDisabled) {
        $(control).prop("disabled", true);
    }
    $(control).parent().css("width", "100%");
    $(control).css("width", "100%");


}

function asignaHiddenField($input, data) {
    asignaValorHiddenFieldSearch($input, $input.attr("data-id"));
    var idErrorCtrol = $input.attr("error-control");
    if (idErrorCtrol !== "")
        $("#" + idErrorCtrol).hide();

    return true;
}

function afterDeleteSearchSimple($input, data) {
    asignaValorHiddenFieldSearch($input, "");
}

function afterDeleteSearchMulti($input, data) {
    asignaHiddenField($input, data);
}

function afterDeleteSearchRemitente($input, data) {
    if (typeof muestraDatosRemitente !== 'undefined' && $.isFunction(muestraDatosRemitente)) {
        muestraDatosRemitente({
            "Nombre": "",
            "Id": "",
            "Cargo": "",
            "Institucion": "",
            "Tipo_Institucion": "",
            "Email": "",
            "Telefono": "",
            "Genero": "",
            "Direccion": "",
            "Rut": "",
        });
    }
}

function afterDeleteSearchCaso($input, data) {
    if (typeof muestraDatosCaso !== 'undefined' && $.isFunction(muestraDatosCaso)) {
        muestraDatosCaso({
            "Id": "",
            "FechaReferencia": ""
        });
    }
}

function asignaValorHiddenFieldSearch($input, valor) {
    var nombreHiddenField = $input.attr("hidden-field");
    $("input[id$='" + nombreHiddenField + "']").val(valor);
}

function datoSearchInvalido(idControl, chequear) {
    var $control = $("#" + idControl);
    if (chequear === false || $control.length == 0) {
        return false;
    }
    var hayError = false;
    var idErrorCtrol = "#" + $control.attr("error-control");
    $(idErrorCtrol).hide();
    if ($control.attr("data-id") == "")
    //        || $control.attr("data-id") == "-1") 
    {
        $(idErrorCtrol).show();
        hayError = true;
    }
    return hayError;
}

function GetSiteUrl() {
    return window.location.protocol + "//" + window.location.host + _spPageContextInfo.siteServerRelativeUrl;
    //var ctx = new SP.ClientContext;
    //var site = ctx.get_site();
    //ctx.load(site);
    //// ctx.executeQueryAsync(function(s, a){alert(site.get_url())});
    //return site.get_url();
}

function callWebMethod(queryParam, httpMethod, jsonData, functionSuccess, errorFunction) {
    var siteUrl = GetSiteUrl();
    var url = siteUrl + "/_layouts/dibam.cmn/AjaxHandler.ashx" + queryParam;
    $.ajax({
        type: httpMethod,
        url: url,
        data: jsonData,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: functionSuccess,
        error: errorFunction
    });
}

function errorGenericCallBack(jqXHR, textStatus, errorThrown) {
    alert(textStatus);
}

$(function () {
    $("input[type=text], textarea").addClass("input-sm");

    // En la master page se había agregado el estilo .form-control {300px !important} q ya se eliminó sin embargo no
    // se refleja este cambio en el navegado y sigue apareciendo este estilo para .form-control por eso acá se fuerza 
    // el width al 100% para sobreescribir el estilo de la master page:
    $(".form-control").each(function () {
        try { // ignorar error "Object doesn't support property or method 'setProperty'", posiblemente por un Id de control igual a alguna variable
            this.style.setProperty("width", "100%", "important");
        }
        catch (err) {
            console.log("INFO - " + this);
        }
    });

    //$('[data-target="#modalNuevoRemitente"]').removeData('toggle');    
    $('[data-target="#modalNuevoRemitente"]').removeAttr('data-toggle');

    $('[data-target="#modalNuevoRemitente"]').click(function () {
        showModalRemitente();
    });

    // Leyenda de Bandeja de entrada 
    var mesesBandeja = "6 meses";
    var qBandejaEntrada = getParameterByName("mode");
    // Se actualiza el texto de la leyenda q indica la cantidad de meses de la data de la bandeja de entrada
    if (qBandejaEntrada == "6" || qBandejaEntrada == "7" || qBandejaEntrada == "10") {
        mesesBandeja = "3 meses";
    }
    var textoHelpBuscar = $("#record_table_filter").html();
    if (textoHelpBuscar != undefined && textoHelpBuscar.indexOf("Importante:") == -1)
        $("#record_table_filter").append('<br><span class="">' +
            '<b>Importante:</b> en esta bandeja se despliegan los ingresos de los últimos  <span id="mesesBandeja1">' + mesesBandeja + '</span> asociados a tu perfil, si necesita buscar en la bandeja, utiliza el casillero <b>Buscar</b>. ' +
            'Si el ingreso es anterior a <span id="mesesBandeja2">' + mesesBandeja + '</span> debe utilizar <b>Búsqueda en Bandeja de Entrada</b>. Para registros en general, es decir no asociados a tu perfil debes utilizar <b>Búsqueda Avanzada</b>. ' +
            '</span>');
    //$("#record_table_length").css("width", "auto");
    // Fin Leyenda de Bandeja de entrada 

    // Se insertan títulos en la página q se crean directamente en sharepoint y n en la webpart
    try {
        var tituloPag = $('#tituloPagina');
        if (!tituloPag || tituloPag.length == 0) {
            // Página Administrador Despachos
            var formPag = $("form[action^='AdminDespachos.aspx']");
            if (formPag && formPag.length > 0) {
                var divTit = formPag.find('div.s4-ca');
                if (divTit && divTit.length > 0) {
                    divTit.prepend("<h4 id='tituloPagina'>Administrar Despachos</h4>");
                }
            }
            // Página Administrador Despachos Iniciativas CMN
            formPag = $("form[action^='AdminDespachoCMN.aspx']");
            if (formPag && formPag.length > 0) {
                var divTit = formPag.find('div.s4-ca');
                if (divTit && divTit.length > 0) {
                    divTit.prepend("<h4 id='tituloPagina'>Administrar Despachos Iniciativas CMN</h4>");
                }
            }
            // Página Administrador Bitácoras
            formPag = $("form[action^='AdminBitacora.aspx']");
            if (formPag && formPag.length > 0) {
                var divTit = formPag.find('div.s4-ca');
                if (divTit && divTit.length > 0) {
                    divTit.prepend("<h4 id='tituloPagina'>Administrar Bitácoras</h4>");
                }
            }
            // Página Agregar Caso
            formPag = $("form[action^='AllItems.aspx']");
            if (formPag && formPag.length > 0 && window.location.pathname == "/Lists/MCasos/AllItems.aspx") {
                var divTit = formPag.find('div.s4-ca');
                if (divTit && divTit.length > 0) {
                    divTit.prepend("<h4 id='tituloPagina'>Agregar Caso</h4>");
                }
            }
        }
    } catch (error) {
        console.error(error);
    }

});

function SetSoloLectura() {
    soloLectura = true;
}

function showAlert(mensaje, callback, titulo, checkMostrandoMsg) {
    if (mostrandoAlert && checkMostrandoMsg) // Evitar q se muestre dos mensajes a la vez
        return;
    mostrandoAlert = true;
    bootbox.alert({
        message: mensaje,
        title: titulo,
        buttons: {
            ok: {
                label: "Aceptar",
                className: "btn-primary",
                callback: function () {
                    mostrandoAlert = false;
                }
            }
        },
        callback: callback,
        closeButton: false
    });
}

function showPrompt(mensaje, callback, confirmClass, cancelClass) {
    bootbox.prompt({
        title: mensaje,
        buttons: {
            confirm: {
                label: "Grabar",
                className: (confirmClass ? confirmClass : "btn-primary")
            },
            cancel: {
                label: "Cancelar",
                className: (cancelClass ? cancelClass : "btn-danger")
            },
        },
        callback: callback,
        closeButton: false

    });
}

function showConfirm(mensaje, callback, confirmClass, cancelClass) {

    bootbox.confirm({
        message: mensaje,
        swapButtonOrder: true,
        buttons: {
            confirm: {
                label: 'Sí',
                className: "btn-confirm " + (confirmClass ? confirmClass : "btn-primary")
            },
            cancel: {
                label: 'No',
                className: "btn-confirm " + (cancelClass ? cancelClass : "btn-danger")
            }
        },
        callback: callback,
        closeButton: false
    });

}

function ubicacionMsgError(error, element) {
    if (element.attr("name") == undefined) {
        error.insertAfter(element);
        return true;
    }
    if (element.attr("name").indexOf("$txtFechaAdjunto") != -1) {
        error.appendTo("#divErrorFechaAdj");
    } else if (element.attr("name").indexOf("$dtcFechaRecepUtDate") != -1) {
        error.appendTo("#divErrorTextFechaRecepc");
    } else if (element.attr("name").indexOf("$dtcFechaIngresoDate") != -1) {
        error.appendTo("#divErrorTextFechaIngreso");
    } else if (element.attr("name").indexOf("$dtcFechaDocumentoDate") != -1) {
        error.appendTo("#divErrorTextFechaDoc");
    } else if (element.attr("name").indexOf("$dtcFechaIngresoDate") != -1) {
        error.appendTo("#divErrorTextFechaIng");
    } else if (element.attr("name").indexOf("dtcFechaEmisionOf") != -1) {
        error.appendTo("#divErrorTextFechaEmi");
    } else if (element.attr("name").indexOf("dtcFechaRecepcion") != -1) {
        error.appendTo("#divErrorTextFechaRec");
    } else if (element.attr("name").indexOf("dtcFechaDesde") != -1 ||
        element.attr("name").indexOf("txtFechaDesde") != -1) {
        error.appendTo("#divErrorTextFechaDesde");
    } else if (element.attr("name").indexOf("dtcFechaHasta") != -1 ||
        element.attr("name").indexOf("txtFechaHasta") != -1) {
        error.appendTo("#divErrorTextFechaHasta");
    } else if (element.parent().hasClass('magicsearch-wrapper') ||
        element.attr("name").indexOf("tbFecha") != -1) {
        error.insertAfter(element.parent());
    }
    else {
        error.insertAfter(element);
    }
}

// Búsqueda de Caso
function seteaSearchCaso(control, url, id, format, fields, success, afterDelete) {
    seteaSearchSimple(control, url, id, format, fields,
        function ($input, data) {
            /*var valorJsonStr = "{Id : " + data.Id + "}";
            callWebMethod("?Opcion=Caso", "GET", valorJsonStr, muestraDatosCaso);*/
            muestraDatosCaso(data);
            return true;
        },
        afterDeleteSearchCaso);
}

function limpiaDatosCaso() {
    $("input[id$='tbRemitenteEdit']").val('');
    $("input[id$='tbCargo']").val('');
    $("input[id$='tbInstitucion']").val('');
}


function muestraDatosCaso(data) {
    /*$("input[id$='tbMCaso']").text(data.Nombre);
    $("input[id$='tbMCaso']").val(data.Nombre);*/
    $("input[id$='hfIdCaso']").val(data.Id);
    $("span[id$='_lbNCaso']").text(data.Id);
    $("span[id$='_lbFechaCaso']").text(data.FechaReferencia);
}

// Funciones para busqueda y creación de Remitente    
function seteaSearchRemitente(control, url, id, format, fields, success, afterDelete) {
    seteaSearchSimple(control, url, id, format, fields,
        function ($input, data) {
            var valorJsonStr = "{Id : " + data.Id + "}";
            callWebMethod("?Opcion=Remitente", "GET", valorJsonStr, muestraDatosRemitente);
            return true;
        });
}

function grabaRemitente() {
    var nombre = $("input[id$='tbRemitenteEdit']").val();
    var genero = $("input[id$='rbGeneroFem']:checked").length > 0 ? 'Femenino' :
        $("input[id$='rbGeneroMasc']:checked").length > 0 ? 'Masculino' :
            $("input[id$='rbGeneroNeutro']:checked").length > 0 ? 'Neutro' : "";
    var email = $("input[id$='tbEmail']").val();
    var rut = $("input[id$=tbRemitenteRut]").val();

    if (nombre.trim() === "" || genero === "") {
        var texto = "Datos incompletos. Por favor, especifique " +
            (nombre.trim() === "" ? " el nombre " : "") +
            (nombre.trim() === "" && genero === "" ? "y" : "") +
            (genero === "" ? " el género " : "") +
            " del remitente";
        showAlert(texto);
        return false;
    }
    if (email != "" && !isEmail(email)) {
        showAlert("Por favor, especifique un email válido.");
        return false;
    }
    if (rut && !$.validateRut(rut)) {
        showAlert("Por favor, especifique un RUT válido.");
        return false;
    }
    var tipoInst = $("input[id$='rbInstPublica']:checked").length > 0 ? 'Pública' :
        $("input[id$='rbInstPrivada']:checked").length > 0 ? 'Privada' :
            $("input[id$='rbInstInterna']:checked").length > 0 ? 'Interna' : "";
    var obj = {
        Id: 0,
        Nombre: $("input[id$='tbRemitenteEdit']").val(),
        Cargo: $("input[id$='tbCargo']").val(),
        Institucion: $("input[id$='tbInstitucion']").val(),
        Tipo_Institucion: tipoInst,
        Direccion: $("input[id$='tbDireccion']").val(),
        Email: $("input[id$='tbEmail']").val(),
        Telefono: $("input[id$='tbTelefono']").val(),
        Genero: genero,
        Rut: rut
    };

    var valorJsonStr = JSON.stringify(obj);

    $("#waitGrabarNuevoRemit").show();
    $("#btnGrabarRemit").prop("disabled", true);
    $("#btnGCancelarRemit").prop("disabled", true);

    callWebMethod("?Opcion=Remitente", "POST", valorJsonStr, function (data) {
        $("#waitGrabarNuevoRemit").hide();
        $("#btnGrabarRemit").prop("disabled", false);
        $("#btnGCancelarRemit").prop("disabled", false);
        if (data.Id > 0) {
            // Se grabaron bien los datos
            $("#modalNuevoRemitente").modal("hide");
            obj.Id = data.Id;
            muestraDatosRemitente(obj);
            var siteUrl = GetSiteUrl();
            var url = siteUrl + "/_layouts/dibam.cmn/AjaxHandler.ashx";
            $('.search-remitente').trigger('destroy');
            seteaSearchRemitente('.search-remitente', url + "?Opcion=Lista&Tipo=Remitentes", 'Id', '%Nombre%', ['Nombre']);
            $(".search-remitente").attr("data-id", data.Id);
            // Ocultar mensaje de Campo Obligatorio en el search de Remitente
            $('.search-remitente').parent().next().hide();
            $('.search-remitente').removeClass("error");
        } else {
            var textoError = (data != null && data.Desc) ? data.Desc : "Ha ocurrido un error al grabar los datos. Consulte el log de errores.";
            alert(textoError);
            // showAlert(textoError);
        }


    });

}

function limpiaRemitente() {
    $("input[id$='tbRemitenteEdit']").val('');
    $("input[id$='rbGeneroFem']").prop('checked', false);
    $("input[id$='rbGeneroMasc']").prop('checked', false);
    $("input[id$='rbGeneroNeutro']").prop('checked', false);
    $("input[id$='tbCargo']").val('');
    $("input[id$='tbInstitucion']").val('');
    $("input[id$='rbInstPublica']").prop('checked', false);
    $("input[id$='rbInstPrivada']").prop('checked', false);
    $("input[id$='rbInstInterna']").prop('checked', false);
    $("input[id$='tbDireccion']").val('');
    $("input[id$='tbEmail']").val('');
    $("input[id$='tbTelefono']").val('');
    $("input[id$='tbRemitenteRut']").val('');
}


function muestraDatosRemitente(data) {
    $("input[id$='tbRemitente']").text(data.Nombre);
    $("input[id$='tbRemitente']").val(data.Nombre);
    $("input[id$='_hfIdRemitente']").val(data.Id);
    $("span[id$='_lbRemitenteNombre']").text(data.Nombre);
    $("span[id$='_lbRemitenteCargo']").text(data.Cargo);
    $("span[id$='_lbRemitenteInst']").text(data.Institucion);
    $("span[id$='_lbRemitenteTipoInst']").text(data.Tipo_Institucion);
    $("span[id$='_lbRemitenteEmail']").text(data.Email);
    $("span[id$='_lbRemitenteTelef']").text(data.Telefono);
    $("span[id$='_lbRemitenteGenero']").text(data.Genero);
    $("span[id$='_lbRemitenteDir']").text(data.Direccion);
    $("span[id$='_lbRemitenteRut']").text(data.Rut);

}

function isEmail(email) {
    //var emailexp = new RegExp(/\b[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,4}\b/i);
    var emailexp = new RegExp(/^([A-Za-z0-9_\-\.])+\@([A-Za-z0-9_\-\.])+\.([A-Za-z]{2,4})$/);

    return emailexp.test(email);
}

function isValidNumRequerimiento(valor, minAnno, maxAnno) {
    minAnno = minAnno == undefined ? 0 : minAnno;
    maxAnno = maxAnno == undefined ? 9999 : maxAnno;
    // Se valida q el formato sea, por ejemplo, 00001-2018
    var numReqRegExp = new RegExp('\\b(\\d{1,5}-\\d{4})$');
    if (numReqRegExp.test(valor)) {
        var anno = valor.substring(valor.indexOf("-") + 1, valor.length);
        return parseInt(anno) <= maxAnno && parseInt(anno) >= minAnno;
    }
    else {
        return false;
    }
}

function isValidNumOficio(valor, minAnno, maxAnno) {
    minAnno = minAnno == undefined ? 0 : minAnno;
    maxAnno = maxAnno == undefined ? 9999 : maxAnno;
    // Se valida q el formato sea, por ejemplo, 00001-2018
    var numReqRegExp = new RegExp('\\b(\\d{1,5}-\\d{4})$');
    if (numReqRegExp.test(valor)) {
        var anno = valor.substring(valor.indexOf("-") + 1, valor.length);
        return parseInt(anno) <= maxAnno && parseInt(anno) >= minAnno;
    }
    else {
        return false;
    }
}

function showDialogoEspera(title, text) {
    window.parent.eval("window.waitDialog = SP.UI.ModalDialog.showWaitScreenWithNoClose('" + title + "', '" + text + "');");
}


function fechaDocumentoChange() {
    try {
        var controlFecha = $("input[id$='dtcFechaDocumento_dtcFechaDocumentoDate']");
        var chkSoloMesAnno = $("input[id$='chbSoloMesAnno']");
        //TODO: q los formatos de fecha se le pasen dinamicamente de acuerdo a la configuración local del servidor:
        var fechaDocStr = controlFecha.val();
        if (chkSoloMesAnno.is(":checked")) {
            // Formatear a sólo Mes y Año
            var estaEnFormatoFull = moment(fechaDocStr, formatoFecha, true).isValid();
            if (estaEnFormatoFull) {
                fechaDocFull = fechaDocStr;
                var fechaDocMYStr = moment(fechaDocStr, formatoFecha).format(formatoFechaMY);
                controlFecha.val(fechaDocMYStr);
            }
        } else {
            // Formato completo
            var estaEnFormatoMY = moment(fechaDocStr, formatoFechaMY, true).isValid();
            if (estaEnFormatoMY) {
                var diaFechaFull = moment(fechaDocFull, formatoFecha).date();
                if (!diaFechaFull) {
                    diaFechaFull = 1;
                }
                var fechaDocFullStr = moment(fechaDocStr, formatoFechaMY).date(diaFechaFull).format(formatoFecha);
                controlFecha.val(fechaDocFullStr);
            }
        }
    }
    catch (err) { }
}

// Metodos de Region, Provincia y Comuna
function successRegion($input, data) {
    asignaHiddenField($input, data);
    ConfigSearchProvincia(true);
    ConfigSearchComuna(true);
    return true;
}

function successProvincia($input, data) {
    asignaHiddenField($input, data);
    // provinciasRegiones.push(data);
    comunasRegiones.push(data);

    var idsRegion = $(".search-region").attr("data-id") || "-1";

    if (idsRegion == "-1") {
        asignaRegion(data.IdRegion);
        ConfigSearchRegion(true);
    }

    ConfigSearchComuna(true);
    return true;
}

function successComuna($input, data) {
    asignaHiddenField($input, data);
    comunasRegiones.push(data);

    var idsProvincia = $(".search-provincia").attr("data-id") || "-1";
    var idsRegion = $(".search-region").attr("data-id") || "-1";

    if (idsRegion == "-1") {
        asignaRegion(data.IdRegion);
        ConfigSearchRegion(true);
    }

    if (idsProvincia == "-1") {
        asignaProvincia(data.IdProvincia);
        ConfigSearchProvincia(true);
    }

    return true;
}

function asignaRegion(idRegion) {
    // Asigna la region especificada luego de seleccionar una provincia o comuna
    var idsRegion = ($("input[id$='hfRegion']").val() || "");
    if ((',' + idsRegion + ',').indexOf(',' + idRegion + ',') < 0) {
        // Si la región no está seleccionada entonces se agrega a las ya seleccionadas
        var valor = idsRegion + (idsRegion ? ',' : '') + idRegion;
        $("input[id$='hfRegion']").val(valor);
        $(".search-region").attr("data-id", valor);
    }
}

function asignaProvincia(idProvincia) {
    // Asigna la provincia especificada luego de seleccionar una comuna
    var idsProvincia = ($("input[id$='hfProvincia']").val() || "");
    if ((',' + idsProvincia + ',').indexOf(',' + idProvincia + ',') < 0) {
        // Si la provincia no está seleccionada entonces se agrega a las ya seleccionadas
        var valor = idsProvincia + (idsProvincia ? ',' : '') + idProvincia;
        $("input[id$='hfProvincia']").val(valor);
        $(".search-provincia").attr("data-id", valor);
    }
}

function afterDeleteRegion($input, data) {
    asignaHiddenField($input, data);
    limpiaComunas(data.Id);
    limpiaProvincias(data.Id);
    ConfigSearchComuna(true);
    ConfigSearchProvincia(true);
    return true;
}

function afterDeleteProvincia($input, data) {
    asignaHiddenField($input, data);
    limpiaComunas(data.Id);
    ConfigSearchComuna(true);
    return true;
}

function afterDeleteComuna($input, data) {
    asignaHiddenField($input, data);
    /*limpiaProvincias(data.Id);
    ConfigSearchRegion(true);
    ConfigSearchProvincia(true);*/
    return true;
}

function limpiaRegion(idRegion) {
    var idsRegion = $("input[id$='hfRegion']").val() ? $("input[id$='hfRegion']").val() : "";
    var arrayRegiones = idsRegion.split(',');
    for (var i = 0; i < comunasRegiones.length; i++) {
        if (comunasRegiones[i].IdRegion == idRegion &&
            arrayRegiones.indexOf(comunasRegiones[i].Id) >= 0) {
            arrayRegiones.splice(arrayRegiones.indexOf(comunasRegiones[i].Id), 1);
        }
    }
    idsRegion = arrayRegiones.join();
    $("input[id$='hfRegion']").val(idsRegion);
}

function limpiaProvinciasOLD(idRegion) {
    var idsProvincia = $("input[id$='hfProvincia']").val() ? $("input[id$='hfProvincia']").val() : "";
    var arrayProv = idsProvincia.split(',');
    for (var i = 0; i < comunasRegiones.length; i++) {
        if (comunasRegiones[i].IdRegion == idRegion &&
            arrayProv.indexOf(comunasRegiones[i].Id) >= 0) {
            arrayProv.splice(arrayProv.indexOf(comunasRegiones[i].Id), 1);
        }
    }
    idsProvincia = arrayProv.join();
    $("input[id$='hfProvincia']").val(idsProvincia);
}

function limpiaProvincias() {
    // Se eliminan las provincias seleccionadas q no pertenezcan a la región seleccionada(s)
    var idsRegion = $(".search-region").attr("data-id") || "-1";
    var idsProvincia = $("input[id$='hfProvincia']").val() ? $("input[id$='hfProvincia']").val() : "";

    if ((idsRegion === "" || idsRegion === "-1")) {
        return;
    }

    var arrRegiones = idsRegion.split(',');
    var arrayProv = idsProvincia.split(',');
    for (var i = 0; i < comunasRegiones.length; i++) {
        if (arrRegiones.indexOf(comunasRegiones[i].IdRegion) < 0 &&
            arrayProv.indexOf(comunasRegiones[i].IdProvincia) >= 0) {
            arrayProv.splice(arrayProv.indexOf(comunasRegiones[i].IdProvincia), 1);
        }
    }
    idsProvincia = arrayProv.join();
    $("input[id$='hfProvincia']").val(idsProvincia);
}

function limpiaComunasOLD(idRegion) {
    var idsComuna = $("input[id$='hfComuna']").val() ? $("input[id$='hfComuna']").val() : "";
    var arrayComunas = idsComuna.split(',');
    for (var i = 0; i < comunasRegiones.length; i++) {
        if (comunasRegiones[i].IdRegion == idRegion &&
            arrayComunas.indexOf(comunasRegiones[i].Id) >= 0) {
            arrayComunas.splice(arrayComunas.indexOf(comunasRegiones[i].Id), 1);
        }
    }
    idsComuna = arrayComunas.join();
    $("input[id$='hfComuna']").val(idsComuna);
}

function limpiaComunas() {
    // Se eliminan las comunas seleccionadas q no pertenezcan a la región y/o provincias seleccionadas
    var idsRegion = $(".search-region").attr("data-id") || "-1";
    var idsProvincia = $(".search-provincia").attr("data-id") || "-1";

    if ((idsRegion === "" || idsRegion === "-1") &&
        (idsProvincia === "" || idsProvincia === "-1")) {
        return;
    }

    var arrRegiones = idsRegion.split(',');
    var arrProvincias = idsProvincia.split(',');
    var idsComuna = $("input[id$='hfComuna']").val() ? $("input[id$='hfComuna']").val() : "";
    var arrayComunas = idsComuna.split(',');
    for (var i = 0; i < comunasRegiones.length; i++) {
        if (arrRegiones.indexOf(comunasRegiones[i].IdRegion) < 0 &&
            arrProvincias.indexOf(comunasRegiones[i].IdProvincia) < 0 &&
            arrayComunas.indexOf(comunasRegiones[i].IdComuna) >= 0) {  /*****cambiar IdComuna por Id si se ocupa el Id en el search como campo de id **************/
            arrayComunas.splice(arrayComunas.indexOf(comunasRegiones[i].IdComuna), 1);
        }
    }
    idsComuna = arrayComunas.join();
    $("input[id$='hfComuna']").val(idsComuna); // Se guarda el valor filtrado de comunas en el hfComunas para luego usarlo
}

function ConfigSearchComunaOLD(inicializar) {
    if (!inicializar) {
        var valor = $(".search-comuna").attr("data-comunas");
        if (valor && valor != "")
            comunasRegiones = stringToJson(valor);
    }
    var idRegion = $(".search-region").attr("data-id") || "-1";
    var idProvincia = $(".search-provincia").attr("data-id") || "-1";

    $(".search-comuna").trigger('destroy');
    if ((idRegion === "" || idRegion === "-1") &&
        (idProvincia === "" || idProvincia === "-1")) {
        $("input[id$='hfComuna']").val("");
        $(".search-comuna").attr("data-id", "");
    } else {
        /*if (inicializar)
        {
          $("input[id$='hfComuna']").val("");
        }*/
        var valor = /*!inicializar &&*/ $("input[id$='hfComuna']").val() ? $("input[id$='hfComuna']").val() : "";
        $(".search-comuna").attr("data-id", valor);

        var siteUrl = GetSiteUrl();
        var url = siteUrl + "/_layouts/dibam.cmn/AjaxHandler.ashx";
        // Search de Comuna
        seteaSearchMulti('.search-comuna',
            url + "?Opcion=Lista&Tipo=Comuna&RegionId=" + idRegion + "&ProvinciaId=" + idProvincia,
            'Id',
            '%Titulo% - %TituloProvincia% - %TituloRegion%',
            ['Titulo'],
            successComuna, false, false, afterDeleteComuna);
    }
}

function ConfigSearchComuna(inicializar) {
    /*if (!inicializar) {
        var valor = $(".search-comuna").attr("data-comunas");
        if (valor && valor != "")
            comunasRegiones = stringToJson(valor);
    }*/
    var idRegion = $(".search-region").attr("data-id") || "-1";
    var idProvincia = $(".search-provincia").attr("data-id") || "-1";
    var idComuna = $(".search-comuna").attr("data-id") || "-1";
    // Se dejan sólo las regiones q no pertenecen a las provincias seleccionadas. Por ejemplo, se tiene
    // seleccionada las regiones Metropolitana y Valparaiso, y sólo la provincia Santiago entonces se 
    // deja solamente la región Valparaiso pues al filtrar por provincia Santiago no se puede incluir la región
    // Metropolitana en el filtro
    var arrRegiones = idRegion.split(',');
    var arrProvincias = idProvincia.split(',');
    for (var i = 0; i < comunasRegiones.length; i++) {
        if (arrProvincias.indexOf(comunasRegiones[i].IdProvincia) >= 0 &&
            arrRegiones.indexOf(comunasRegiones[i].IdRegion) >= 0) {
            arrRegiones.splice(arrRegiones.indexOf(comunasRegiones[i].IdRegion), 1);
        }
    }
    idRegion = arrRegiones.join();

    if (inicializar) {
        // No se hace nada
    } else {
        // Entra aquí si se cambió la región o provincia y se necesita q el search de comuna muestre sólo
        // las correspondientes a la región y provincia seleccionadas.
        // Las comunas q están seleccionadas y q no correspondan a la provincia o región seleccionadas ya fueron eliminadas en una llamada a limpiaComunas

    }

    $(".search-comuna").trigger('destroy');
    var valor = /*!inicializar &&*/ $("input[id$='hfComuna']").val() ? $("input[id$='hfComuna']").val() : "";
    $(".search-comuna").attr("data-id", valor);

    var siteUrl = GetSiteUrl();
    var url = siteUrl + "/_layouts/dibam.cmn/AjaxHandler.ashx";
    // Search de Comuna
    seteaSearchMulti('.search-comuna',
        url + "?Opcion=Lista&Tipo=Comuna&RegionId=" + idRegion + "&ProvinciaId=" + idProvincia,
        'IdComuna',
        '%Titulo% - %TituloProvincia% - %TituloRegion%',
        ['Titulo'],
        successComuna, false, false, afterDeleteComuna);


}

function ConfigSearchProvinciaOLD(inicializar) {
    if (!inicializar) {
        var valor = $(".search-provincia").attr("data-provincias");
        if (valor && valor != "")
            comunasRegiones = stringToJson(valor);
    }
    var idRegion = $(".search-region").attr("data-id") || "-1";
    var idComuna = $(".search-comuna").attr("data-id") || "-1";

    $(".search-provincia").trigger('destroy');
    if ((idRegion === "" || idRegion === "-1") &&
        (idComuna === "" || idComuna === "-1")) {
        $("input[id$='hfProvincia']").val("");
        $(".search-provincia").attr("data-id", "");
    } else {
        /*if (inicializar)
        {
          $("input[id$='hfProvincia']").val("");
        }*/
        var valor = /*!inicializar &&*/ $("input[id$='hfProvincia']").val() ? $("input[id$='hfProvincia']").val() : "";
        $(".search-provincia").attr("data-id", valor);

        var siteUrl = GetSiteUrl();
        var url = siteUrl + "/_layouts/dibam.cmn/AjaxHandler.ashx";
        // Search de Provincia
        seteaSearchMulti('.search-provincia',
            url + "?Opcion=Lista&Tipo=Provincia&RegionId=" + idRegion + "&ComunaId=" + idComuna,
            'IdProvincia',
            '%TituloProvincia% - %TituloRegion%',
            ['TituloProvincia'],
            successProvincia, false, false, afterDeleteProvincia);

    }
}

function ConfigSearchProvincia(inicializar) {
    /*if (!inicializar) {
        var valor = $(".search-provincia").attr("data-provincias");
        if (valor && valor != "")
            comunasRegiones = stringToJson(valor);
    }*/
    var idRegion = $(".search-region").attr("data-id") || "-1";
    var idProvincia = $(".search-provincia").attr("data-id") || "-1";
    var idComuna = $(".search-comuna").attr("data-id") || "-1";

    $(".search-provincia").trigger('destroy');
    var valor = /*!inicializar &&*/ $("input[id$='hfProvincia']").val() ? $("input[id$='hfProvincia']").val() : "";
    $(".search-provincia").attr("data-id", valor);

    var siteUrl = GetSiteUrl();
    var url = siteUrl + "/_layouts/dibam.cmn/AjaxHandler.ashx";
    // Search de Provincia
    seteaSearchMulti('.search-provincia',
        url + "?Opcion=Lista&Tipo=Provincia&RegionId=" + idRegion + "&ComunaId=" + idComuna,
        'IdProvincia',
        '%TituloProvincia% - %TituloRegion%',
        ['TituloProvincia'],
        successProvincia, false, false, afterDeleteProvincia);
}

function ConfigSearchRegionOLD(inicializar) {
    // if (!inicializar) {
    // var valor = $(".search-region").attr("data-regiones");
    // if (valor && valor != "")
    // comunasRegiones = stringToJson(valor);
    // }
    var idComuna = $(".search-comuna").attr("data-id") || "-1";
    var idProvincia = $(".search-provincia").attr("data-id") || "-1";

    $(".search-region").trigger('destroy');
    if ((idComuna === "" || idComuna === "-1") &&
        (idProvincia === "" || idProvincia === "-1")) {
        $("input[id$='hfRegion']").val("");
        $(".search-region").attr("data-id", "");
    } else {
        /*if (inicializar)
        {
          $("input[id$='hfRegion']").val("");
        }*/
        var valor = /*!inicializar &&*/ $("input[id$='hfRegion']").val() ? $("input[id$='hfRegion']").val() : "";
        $(".search-region").attr("data-id", valor);

        var siteUrl = GetSiteUrl();
        var url = siteUrl + "/_layouts/dibam.cmn/AjaxHandler.ashx";
        // Search de Region
        seteaSearchMulti('.search-region',
            url + "?Opcion=Lista&Tipo=Region&ProvinciaId=" + idProvincia + "&ComunaId=" + idComuna,
            'Id',
            '%Titulo%',
            ['Titulo'],
            successRegion);
    }
}

function ConfigSearchRegion(inicializar) {
    // if (!inicializar) {
    // var valor = $(".search-region").attr("data-regiones");
    // if (valor && valor != "")
    // comunasRegiones = stringToJson(valor);
    // }
    var idRegion = $(".search-region").attr("data-id") || "-1";
    var idProvincia = $(".search-provincia").attr("data-id") || "-1";
    var idComuna = $(".search-comuna").attr("data-id") || "-1";

    $(".search-region").trigger('destroy');
    var valor = /*!inicializar &&*/ $("input[id$='hfRegion']").val() ? $("input[id$='hfRegion']").val() : "";
    $(".search-region").attr("data-id", valor);

    var siteUrl = GetSiteUrl();
    var url = siteUrl + "/_layouts/dibam.cmn/AjaxHandler.ashx";
    // Search de Region
    seteaSearchMulti('.search-region',
        url + "?Opcion=Lista&Tipo=Region&ProvinciaId=" + idProvincia + "&ComunaId=" + idComuna,
        'Id',
        '%Titulo%',
        ['Titulo'],
        successRegion, false, false, afterDeleteRegion);
}
// Fin Metodos de Region, Provincia y Comuna

function stringToJson(dataStr) {
    if (!dataStr || dataStr == "") {
        return {};
    }
    var json = jQuery.parseJSON(dataStr); // JSON.parse(dataStr); // JSON.stringify(eval("(" + dataStr+ ")"));
    return json;
}

function asignaLeyendas() {
    var leyenda = "<span class='help-block'> %LEYENDA%</span>";
    // Leyenda de Región
    var texto = "Puede seleccionar 1 o 2 regiones. Si atañe a más de 2 regiones seleccione “Multiregional”. Si compete a todo el país seleccione “No aplica”.";
    $("input[id$='tbRegion']").parent().append(leyenda.replace("%LEYENDA%", texto));

    // Nombre de remitente
    texto = "Nombres y apellidos: Juan Diego Pérez Dominguez.";
    $("input[id$='tbRemitenteEdit']").parent().append(leyenda.replace("%LEYENDA%", texto));

    // Cargo de remitente
    texto = "Encargado Unidad de Planimetria. Dibujante Técnico";
    $("input[id$='tbCargo']").parent().append(leyenda.replace("%LEYENDA%", texto));

    // Institución de remitente
    texto = "Pérez Dominguez Arquitectos";
    $("input[id$='tbInstitucion']").parent().append(leyenda.replace("%LEYENDA%", texto));

    // Dirección de remitente
    texto = "Avenida del Mar N° 84, Viña del Mar, Valparaíso, Chile.";
    $("input[id$='tbDireccion']").parent().append(leyenda.replace("%LEYENDA%", texto));

    // Email de remitente
    texto = "Ej.: micorreo@empresa.cl; micorreo@empresa.com; micorreo@empresa.net";
    $("input[id$='tbEmail']").parent().append(leyenda.replace("%LEYENDA%", texto));

    // Remitente
    texto = "Nombres y apellidos: Juan Diego Pérez Dominguez.";
    $("input[id$='tbRemitente']").parent().append(leyenda.replace("%LEYENDA%", texto));

    // Destinatario
    texto = "Nombres y apellidos: Juan Diego Pérez Dominguez.";
    $("input[id$='tbDestinatario']").parent().append(leyenda.replace("%LEYENDA%", texto));

    // Fecha de Ingreso Histórico
    //texto = "La fecha tiene que ser en formato dd/mm/aaaa.";
    //$("input[id$='dtcFechaIngresoDate']").closest("div").append(leyenda.replace("%LEYENDA%", texto));


}

function getOriginalIdAspControl(control) {
    var id = $(control).attr("Id");
    var last_ = id.lastIndexOf("_") + 1;
    id = id.substring(last_, id.length);
    return id;
}

function recargarPagina() {
    location.reload(true);
}


function showModalRemitente() {
    $("#modalNuevoRemitente").modal();
}


function openDialog(tit, url, callback, width, height, showMaximized) {
    options = SP.UI.$create_DialogOptions();
    options.title = tit;
    options.url = url
    if (width)
        options.width = width;
    if (height)
        options.height = height;
    if (showMaximized)
        options.showMaximized = showMaximized;
    if (callback)
        options.dialogReturnValueCallback = Function.createDelegate(null, callback);
    SP.UI.ModalDialog.showModalDialog(options);

}

function getFechaHoyStr(formato) {
    if (!formato) {
        formato = formatoFecha
    }
    return moment().format(formato);
}

function archivoValido(tamannoPermitidoBytes, extPermitidas, controlUpl) {
    if (controlUpl == undefined || controlUpl.length == 0 || controlUpl.val() == '')
        return true;

    var tamArch = controlUpl[0].files[0].size;
    var extArch = controlUpl.val().replace(/^.*\./, '').toLowerCase();
    var arrExt = extPermitidas.toLowerCase().split(';');

    return tamArch <= tamannoPermitidoBytes && arrExt.indexOf(extArch) != -1;
}

function fechaToStr(fecha, formato) {
    if (fecha == null || !moment(fecha).isValid() || moment(fecha).year() <= 50)
        return '';
    if (!formato)
        formato = formatoFechaGrilla;
    var fechaStr = moment(fecha).format(formato);
    return fechaStr;
}

function getParameterByName(name, url) {
    if (!url) url = window.location.href;
    name = name.replace(/[\[\]]/g, "\\$&");
    var regex = new RegExp("[?&]" + name + "(=([^&#]*)|&|#|$)"),
        results = regex.exec(url);
    if (!results) return null;
    if (!results[2]) return '';
    return decodeURIComponent(results[2].replace(/\+/g, " "));
}

function caracteresMateria(valor) {
    return getValidNumber(valor, 255, 1000);
}

function getValidNumber(valor, defaultValue, maxValue) {
    if (!valor) {
        valor = defaultValue;
    }
    if (valor != parseInt(valor, 10)) // valor no es numerico
        valor = defaultValue;
    else if (valor > maxValue) // valor es numerico mayor a maxValue
        valor = maxValue;
    return valor;
}

//function PlegarPanel(panel) {
//    /*if ($(panel).hasClass('in')) {
//       $(panel).removeClass('in');
//    }*/
//    $(panel).collapse('toggle');
//}

//jQuery.extend(jQuery.fn.dataTableExt.oSort, {
//    "date-uk-pre": function (a) {
//        if (a == null || a == "") {
//            return 0;
//        }
//        var ukDatea = a.split('/');
//        return (ukDatea[2] + ukDatea[1] + ukDatea[0]) * 1;
//    },

//    "date-uk-asc": function (a, b) {
//        return ((a < b) ? -1 : ((a > b) ? 1 : 0));
//    },

//    "date-uk-desc": function (a, b) {
//        return ((a < b) ? 1 : ((a > b) ? -1 : 0));
//    }
//});

function MuestraEspera(elemento, mensaje) {
    return; // TODO: está dando error LoadingOverlay o también blockUI indicando q no existen
    if (!mensaje || mensaje.length == 0)
        mensaje = 'Cargando datos, espere por favor...  ';
    mensaje += "<i class='fa fa-spinner fa-spin'></i>";
    var defaultOption = {
        text: mensaje,
        /*css: { border: '1px solid gray' },
        centerX: true,
        centerY: true,
        overlayCSS: {
            opacity: 0.3
        }*/
    };
    if (!elemento) {
        $.LoadingOverlay("show", defaultOption);
    } else {
        elemento.LoadingOverlay("show", defaultOption);
        //TODO: el block y unblock a veces da error de q no está definida para el elemento, por eso el chequeo
        /*if ($.isFunction(elemento.block))
            elemento.block(defaultOption);
        else if ($.isFunction(elemento.blockUI))
            elemento.blockUI(defaultOption);*/
    }
}

function OcultaEspera(elemento) {
    return; // TODO: está dando error LoadingOverlay o también blockUI indicando q no existen
    if (!elemento) {
        $.LoadingOverlay("hide");
    } else {
        elemento.LoadingOverlay("hide");
        /*if ($.isFunction(elemento.unblock))
            elemento.unblock();
        else if ($.isFunction(elemento.unblockUI))
            elemento.unblockUI();*/
    }
}

function blockPageUI(message) {

    var msg = message || "";

    $.blockUI({
        message: "<br><i class='x-icon-loading'></i>",
        //fadeIn: 500, 
        baseZ: 1002,
        css: {
            width: '90px',
            height: '75px',
            //top                    : '10px', 
            left: '45%',
            //right                  : '10px', 
            //border                 : 'none', 
            //padding                : '5px', 
            // backgroundColor        : '#000', 
            '-webkit-border-radius': '8px',
            '-moz-border-radius': '8px',
            '-ms-border-radius': '8px',
            '-o-border-radius': '8px',
            'border-radius': '8px',
            'box-shadow': '0 2px 6px rgba(0, 0, 0, 0.2)'
            //opacity                : .6, 
            //color                  : '#fff' 
        }
    });
}

function blockPageUIclose() {
    $.unblockUI({ fadeOut: 0 });
}

// Gestión de datos de monumento nacional
function setCategoriasBusq(categBusqStr) {
    if (!categBusqStr) {
        return [];
    } else {
        categoriasBusq = categBusqStr.split(',');
    }
}

function successCategoriaMon($input, data) {
    asignaHiddenField($input, data);
    cambiaEstadoBusqRegmon();
}

function afterDeleteCategoriaMon($input, data) {
    asignaHiddenField($input, data);
    cambiaEstadoBusqRegmon();
    return true;
}

// Habilita o deshabilita la búsqueda de Regmon
function cambiaEstadoBusqRegmon() {
    var catMonNac = $(".search-tipomon").attr("data-id") || "";
    var mostrarRegMon = true;

    if (catMonNac) {
        catMonNac.split(",").forEach(function (item, index) {
            // Si al menos 1 de las categorias seleccionas no es de búsqueda en regmon entonces se inhabilita la búsqueda
            if (categoriasBusq.indexOf(item) < 0) {
                mostrarRegMon = false;
            }
        });
    } else {
        mostrarRegMon = false;
    }

    if (mostrarRegMon) { // catMonNac == 'MH' || catMonNac == 'ZT' || catMonNac == 'SN') {
        // Busqueda desde Regmon
        $("#btnBuscarMn").show();

        $(".datos-mn").prop("readonly", true);
        $(".datos-mn").css("background-color", "#eee");
        $("input[class^='search-'].datos-mn").prop("disabled", true);

        $("#divMonNacional").removeClass("col-sm-9");
        $("#divMonNacional").addClass("col-sm-8");

        $("input[class^='search-'].datos-mn").addClass("search-ro");
    } else {
        // Se llenan los datos a mano, se deshabilita la Busqueda desde Regmon
        $("#btnBuscarMn").hide();

        $(".datos-mn").prop("readonly", false);
        $(".datos-mn").css("background-color", "white");
        $("input[class^='search-'].datos-mn").prop("disabled", false);

        $("#divMonNacional").removeClass("col-sm-8");
        $("#divMonNacional").addClass("col-sm-9");

        // Cuando se deshabilita la búsqueda desde Regmon hay q habilitar el botón de cerrar de los items en Región, Provincia y Comuna
        var closeCtrol = $("input[class^='search-'].datos-mn.search-ro").parent('div').find('.multi-item-close');
        if (closeCtrol) {
            closeCtrol.show();
        }

        $("input[class^='search-'].datos-mn").removeClass("search-ro");
    }
}

function inicializaBusquedaRegMon() {
    cambiaEstadoBusqRegmon();
}

// Función q abre la ventana de búsqueda de Regmon
function buscarMonumentoNac() {

    var options = {
        url: '/Paginas/Flujo_BuscadorMonumento.aspx',
        title: 'Buscador de Monumentos'
    };
    openDialog(options.title, options.url, CodigoMonumento);
    // var modal = "openDialog('" + options.title + "','" + options.url + "');"
    // eval(modal);
    return false;
}

// Funcion q se gatilla cuando se selecciona un momumento en Regmon
function CodigoMonumento(resultClose, id) {
    $("#btnBuscarMn").hide();
    $("#waitBuscarMn").show();
    if (id) {
        callWebMethod("?Opcion=DatosMnRegMon&monNac=" + id, "GET", "", actualizaDatosMonNac, errorDatosRegMon);
    } else {
        $("#btnBuscarMn").show();
        $("#waitBuscarMn").hide();
    }
}

// Se actualizan los datos del ingreso cuando se hace una búsqueda en Regmon
function actualizaDatosMonNac(datos) {
    $("#btnBuscarMn").show();
    $("#waitBuscarMn").hide();

    if (datos) {
        // En los datos q vienen de Regmon no está la categoría de monumento como está en Gedoc, tomo
        // la categoría del código de monumento q viene en datos
        var codCatMnRegmon = datos.CodigoMonNac;
        codCatMnRegmon = codCatMnRegmon.slice(codCatMnRegmon.indexOf('_') + 1);
        codCatMnRegmon = codCatMnRegmon.substring(0, codCatMnRegmon.indexOf('_'));
        var catMonNacGedoc = $(".search-tipomon").siblings('.multi-items');
        var mnOk = false;
        if (catMonNacGedoc) {
            for (var i = 0; i < catMonNacGedoc.length; i++) {
                var catGeDoc = catMonNacGedoc[i].innerText;
                if (codCatMnRegmon == catGeDoc) {
                    // Si el MN seleccionado en Regmon pertenece a alguna de las categorias seleccionasla búsqueda
                    mnOk = true;
                    break;
                }
            }
        }
        if (!mnOk) {
            showAlert('Atención. El Monumento Nacional seleccionado en Regmon no corresponde a la categoría especificada en Gedoc.');
        }

        $("input[id$='tbCodMonNac']").val(datos.CodigoMonNac);
        $("input[id$='tbMonNacional']").val(datos.DenominacionOficial);
        $("input[id$='tbOtrasDenom']").val(datos.OtrasDenominaciones);
        $("input[id$='tbNombreActual']").val(datos.NombreUsoActual);
        $("input[id$='tbDirMonNacional']").val(datos.DireccionMonNac);
        $("input[id$='tbRefLocalidad']").val(datos.ReferenciaLocalidad);
        $("input[id$='tbRolMN']").val(datos.Rol);
        $(".search-region").attr("data-id", datos.RegionId);
        // $(".search-region").trigger('set', { id: datos.RegionId, override: false });
        $(".search-provincia").attr("data-id", datos.ProvinciaId);
        // $(".search-provincia").trigger('set', { id: datos.ProvinciaId });
        $(".search-comuna").attr("data-id", datos.ComunaId);
        // $(".search-comuna").trigger('set', { id: datos.ComunaId });
        $("input[id$='hfRegion']").val(datos.RegionId);
        $("input[id$='hfProvincia']").val(datos.ProvinciaId);
        $("input[id$='hfComuna']").val(datos.ComunaId);

        ConfigSearchRegion();
        ConfigSearchProvincia();
        ConfigSearchComuna();
        // Después de configurar los search de comuna, provincia y región se habilitan por lo q tengo q deshabilitarlas nuevamente
        $("input[class^='search-'].datos-mn").prop("disabled", true);
    }
}

function errorDatosRegMon() {
    $("#btnBuscarMn").show();
    $("#waitBuscarMn").hide();
    showAlert("Ocurrió un error al obtener los datos del Monumento, por favor vuelva a intentar la operación.");
}

/* Abre la URL de la solicitud del sistema de trámites asociada al ingreso*/
function abreUrlTramite(urlAuth, user, secretKey, urlSolicitud) {

    if (!urlAuth) {
        showAlert('El ingreso no está asociado a una solicitud en el Sistema de Trámites.');
        return false;
    }
    $("#btTramites").hide();
    $("#waitLinkTramites").show();

    var data = { "UserName": user, "SecretKey": secretKey };

    // PRUEBAS:
    /*urlAuth = "https://reqres.in/api/login";
	data = {
			"email": "eve.holt@reqres.in",
			"password": "cityslicka" };
	data = { "email": "peter@klaven" };  // Esta data genera un error 400 indicando login fail */

    $.ajax({
        type: "POST",
        url: urlAuth,
        data: data,
        dataType: 'text'
    }).done(function (token) {
        $("#btTramites").show();
        $("#waitLinkTramites").hide();
        if (token) {
            urlSolicitud = urlSolicitud.replace('%token%', token);
            window.open(urlSolicitud, '_blank');
        } else {
            showAlert('No se recibió respuesta del sistema de trámites, por favor, reintente la operación.');
        }
    }).fail(function (xhr, result, status) {
        $("#btTramites").show();
        $("#waitLinkTramites").hide();
        if (xhr.status == 0) {
            showAlert('No fue posible contactar la URL del sistema de trámite.');
        } else if (xhr.status == 404) {
            showAlert('No es posible abrir la página. Error 404 al intentar acceder a la URL del sistema de trámites.');
        } else if (xhr.status == 500) {
            showAlert('No es posible abrir la página. Error 500 al intentar acceder a la URL del sistema de trámites.');
        } else {
            showAlert('No es posible abrir la página. Error desconocido:\n' + xhr.responseText);
        }
    });
}

// Fin