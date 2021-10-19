using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gedoc.Etl.Winsrv.Entidades
{
    public class Despacho : BaseEntity
    {
        public string Estado_x0020_del_x0020_registro { get; set; }
        public string Estado_x0020_del_x0020_despacho { get; set; }
        public string G_x00e9_nero_x0020_destinatario_x0020_nuevo { get; set; }
        public string Tipo_x0020_de_x0020_Instituci_x00f3_n_x0020_destinatario_x0020_nuevo { get; set; }
        public string ContentType { get; set; }
        public string LinkFilenameNoMenu { get; set; }
        public string LinkFilename { get; set; }
        public string LinkFilename2 { get; set; }
        public string ServerUrl { get; set; }
        public string BaseName { get; set; }
        public string FileSizeDisplay { get; set; }
        public string SelectTitle { get; set; }
        public string SelectFilename { get; set; }
        public string ContentTypeId { get; set; }
        public int ID { get; set; }
        public DateTime? Fecha_x0020_de_x0020_recepci_x00f3_n { get; set; }
        public DateTime? Fecha_x0020_emisi_x00f3_n_x0020_oficio { get; set; }
        public DateTime? Fecha_x0020_de_x0020_env_x00ed_o1 { get; set; }
        public DateTime? Fecha { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? Modified { get; set; }
        public string GUID { get; set; }
        public string Proveedor_x0020_de_x0020_despacho { get; set; }
        public string Medio_x0020_de_x0020_despacho { get; set; }
        public string Medio_x0020_de_x0020_verificaci_x00f3_n { get; set; }
        public string Proveedor_x0020_de_x0020_despacho_x003A_T_x00ed_tulo { get; set; }
        public string Medio_x0020_de_x0020_despacho_x003A_T_x00ed_tulo { get; set; }
        public string Medio_x0020_de_x0020_verificaci_x00f3_n_x003A_T_x00ed_tulo { get; set; }
        public string Destinatario { get; set; }
        public string Destinatario_x003A_Correo_x0020_electr_x00f3_nico { get; set; }
        public string Destinatario_x003a_Nombre { get; set; }
        public string Destinatario_x003a_Instituci_x00f3_n { get; set; }
        public string Unidad_x0020_T_x00e9_cnica_x0020_Asignada { get; set; }
        public string FileRef { get; set; }
        public string FileDirRef { get; set; }
        public string Last_x0020_Modified { get; set; }
        public string Created_x0020_Date { get; set; }
        public string File_x0020_Size { get; set; }
        public string UniqueId { get; set; }
        public string Tipos_x0020_de_x0020_adjuntos { get; set; }
        public string Tipos_x0020_de_x0020_adjuntos_x003A_T_x00ed_tulo { get; set; }
        public string Soporte { get; set; }
        public string Soporte_x003A_T_x00ed_tulo { get; set; }
        public string Requerimiento { get; set; }
        public string Documento_x0020_ingreso_x003A_Fecha_x0020_de_x0020_ingreso { get; set; }
        public string Requerimiento_x003A_T_x00ed_tulo { get; set; }
        public string Documento_x0020_ingreso_x003A_ID { get; set; }
        public string Destinatarios_x0020_en_x0020_copia { get; set; }
        public string Destinatarios_x0020_en_x0020_copia_x003A_Correo_x0020_electr_x00f3_nico { get; set; }
        public string Documento_x0020_ingreso_x003a_UT { get; set; }
        public string Documento_x0020_ingreso_x003a_txtProfesionalEnArea { get; set; }
        public string Etiqueta { get; set; }
        public string Comuna { get; set; }
        public string Regi_x00f3_n { get; set; }
        public string Materia_x0020_de_x0020_despacho { get; set; }
        public string Observaciones_x0020_de_x0020_adjuntos { get; set; }
        public string Observaciones_x0020_medio_x0020_de_x0020_verificaci_x00f3_n { get; set; }
        public string Observaciones_x0020_del_x0020_despacho { get; set; }
        public string Observaciones_x002f_Acuerdos_x002f_Comentarios { get; set; }
        public string Descripci_x00f3_n_x0020_de_x0020_proyecto_x0020_o_x0020_actividad { get; set; }
        public string N_x00fa_mero_x0020_gu_x00ed_a_x0020_de_x0020_despacho { get; set; }
        public int Cantidad_x0020_de_x0020_adjuntos { get; set; }
        public string Modified_x0020_By { get; set; }
        public string Created_x0020_By { get; set; }
        public string File_x0020_Type { get; set; }
        public string HTML_x0020_File_x0020_Type { get; set; }
        public string Title { get; set; }
        public string N_x00fa_mero_x0020_de_x0020_despacho { get; set; }
        public string Correo_x0020_electr_x00f3_nico_x0020_destinatario_x0020_nuevo { get; set; }
        public string Direcci_x00f3_n_x0020_destinatario_x0020_nuevo { get; set; }
        public string Nombre_x0020_destinatario_x0020_nuevo { get; set; }
        public string Tel_x00e9_fono_x0020_destinatario_x0020_nuevo { get; set; }
        public string Cargo_x0020_o_x0020_profesi_x00f3_n_x0020_destinatario_x0020_nuevo { get; set; }
        public string Instituci_x00f3_n_x0020_destinatario_x0020_nuevo { get; set; }
        public string Dest_correo { get; set; }
        public string Today { get; set; }
        public string Rol_x0020_MN { get; set; }
        public string Responsable_x0020_del_x0020_requerimiento { get; set; }
        public string Author { get; set; }
        public string Editor { get; set; }
        public string FileLeafRef { get; set; }
        //public int IdCarga { get; set; }
    }
}
