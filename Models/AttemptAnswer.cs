namespace BeReadyForExam.Models
{
    public class AttemptAnswer
    {
        public int Id { get; set; }

        public int ExamAttemptId { get; set; }

        public int QuestionId { get; set; }

        public int SelectedOptionId { get; set; }

        public bool IsCorrect { get; set; }

        public ExamAttempt ExamAttempt { get; set; } = null!;
        public Question Question { get; set; } = null!;
        public Option SelectedOption { get; set; } = null!;
    }
}
