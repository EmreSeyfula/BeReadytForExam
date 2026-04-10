using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace BeReadyForExam.Models
{
    public class Topic
    {
        public int Id { get; set; }
        [Display(Name = "Предмет")]
        public int SubjectId { get; set; }
        [Display(Name = "Име")]
        [Required(ErrorMessage = "Полето „Име“ е задължително.")]
        public string Name { get; set; } = string.Empty;
        [Display(Name = "Активна")]
        public bool IsActive { get; set; }
        [Display(Name = "Предмет")]
        public Subject? Subject { get; set; }

        public ICollection<Question> Questions { get; set; } = new List<Question>();
    }
}
