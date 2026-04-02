using System.ComponentModel.DataAnnotations;

namespace BeReadyForExam.Models
{
    public class Exam
    {
        public int Id { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Моля, изберете тема.")]
        public int TopicId { get; set; }
        public Topic? Topic { get; set; }

        [Required(ErrorMessage = "Полето „Заглавие“ е задължително.")]
        [MaxLength(200)]
        public string Title { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public int? TimeLimitMinutes { get; set; }
        public int QuestionsCount { get; set; } = 10;
        public bool RandomizeQuestions { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Question> Questions { get; set; } = new List<Question>();
        public ICollection<ExamAttempt> Attempts { get; set; } = new List<ExamAttempt>();
    }
}