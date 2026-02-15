using Microsoft.CodeAnalysis.Options;
using System.Collections.Generic;
namespace BeReadyForExam.Models
{
  
        public class Question
        {
            public int Id { get; set; }

            public int TopicId { get; set; }

            public string Text { get; set; }

            public bool IsActive { get; set; }

            public Topic Topic { get; set; }

            public ICollection<Option> Options { get; set; }
        }
}

