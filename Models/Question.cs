using Microsoft.CodeAnalysis.Options;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace BeReadyForExam.Models
{
  
        public class Question
        {
            public int Id { get; set; }
        [Display(Name = "Тест")]
        public int ExamId { get; set; }

        public Exam Exam { get; set; }

        [Display(Name = "Текст")]
        [Required(ErrorMessage = "Полето „Текст“ е задължително.")]
        public string Text { get; set; }
        [Display(Name = "Активен")]
        public bool IsActive { get; set; }

            public ICollection<Option>? Options { get; set; }
        }
}

