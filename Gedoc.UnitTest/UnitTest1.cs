using Gedoc.Repositorio.Interfaces;
using Gedoc.Repositorio.Maps.Interfaces;
using Gedoc.Service.DataAccess;
using Gedoc.Service.DataAccess.Interfaces;
using Gedoc.WebApp.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ControllerContext = System.Web.Mvc.ControllerContext;

namespace Gedoc.UnitTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void UsuarioController_NuevoSolicitanteUrgencia_DevolverNombreVista()
        {
            //Devolver nombre de la vista redireccionada

            //Arrange
            var servicioUsuario = new Mock<UsuarioService>(new Mock<IUsuarioRepositorio>().Object); //Simular implementación de interfaz
            var controlador = new UsuarioController(servicioUsuario.Object); //Llamar al controlador con parámetro de la interfaz

            //Act
            var resultado = controlador.NuevoSolicitanteUrgencia(); //Guardar en var el resultado del método

            //Assert
            Assert.AreEqual(((ViewResult)resultado).ViewName, "FormSolicitanteUrgencia"); //Validar nombre de la View devuelta
        }

        [TestMethod]
        public void ReporteController_EjecutarReporte_DevolverMensajeError()
        {
            //Devolver mensaje de error al no encontrar reporte

            //Arrange
            var servicioMantenedor = new Mock<MantenedorService>(new Mock<IMantenedorRepositorio>().Object);
            var controlador = new ReporteController(servicioMantenedor.Object);

            int id = 1;
            int session = 1;
            int ut = 1;

            //Act
            var resultado = (RedirectToRouteResult)controlador.EjecutarReporte(id, session, ut);

            //Assert
            Assert.AreEqual("El reporte especificado no se enuentra en la aplicación.", resultado.RouteValues["mensaje"]); //Validar mensaje de error devuelto

        }

        [TestMethod]
        public void ReporteController_Excel_Export_Save_DevolverNombreArchivo()
        {
            //Devolver nombre del archivo guardado

            //Arrange
            var servicioMantenedor = new Mock<MantenedorService>(new Mock<IMantenedorRepositorio>().Object);
            var controlador = new ReporteController(servicioMantenedor.Object);

            string typeContent = "Libro de Excel";
            string base64 = "QXJjaGl2b0V4Y2VsLnhsc3g=";
            string Nombre = "ArchivoExcel.xlsx";

            //Act
            var resultado = (FileContentResult)controlador.Excel_Export_Save(typeContent, base64, Nombre);

            //Assert
            Assert.AreEqual("ArchivoExcel.xlsx", resultado.FileDownloadName); //Validar nombre archivo

        }

        [TestMethod]
        public void ArchivarRequerimientoController_EmptyTrash_DevolverArchivoJsonMensajeError()
        {
            //Validar generacion de archivo Json no vacío
            //Validar mensaje de error

            //Arrange
            var servicioPapelera = new Mock<PapeleraService>();
            var controlador = new PapeleraController(servicioPapelera.Object);

            var context = new Mock<ControllerContext>(); //Simular un contexto para el controlador
            var session = new Mock<HttpSessionStateBase>(); //Simular una Session
            context.Setup(m => m.HttpContext.Session).Returns(session.Object); //Retornar variable de sesión simulada
            controlador.ControllerContext = context.Object; //Asignar contexto al controlador

            //Act
            var resultado = (JsonResult)controlador.EmptyTrash();

            //Assert
            var propiedad = resultado.Data.GetType().GetProperties().Where(p => string.Compare(p.Name, "Mensaje") == 0).FirstOrDefault(); //Encontrar mensaje JsonResult
            string mensaje = propiedad.GetValue(resultado.Data).ToString();

            Assert.IsNotNull(resultado.Data); //Validar que el archivo Json no esté vacío
            Assert.AreEqual("Se perdio la Sesión", mensaje); //Validar mensaje de error

        }

    }
}
