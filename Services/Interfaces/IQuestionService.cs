using BeReadyForExam.Models;

namespace BeReadyForExam.Services.Interfaces
{
    public interface IQuestionService
    {
        Task<List<Question>> GetAllAsync();
        Task<Question> GetByIdAsync(int id);
        Task CreateAsync(Question question);
        Task UpdateAsync(Question question);
        Task DeleteAsync(int id);

        Task<List<Topic>> GetAllTopicsAsync();
    }
}
