using BeReadyForExam.Models;

namespace BeReadyForExam.Services.Interfaces
{
    public interface ITopicService
    {
        Task<List<Topic>> GetAllAsync();
        Task<Topic> GetByIdAsync(int id);
        Task CreateAsync(Topic topic);
        Task UpdateAsync(Topic topic);
        Task DeleteAsync(int id);

        Task<List<Subject>> GetAllSubjectsAsync();
    }
}
