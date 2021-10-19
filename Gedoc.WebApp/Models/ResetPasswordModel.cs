using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Gedoc.WebApp.Models
{
    public class ResetPasswordModel
    {
        [Required(ErrorMessage = "La nueva password es requerida", AllowEmptyStrings = false)]
        [DataType(DataType.Password)]
        [StringLength(10, ErrorMessage = "La password no puede superar los 10 caracteres.")]
        public string NuevaClave { get; set; }

        [DataType(DataType.Password)]
        [Compare("NuevaClave", ErrorMessage = "Las password no coinciden")]
        [StringLength(10, ErrorMessage = "La password no puede superar los 10 caracteres.")]
        public string ConfirmaPassword { get; set; }
        [Required]
        public string ResetToken { get; set; }
    }
}