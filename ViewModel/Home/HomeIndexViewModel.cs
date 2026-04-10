namespace BeReadyForExam.ViewModel.Home
{
    public class HomeIndexViewModel
    {
        public int SubjectsCount { get; set; }

        public int QuestionsCount { get; set; }

        public int UsersCount { get; set; }

        public bool IsAuthenticated { get; set; }

        public bool IsAdmin { get; set; }

        public bool HasProgressData { get; set; }

        public string ProgressTitle { get; set; } = string.Empty;

        public string ProgressDescription { get; set; } = string.Empty;

        public double ProgressPercent { get; set; }

        public string ProgressValueText { get; set; } = string.Empty;

        public string SecondaryMetricLabel { get; set; } = string.Empty;

        public string SecondaryMetricValueText { get; set; } = string.Empty;

        public double SecondaryMetricPercent { get; set; }

        public string DetailsButtonText { get; set; } = string.Empty;

        public string DetailsController { get; set; } = string.Empty;

        public string DetailsAction { get; set; } = string.Empty;
    }
}
