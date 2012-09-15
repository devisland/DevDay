using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DevDay.Models
{
    [MetadataType(typeof(SubmissionMetadata))]
    public partial class Submission
    {
        public class SubmissionMetadata
        {
            [ScaffoldColumn(false)]
            public int ID { get; set; }

            [ScaffoldColumn(false)]
            public int UserID { get; set; }

            [Required(ErrorMessage = "{0} é requerido.")]
            [DisplayName("Nome")]
            [StringLength(50, ErrorMessage = "Limite de 50 caracteres")]
            public string Name { get; set; }

            [Required(ErrorMessage = "{0} é requerido.")]
            [DisplayName("Descrição")]
            [DataType(DataType.Password)]
            [StringLength(250, ErrorMessage = "Limite de 250 caracteres")]
            public string Description { get; set; }

            [ScaffoldColumn(false)]
            [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy HH:MM}", ApplyFormatInEditMode = true)]
            public DateTime CreatedOn { get; set; }

            [ScaffoldColumn(false)]
            public DateTime ModifiedOn { get; set; }

            [ScaffoldColumn(false)]
            public DateTime FilePath { get; set; }
        }
    }
}