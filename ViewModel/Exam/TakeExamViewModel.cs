namespace BeReadyForExam.ViewModel.Exam
{
    public class TakeExamViewModel
    {
        public int AttemptId { get; set; }
        public int ExamId { get; set; }

        public DateTime StartedAt { get; set; }
        public int? TimeLimitMinutes { get; set; }
        public DateTime? DeadlineUtc { get; set; }
        public int? SecondsRemaining { get; set; }
        public bool IsExpired { get; set; }

        public List<ExamQuestionVM> Questions { get; set; } = new();
    }

    public class ExamQuestionVM
    {
        public int QuestionId { get; set; }
        public string Text { get; set; } = string.Empty;
        public List<ExamOptionVM> Options { get; set; } = new();
    }

    public class ExamOptionVM
    {
        public int OptionId { get; set; }
        public string Text { get; set; } = string.Empty;
    }
}

