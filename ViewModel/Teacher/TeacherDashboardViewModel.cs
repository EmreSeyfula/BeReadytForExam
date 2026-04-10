namespace BeReadyForExam.ViewModel.Teacher
{
    public class TeacherDashboardViewModel
    {
        public int TotalExams { get; set; }
        public int ActiveExams { get; set; }
        public int TotalQuestions { get; set; }
        public int TotalAttempts { get; set; }
        public int CompletedAttempts { get; set; }
        public double AverageScorePercent { get; set; }

        public List<TeacherRecentAttemptVM> RecentAttempts { get; set; } = new();
    }

    public class TeacherRecentAttemptVM
    {
        public int AttemptId { get; set; }
        public string ExamTitle { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserDisplayName { get; set; } = string.Empty;
        public int TotalQuestions { get; set; }
        public int CorrectCount { get; set; }
        public double ScorePercent { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
    }
}
