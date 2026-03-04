using BeReadyForExam.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using BeReadyForExam.Data;
using BeReadyForExam.Models;

namespace BeReadyForExam.Services.Implementations
{
    public class QuestionService : IQuestionService
    {
        private readonly ApplicationDbContext _context;

        public QuestionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Question>> GetAllAsync(int? examId)
        {
            var query = _context.Questions
                .Include(q => q.Exam)
                    .ThenInclude(e => e.Topic)
                .Include(q => q.Options)
                .AsQueryable();

            if (examId.HasValue)
                query = query.Where(q => q.ExamId == examId.Value);

            return await query.ToListAsync();
        }

        public async Task<Question> GetByIdAsync(int id)
        {
            return await _context.Questions
             .Include(q => q.Options)
            .FirstOrDefaultAsync(q => q.Id == id);
        }

        public async Task CreateAsync(Question question)
        {
            _context.Questions.Add(question);
            await _context.SaveChangesAsync();
        }


        public async Task UpdateAsync(Question updated)
        {
            var dbQuestion = await _context.Questions
                .Include(q => q.Options)
                .FirstOrDefaultAsync(q => q.Id == updated.Id);

            if (dbQuestion == null)
                throw new InvalidOperationException("Question not found.");

            dbQuestion.Text = updated.Text;
            dbQuestion.ExamId = updated.ExamId;
            dbQuestion.IsActive = updated.IsActive;

           
            var keepIds = updated.Options
                .Where(o => o.Id > 0)
                .Select(o => o.Id)
                .ToHashSet();

         
            var toRemove = dbQuestion.Options
                .Where(o => o.Id > 0 && !keepIds.Contains(o.Id))
                .ToList();

            _context.Options.RemoveRange(toRemove);

           
            foreach (var opt in updated.Options)
            {
                if (opt.Id > 0)
                {
                    var dbOpt = dbQuestion.Options.FirstOrDefault(o => o.Id == opt.Id);
                    if (dbOpt != null)
                    {
                        dbOpt.Text = opt.Text;
                        dbOpt.IsCorrect = opt.IsCorrect;
                    }
                }
                else
                {
                    dbQuestion.Options.Add(new Option
                    {
                        Text = opt.Text,
                        IsCorrect = opt.IsCorrect
                    });
                }
            }

            await _context.SaveChangesAsync();
        }


        public async Task DeleteAsync(int id)
        {
            var q = await _context.Questions.FindAsync(id);
            if (q != null)
            {
                _context.Questions.Remove(q);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Topic>> GetAllTopicsAsync()
        {
            return await _context.Topics.ToListAsync();
        }
    }
}
