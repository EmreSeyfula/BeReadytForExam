namespace BeReadyForExam.ViewModel.Exam
{
    public class TeacherExamRowViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string TopicName { get; set; }
        public bool IsActive { get; set; }

        public int ConfiguredQuestionsCount { get; set; } 
        public int ActualQuestionsInBank { get; set; }   
    }
}
