namespace BeReadyForExam.Models
{
    public class AttemptAnswer
    {
        public int Id { get; set; }

        public int ExamAttemptId { get; set; }

        public int QuestionId { get; set; }

        public int SelectedOptionId { get; set; }

        public bool IsCorrect { get; set; }

        public ExamAttempt ExamAttempt { get; set; }
        public Question Question { get; set; }
        public Option SelectedOption { get; set; }
    }
}
