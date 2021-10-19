

var Acciones = Acciones ||
{
    ejecutarAccion: function (idAccion) {
        switch (idAccion) {
            case 'ic':
                // Ingreso Central
                aCCIONES.ingresoCentral();
                break;
        }
    },

    ejecutaAccionNuevaVentana: function (titulo, url, width, height, accion, callback, ocultarClose) {
        var result = Main.muestraVentanaModal(titulo, url, width, height, ocultarClose, "winFormIngreso", undefined);
        result.then(function (result) {
            if (result == true || result.Resultado == true || accion == "BI") {
                // refrescar bandeja. TODO: chequear si la acción después de realizada hace refrescamiento de la bandeja.
                //***if (accion == "EDESP" || accion == 'CDESP' || accion == "BITA" || accion == "ADJ" || accion == "NOTIF" || accion == "PLANT") {
                    // Si es editar despacho o cierre de despacho o nueva bitacora o nuevo adjunto
                if (callback) {
                    callback();
                    //***}
                } else { // Se refresca la página para q la bandeja refleje los cambios
                    if ($("#gridBandeja").length && $("#gridBandeja").data("kendoGrid")) {
                        $("#gridBandeja").data("kendoGrid").dataSource.read();  // Se refresca bandeja de entrada
                    } else
                    {
                        location.reload(true); // Se recarga toda la página
                    }
                    if ($("#gridBandejaPrio").length && $("#gridBandejaPrio").data("kendoGrid")) {
                        $("#gridBandejaPrio").data("kendoGrid").dataSource.read(); // Se refresca pestaña de priorizados
                    }
                    if (typeof muestraCantidadPriorizados === 'function') {
                        muestraCantidadPriorizados(); // Se actualiza texto de cantidad de notificaciones
                    }

                }
            } else {
                // hubo error o se canceló la acción, no se hace nada
            }
        });
    },

    ingresoCentral: function (url) {
        var result = Main.muestraVentanaModal('Ingreso Central', url + '?idAccion=IC');
        result.then(function (result) {
            if (result) {
                //TODO: refrescar bandeja
                console.log("Se refresca bandeja");
            } else {
                console.log("No se hace nada");
            }
        });
    },

    asignacionUt: function (url) {
        var result = Main.muestraVentanaModal('Asignación de UT', url + '?idAccion=AU');
        result.then(function (result) {
            if (result) {
                //TODO: refrescar bandeja
                console.log("Se refresca bandeja");
            } else {
                console.log("No se hace nada");
            }
        });
    },

    priorizacon: function (url) {
        var result = Main.muestraVentanaModal('Priorizaciónl', url + '?idAccion=PR');
        result.then(function (result) {
            if (result) {
                //TODO: refrescar bandeja
                console.log("Se refresca bandeja");
            } else {
                console.log("No se hace nada");
            }
        });
    }

}