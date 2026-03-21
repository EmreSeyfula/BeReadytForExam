using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
namespace BeReadyForExam.ViewModel.Exam
{
    public class ExamFormViewModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a topic.")]
        public int TopicId { get; set; }

        public bool IsActive { get; set; } = true;

        [Range(1, 500, ErrorMessage = "Questions count must be between 1 and 500.")]
        public int QuestionsCount { get; set; } = 10;

        [Range(1, 600, ErrorMessage = "Time limit must be between 1 and 600 minutes.")]
        public int? TimeLimitMinutes { get; set; }

        public bool RandomizeQuestions { get; set; } = true;

        public List<SelectListItem> Topics { get; set; } = new();
    }
}
