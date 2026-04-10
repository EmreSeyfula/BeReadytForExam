using BeReadyForExam.Models;
using BeReadyForExam.ViewModel.Exam;
using BeReadyForExam.ViewModel.Teacher;
namespace BeReadyForExam.Services.Interfaces
{
    public interface IExamService
    {
        Task<List<Exam>> GetAllAsync();
        Task<Exam?> GetByIdAsync(int id);
        Task<ExamEditorViewModel?> GetEditorByIdAsync(int id);
        Task<int> CreateAsync(Exam exam);
        Task<int> CreateWithQuestionsAsync(ExamEditorViewModel model);
        Task UpdateAsync(Exam exam);
        Task UpdateWithQuestionsAsync(ExamEditorViewModel model);
        Task DeleteAsync(int id);
        Task<int> StartExamAsync(int examId, string userId);
        Task<TakeExamViewModel> GetExamAsync(int attemptId, string userId, bool canAccessAllAttempts = false);
        Task SubmitExamAsync(SubmitExamViewModel model, string userId, bool canAccessAllAttempts = false);
        Task<ExamResultViewModel> GetResultAsync(int attemptId, string userId, bool canAccessAllAttempts = false);
        Task<MyExamHistoryViewModel> GetMyHistoryAsync(string userId);
        Task<List<Exam>> GetAllActiveExamsAsync();
        Task<List<Exam>> GetAvailableExamsAsync();
        Task<List<TeacherExamRowViewModel>> GetTeacherExamListAsync();
        Task<TeacherDashboardViewModel> GetTeacherDashboardAsync();
    }
}
