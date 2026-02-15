using BeReadyForExam.Models;

namespace BeReadyForExam.Services.Interfaces
{
   
        public interface ISubjectService
        {
            Task<List<Subject>> GetAllAsync();
            Task<Subject> GetByIdAsync(int id);
            Task CreateAsync(Subject subject);
            Task UpdateAsync(Subject subject);
            Task DeleteAsync(int id);
        }

}
