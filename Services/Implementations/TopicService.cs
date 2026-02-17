using Microsoft.EntityFrameworkCore;
using BeReadyForExam.Data;
using BeReadyForExam.Models;
using BeReadyForExam.Services.Interfaces;
namespace BeReadyForExam.Services.Implementations
{
    public class TopicService : ITopicService
    {
        private readonly ApplicationDbContext _context;
        public TopicService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<List<Topic>> GetAllAsync()
        {
            return await _context.Topics.Include(t => t.Subject).ToListAsync();
        }
        public async Task<Topic> GetByIdAsync(int id)
        {
            return await _context.Topics.Include(t => t.Subject).FirstOrDefaultAsync(t => t.Id == id);
        }
        public async Task CreateAsync(Topic topic)
        {
            _context.Topics.Add(topic);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(Topic topic)
        {
            _context.Topics.Update(topic);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(int id)
        {
            var topic = await _context.Topics.FindAsync(id);
            if (topic != null)
            {
                _context.Topics.Remove(topic);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<List<Subject>> GetAllSubjectsAsync()
        {
            return await _context.Subjects.ToListAsync();
        }
    }
}
