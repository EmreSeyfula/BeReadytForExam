using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace BeReadyForExam.Models
{
    public class Subject
    {
        public int Id { get; set; }
        [Display(Name = "Име")]
        [Required(ErrorMessage = "Полето „Име“ е задължително.")]
        public string Name { get; set; }
        [Display(Name = "Описание")]
        public string Description { get; set; }

        public ICollection<Topic>? Topics { get; set; }
    }
}
