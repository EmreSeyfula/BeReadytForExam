using System.Collections.Generic;
namespace BeReadyForExam.Models
{
    public class Topic
    {
        public int Id { get; set; }

        public int SubjectId { get; set; }

        public string Name { get; set; }

        public bool IsActive { get; set; }

        public Subject Subject { get; set; }

        public ICollection<Question> Questions { get; set; }
    }
}
