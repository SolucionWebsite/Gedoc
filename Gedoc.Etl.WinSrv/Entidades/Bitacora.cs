using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gedoc.Etl.Winsrv.Entidades
{
    public class Bitacora : BaseEntity
    {
        // TODO: quitar los bool
        public bool JCMN_notif { get; set; }
        public bool Desp_notif { get; set; }
        public bool SolRev_notif { get; set; }
        public bool solRev_EsperarRespuesta { get; set; }
        //public bool _HasCopyDestinations { get; set; }
        //public bool _IsCurrentVersion { get; set; }
        public string Estado_x0020_del_x0020_registro { get; set; }
        public string Enviar_x0020_solicitud { get; set; }
        public string ContentType { get; set; }
        //public string Edit { get; set; }
        //public string LinkTitleNoMenu { get; set; }
        //public string LinkTitle { get; set; }
        //public string LinkTitle2 { get; set; }
        //public string LinkFilenameNoMenu { get; set; }
        //public string LinkFilename { get; set; }
        //public string LinkFilename2 { get; set; }
        //public string ServerUrl { get; set; }
        //public string EncodedAbsUrl { get; set; }
        //public string BaseName { get; set; }
        public string ContentTypeId { get; set; }
        public int ID { get; set; }
        public DateTime? Fecha { get; set; }
        public DateTime? Fecha_x0020_solicitud_x0020_revi { get; set; }
        public DateTime? Fecha_x0020_solicitud_x0020_desp { get; set; }
        public DateTime? JCMN_fecha { get; set; }
        public DateTime? Desp_fecha { get; set; }
        public DateTime? SolRev_fecha { get; set; }
        public DateTime? Modified { get; set; }
        public DateTime? Created { get; set; }
        public string Requerimiento { get; set; }
        public string Documento_x0020_ingreso_x003a_ID { get; set; }
        public string Documento_x0020_ingreso_x003A_Fe { get; set; }
        public string Documento_x0020_ingreso_x003a_N_ { get; set; }
        public string Numero_x0020_de_x0020_Despacho { get; set; }
        public string FileRef { get; set; }
        public string FileDirRef { get; set; }
        public string Last_x0020_Modified { get; set; }
        public string Created_x0020_Date { get; set; }
        public string Observaciones { get; set; }
        public string _x00da_ltimo_x0020_comentario { get; set; }
        public string Title { get; set; }
        public string File_x0020_Type { get; set; }
        public string JCMN_correo { get; set; }
        public string Desp_correo { get; set; }
        public string SolRev_correo { get; set; }
        public string Solicitar_x0020_revisi_x00f3_n_x { get; set; }
        public string Solicitar_x0020_Despacho_x0020_a { get; set; }
        public string Usuario_x0020_solicitud_x0020_re { get; set; }
        public string Usuario_x0020_solicitud_x0020_de { get; set; }
        public string Author { get; set; }
        public string Editor { get; set; }
        public string UniqueId { get; set; }
        //public int IdCarga { get; set; }
    }

}
