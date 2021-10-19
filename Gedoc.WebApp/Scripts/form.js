

var Form = Form ||
{
    inicializaForm: function() {

        Form.inicializaEventos();
        Form.configAreaEscritura();
    },

    inicializaEventos: function() {
        // Para check de Considerar sólo Mes y Año en fecha de documento
        $("#soloMesAnno").change(function() {
            Form.soloMesAnnoChange();
        });

        // dar formato a nuevos números de Despachos y Despachos Iniciativa
        $("input[data-formatearDesp='true']").blur(function () {
            // Ej formato correcto: 00001-2018
            var valor = $(this).val();
            if (valor && Form.isValidNumOficio(valor)) {
                // sólo se da formato al número si es válido el formato
                var numero = valor.substring(0, valor.indexOf("-"));
                if (numero.length < 5) {
                    var ceros = Array(5 - numero.length + 1).join("0");
                    valor = ceros + valor;
                    $(this).val(valor);
                }
            }
        });

    },

    isValidNumRequerimiento: function(valor, minAnno, maxAnno) {
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
    },

    isValidNumOficio: function (valor, minAnno, maxAnno) {
        if (!valor) return true;
        minAnno = minAnno == undefined ? 0 : minAnno;
        maxAnno = maxAnno == undefined ? 9999 : maxAnno;
        // Se valida q el formato sea, por ejemplo, 00001-2018
        var numReqRegExp = new RegExp('\\b(\\d{1,5}-\\d{4})$');
        if (numReqRegExp.test(valor)) {
            // [06/01/2021] Se deshabilita este chequeo de acuerdo a incidencia 16766 q impide q el número de oficio 04299-2020 se ingrese en el 2021
            //// Se valida q el año del Número de Oficio corresponda al año actual:
            //var anno = valor.substring(valor.indexOf("-") + 1, valor.length);
            //return parseInt(anno) <= maxAnno && parseInt(anno) >= minAnno;
            return true;
        }
        else {
            return false;
        }
    },

    configAreaEscritura: function() {
        // Para limitar la cantidad de caracteres de un div editable o un textarea
        // y mostrar la cantidad de caracteres restantes disponibles para escritura.
        // Aplicable a <div> y <textarea>.
        $('.texto-largo').on({
            'keydown': function(e) {
                var event = e.originalEvent;
                var maxLengh = parseInt($(this).attr('maxlength'));
                if (!maxLengh) {
                    maxLengh = parseInt($(this).data('maxlength'));
                }

                var longTexto = $(this).is("textarea") ? $(this).val().length : $(this).text().length;
                if (event == undefined ||
                (longTexto >= maxLengh &&
                    event.keyCode != 8 && /*Backspace*/
                    event.keyCode != 46 && /*Delete*/
                    event.keyCode != 37 && /*Tecla izquieda*/
                    event.keyCode != 38 && /*Tecla izquieda*/
                    event.keyCode != 39 && /*Tecla arriba */
                    event.keyCode != 40)) /*Tecla abajo */
                {
                    event.preventDefault();
                    return false;
                }
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

                var clipbType = "text"; // $(this).is("textarea") ? "text/plain" : "text/plain";
                var len = $(this).is("textarea") ? $(this).val().length : this.innerText.length,
                    cp = e.originalEvent.clipboardData.getData(clipbType);

                if ((len + cp.length) > maxLengh) {
                    if ($(this).is("textarea"))
                        $(this).val($(this).val() + cp.substring(0, maxLengh - len));
                    else
                        this.innerHTML += cp.substring(0, maxLengh - len);
                    return false;
                } else if (!$(this).is("textarea")) {
                    // si es un div editable entonces se evita q se incruste código html.
                    // Se conservan los div editable para compatibilidad con campos q tienen código html, pero
                    // ahora se limitan estos campos a texto puro para ir eliminando el html.
                    // TODO: depurar en bd los campos con código html y dejarlos en puro texto y utilizar en la app
                    // Textarea. Tener en cuenta q al elimnar el código html en los campos en bd
                    // tienen q conservarse los cambios de línea en el texto.
                    this.innerHTML += cp;
                    return false;
                }
            },
            'drop': function(e) {
                e.preventDefault();
                e.stopPropagation();
            }
        });
    },

    soloMesAnnoChange: function() {
        try {
            var controlFecha = $("#fechaDocumento");
            var datepicker = controlFecha.data("kendoDatePicker");
            var chkSoloMesAnno = $("#soloMesAnno");
            //var fechaDocStr = controlFecha.val();
            if (chkSoloMesAnno.is(":checked")) {
                // Formatear a sólo Mes y Año
                datepicker.setOptions({
                    format: Main.formatoFechaMY
                });
                //var estaEnFormatoFull = moment(fechaDocStr, formatoFechaMom, true).isValid();
                //if (estaEnFormatoFull) {
                //    fechaDocFull = fechaDocStr;
                //    var fechaDocMYStr = moment(fechaDocStr, formatoFechaMom).format(formatoFechaMYMom);
                //    //controlFecha.val(fechaDocMYStr);
                //    var datepicker = controlFecha.data("kendoDatePicker");
                //}
            } else {
                // Formato completo
                datepicker.setOptions({
                    format: formatoFecha
                });
                //var estaEnFormatoMY = moment(fechaDocStr, formatoFechaMYMom, true).isValid();
                //if (estaEnFormatoMY) {
                //    var diaFechaFull = moment(fechaDocFull, formatoFechaMom).date();
                //    if (!diaFechaFull) {
                //        diaFechaFull = 1;
                //    }
                //    var fechaDocFullStr = moment(fechaDocStr, formatoFechaMYMom).date(diaFechaFull).format(formatoFechaMom);
                //    controlFecha.val(fechaDocFullStr);
                //}
            }
        } catch (err) {
        }
    },

    setDateInputLang: function(datePicker) {
        if (!datePicker) return;
        var datetimepicker = datePicker.data("kendoDatePicker");
        if (!datetimepicker) return;
        if (!datetimepicker._dateInput) return;
        if (datetimepicker) {
            datetimepicker._dateInput.setOptions({
                messages: {
                    "year": "aaaa",
                    "month": "mm",
                    "day": "dd",
                    "hour": "hh",
                    "minute": "mm",
                    "second": "ss",
                    "dayperiod": "am/pm",
                }
            });
        }
    },

    setValueHiddeFieldTextArea: function(hiddenField, textArea) {
        if ($("#" + textArea).length && $('input[name="' + hiddenField + '"]').length) {
            $('input[name="' + hiddenField + '"]').val($("#" + textArea).html());
        }

    },

    esEmailMultiValido: function(email) {
        var re =
            /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
        return re.test(email);
    },

    esTelefonoValido: function (telefono) {
        var re = /^[0-9]+$/; // Sólo números
        return re.test(telefono);
    },

    esTelefonoValido_OLD: function (telefono) {
        var re = /^(\+?56)?(\s?)(0?9)(\s?)[9876543]\d{7}$/; // Formato correcto de número teléfono Chile
        return re.test(telefono);
    },

    stripHtml: function (html) {
        try {
            var doc = new DOMParser().parseFromString(html, 'text/html');
            return doc.body.textContent || "";
        } catch (error) {
            console.error(error);
            return html;
        }
    },

    limpiaHtmlTextArea: function() {
        $("textarea").each(function (index) {
            var texto = $(this).text();
            texto = Form.stripHtml(texto);
            $(this).text(texto);
        });
    }


};
