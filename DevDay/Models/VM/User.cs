using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DevDay.Models
{
    [MetadataType(typeof(UserMetadata))]
    public partial class User
    {
        public class UserMetadata
        {
            [ScaffoldColumn(false)]
            public int ID { get; set; }

            [Required(ErrorMessage = "{0} é requerido.")]
            [DisplayName("Nome")]
            [StringLength(50, ErrorMessage = "Limite de 50 caracteres")]
            public string Name { get; set; }

            [Required(ErrorMessage = "{0} é requerido.")]
            [DisplayName("E-mail")]
            [DataType(DataType.EmailAddress)]
            [StringLength(50, ErrorMessage = "Limite de 50 caracteres")]
            public string Email { get; set; }

            [DisplayName("Senha")]
            [DataType(DataType.Password)]
            [StringLength(50, ErrorMessage = "Limite de 50 caracteres")]
            public string Password { get; set; }

            [DisplayName("Vou participar do desafio")]
            public DateTime IsCompetitor { get; set; }

            [DisplayName("Último logon em")]
            [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy HH:MM}", ApplyFormatInEditMode = true)]
            public DateTime LastLoggedOn { get; set; }
        }
    }
}