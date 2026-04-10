using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace BeReadyForExam.ViewModel
{
    public class QuestionCreateViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Полето „Текст“ е задължително.")]
        public string Text { get; set; } = string.Empty;

        public int? SubjectId { get; set; }
        public int? TopicId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Моля, изберете тест.")]
        public int ExamId { get; set; }

        public bool IsActive { get; set; } = true;

        public List<SelectListItem> Subjects { get; set; } = new();
        public List<SelectListItem> Topics { get; set; } = new();
        public List<SelectListItem> Exams { get; set; } = new();
        public List<OptionInputModel> Options { get; set; } = new();
    }

    public class OptionInputModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Полето „Текст на отговора“ е задължително.")]
        public string Text { get; set; } = string.Empty;

        [Display(Name = "Верен")]
        public bool IsCorrect { get; set; }
    }
}
