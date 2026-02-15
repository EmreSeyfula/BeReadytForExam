namespace BeReadtForExam.Models
{
    public class AttemptAnswer
    {
        public int Id { get; set; }

        public int QuizAttemptId { get; set; }

        public int QuestionId { get; set; }

        public int SelectedOptionId { get; set; }

        public bool IsCorrect { get; set; }

        public QuizAttempt QuizAttempt { get; set; }
    }
}
