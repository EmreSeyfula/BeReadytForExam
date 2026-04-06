using BeReadyForExam.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BeReadyForExam.ViewModel.Question
{
    public class QuestionIndexViewModel
    {
        public List<BeReadyForExam.Models.Question> Questions { get; set; } = new();

        public string? Search { get; set; }
        public int? SubjectId { get; set; }
        public int? TopicId { get; set; }
        public int? ExamId { get; set; }
        public bool? IsActive { get; set; }

        public List<SelectListItem> Subjects { get; set; } = new();
        public List<SelectListItem> Topics { get; set; } = new();
        public List<SelectListItem> Exams { get; set; } = new();
    }
}


