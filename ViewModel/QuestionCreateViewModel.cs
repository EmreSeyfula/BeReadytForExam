using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace BeReadyForExam.ViewModel
{
    public class QuestionCreateViewModel
    {
        public string Text { get; set; }
        public int Id { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Please select a test.")]
        public int ExamId { get; set; }
        public List<SelectListItem> Exams { get; set; } = new();

        public List<OptionInputModel> Options { get; set; } = new();
    }

    public class OptionInputModel
    {
        [Required]
        public string Text { get; set; }
        public int Id { get; set; }
        public bool IsCorrect { get; set; }
    }
}
