namespace BeReadyForExam.ViewModel.Exam
{
    public class MyExamHistoryViewModel
    {
        public List<MyExamAttemptRowVM> Attempts { get; set; } = new();
    }
    public class MyExamAttemptRowVM
    {
        public int AttemptId { get; set; }
        public string TopicName { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public int TotalQuestions { get; set; }
        public int CorrectCount { get; set; }
        public double ScorePercent { get; set; }
        public double Grade { get; set; }
    }
}
