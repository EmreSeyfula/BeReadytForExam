using BeReadyForExam.ViewModel.Exam;

namespace BeReadyForExam.Services.Interfaces
{
    public interface IExamService
    {
        Task<int> StartExamAsync(int topicId, string userId);
        Task<TakeExamViewModel> GetExamAsync(int attemptId);
        Task SubmitExamAsync(SubmitExamViewModel model);
        Task<ExamResultViewModel> GetResultAsync(int attemptId, string userId);
        Task<MyExamHistoryViewModel> GetMyHistoryAsync(string userId);
    }
}
