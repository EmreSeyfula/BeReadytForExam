namespace BeReadyForExam.ViewModel.Exam
{
    public class ExamQuestionEditorViewModel
    {
        public int Id { get; set; }

        public string Text { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public List<ExamOptionEditorViewModel> Options { get; set; } = new()
        {
            new(),
            new()
        };
    }
}
