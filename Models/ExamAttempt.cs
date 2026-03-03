using System;
using System.Collections.Generic;
namespace BeReadyForExam.Models
{
    public class ExamAttempt
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public int ExamId { get; set; }
       
        public DateTime StartedAt { get; set; }

        public DateTime? FinishedAt { get; set; }

        public int TotalQuestions { get; set; }

        public int CorrectCount { get; set; }

        public double ScorePercent { get; set; }

        public double Grade { get; set; }

        public Exam Exam { get; set; }

        public ICollection<AttemptAnswer>? AttemptAnswers { get; set; }
    }
}
