namespace BeReadyForExam.ViewModel.Exam
{
    public class SubmitExamViewModel
    {
        public int AttemptId { get; set; }

        public List<ExamAnswerInputVM> Answers { get; set; } = new();
    }

    public class ExamAnswerInputVM
    {
        public int QuestionId { get; set; }
        public int SelectedOptionId { get; set; }
    }
}

