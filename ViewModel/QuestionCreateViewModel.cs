using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace BeReadyForExam.ViewModel
{
    public class QuestionCreateViewModel
    {
        [Required]
        public string Text { get; set; }

        public int TopicId { get; set; }

        public List<SelectListItem> Topics { get; set; } = new();

        public List<OptionInputModel> Options { get; set; } = new();
    }

    public class OptionInputModel
    {
        public string Text { get; set; }
        public bool IsCorrect { get; set; }
    }
}
