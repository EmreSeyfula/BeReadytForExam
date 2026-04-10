namespace BeReadyForExam.ViewModel.Question
{
    public class BulkCreateQuestionsViewModel
    {
        public int ExamId { get; set; }

        public List<QuestionInputModel> Questions { get; set; } = new();
    }

    public class QuestionInputModel
    {
        public string Text { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        public List<OptionInputModel> Options { get; set; } = new()
        {
            new(), new()
        };
    }
}
