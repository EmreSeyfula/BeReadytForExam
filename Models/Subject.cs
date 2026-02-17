using System.Collections.Generic;
namespace BeReadyForExam.Models
{
    public class Subject
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public ICollection<Topic>? Topics { get; set; }
    }
}
