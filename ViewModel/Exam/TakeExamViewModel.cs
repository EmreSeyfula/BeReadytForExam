namespace BeReadyForExam.ViewModel.Exam
{
    public class TakeExamViewModel
    {
        public int AttemptId { get; set; }
        public int ExamId { get; set; }

        public List<ExamQuestionVM> Questions { get; set; } = new();
    }

    public class ExamQuestionVM
    {
        public int QuestionId { get; set; }
        public string Text { get; set; }

        public List<ExamOptionVM> Options { get; set; } = new();
    }

    public class ExamOptionVM
    {
        public int OptionId { get; set; }
        public string Text { get; set; }
    }
}

