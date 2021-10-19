using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace Gedoc.WebReport.Logging
{
    public class ActionLogger
    {
        private ILog _log = null;

        public void LoadConfig(string nameLogger)
        {
            log4net.Config.XmlConfigurator.Configure();
            _log = log4net.LogManager.GetLogger(nameLogger);
        }


        public void Error(Exception ex)
        {
            //if (ex is System.Data.Entity.Validation.DbEntityValidationException)
            //{
            //    DbEntityValidationResult first = null;
            //    foreach (var error in ((System.Data.Entity.Validation.DbEntityValidationException)ex).EntityValidationErrors)
            //    {
            //        first = error;
            //        break;
            //    }
            //    var mensaje = first == null ? ex.Message : first.ValidationErrors.FirstOrDefault().ErrorMessage;
            //    _log.Error(mensaje, ex);
            //}
            //else 
            if (ex.InnerException != null && ex.InnerException.InnerException != null && !string.IsNullOrWhiteSpace(ex.InnerException.InnerException.Message))
            {
                _log.Error(ex.InnerException.InnerException.Message, ex);
            }
            else
            {
                _log.Error(ex.Message, ex);
            }
        }

        public void Error(string message, Exception ex)
        {
            _log.Error(message, ex);
        }

        public void Error(string message)
        {
            _log.Error(message);
        }


        public void Info(string message)
        {
            _log.Info(message);
        }
    }
}
