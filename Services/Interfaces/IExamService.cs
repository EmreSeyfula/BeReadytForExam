using BeReadyForExam.Models;
using BeReadyForExam.ViewModel.Exam;

namespace BeReadyForExam.Services.Interfaces
{
    public interface IExamService
    {
        Task<List<Exam>> GetAllAsync();
        Task<Exam> GetByIdAsync(int id);
        Task<int> CreateAsync(Exam exam);
        Task UpdateAsync(Exam exam);
        Task DeleteAsync(int id);
        Task<int> StartExamAsync(int examId, string userId);
        Task<TakeExamViewModel> GetExamAsync(int attemptId);
        Task SubmitExamAsync(SubmitExamViewModel model);
        Task<ExamResultViewModel> GetResultAsync(int attemptId, string userId);
        Task<MyExamHistoryViewModel> GetMyHistoryAsync(string userId);
        Task<List<Exam>> GetAllActiveExamsAsync();
        Task<List<Exam>> GetAvailableExamsAsync();
        Task<List<TeacherExamRowViewModel>> GetTeacherExamListAsync();
    }
}
