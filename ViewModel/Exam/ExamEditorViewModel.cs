using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace BeReadyForExam.ViewModel.Exam
{
    public class ExamEditorViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Полето „Заглавие“ е задължително.")]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public int? SubjectId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Моля, изберете тема.")]
        public int TopicId { get; set; }

        [Range(1, 500, ErrorMessage = "Броят въпроси трябва да е между 1 и 500.")]
        public int QuestionsCount { get; set; } = 5;

        [Range(1, 600, ErrorMessage = "Ограничението за време трябва да е между 1 и 600 минути.")]
        public int? TimeLimitMinutes { get; set; }

        public bool RandomizeQuestions { get; set; } = true;

        public bool IsActive { get; set; } = true;

        public List<ExamQuestionEditorViewModel> Questions { get; set; } = new();

        public List<SelectListItem> Subjects { get; set; } = new();

        public List<SelectListItem> Topics { get; set; } = new();
    }
}