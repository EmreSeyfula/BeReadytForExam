namespace BeReadyForExam.ViewModel.Exam
{
    public class ExamResultViewModel
    {
        public int AttemptId { get; set; }
        public string TopicName { get; set; } = string.Empty;

        public int TotalQuestions { get; set; }
        public int CorrectCount { get; set; }
        public double ScorePercent { get; set; }
        public double Grade { get; set; }

        public DateTime StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }

        public List<WrongAnswerVM> WrongAnswers { get; set; } = new();
    }

    public class WrongAnswerVM
    {
        public string QuestionText { get; set; } = string.Empty;
        public string SelectedOptionText { get; set; } = string.Empty;
        public string CorrectOptionText { get; set; } = string.Empty;
    }
}

