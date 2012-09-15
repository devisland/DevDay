using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DevDay.Models.VM
{
    public class Credential
    {
        [Required(ErrorMessage = "{0} é requerido.")]
        [DisplayName("E-mail")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required(ErrorMessage = "{0} é requerido.")]
        [DisplayName("Senha")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}