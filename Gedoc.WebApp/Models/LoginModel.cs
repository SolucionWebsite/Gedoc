using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Gedoc.WebApp.Models
{
    public class LoginModel
    {
        [DisplayName("Usuario:")]
        [Required(ErrorMessage = "Este campo es requerido"), MaxLength(255)]
        public string Username { get; set; }
        public string Email { get; set; }
        [DisplayName("Password:")]
        [Required(ErrorMessage = "Este campo es requerido"), MaxLength(255)]
        public string Password { get; set; }
        public string Mensaje { get; set; }
    }
}