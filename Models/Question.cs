using Microsoft.CodeAnalysis.Options;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace BeReadyForExam.Models
{
  
        public class Question
        {
            public int Id { get; set; }

        public int ExamId { get; set; }
        public Exam Exam { get; set; }
        [Required]    
            public string Text { get; set; }

            public bool IsActive { get; set; }

            public ICollection<Option>? Options { get; set; }
        }
}

