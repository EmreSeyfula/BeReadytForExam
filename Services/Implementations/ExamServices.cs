using BeReadyForExam.Data;
using BeReadyForExam.Models;
using BeReadyForExam.Services.Interfaces;
using BeReadyForExam.ViewModel.Exam;
using Microsoft.EntityFrameworkCore;

namespace BeReadyForExam.Services.Implementations
{
    public class ExamService : IExamService
    {
        private readonly ApplicationDbContext _context;

        public ExamService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Exam>> GetAllAsync()
        {
            return await _context.Exams
                .Include(e => e.Topic)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();
        }
        public async Task<List<Exam>> GetAvailableExamsAsync()
        {
            return await _context.Exams
                .Include(e => e.Topic)
                .Where(e => e.IsActive)
                .OrderBy(e => e.Topic.Name)
                .ThenBy(e => e.Title)
                .ToListAsync();
        }

        public async Task<Exam> GetByIdAsync(int id)
        {
            return await _context.Exams
                .Include(e => e.Topic)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<int> CreateAsync(Exam exam)
        {
            _context.Exams.Add(exam);
            await _context.SaveChangesAsync();
            return exam.Id;
        }

        public async Task UpdateAsync(Exam exam)
        {
            _context.Exams.Update(exam);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var exam = await _context.Exams.FindAsync(id);
            if (exam != null)
            {
                _context.Exams.Remove(exam);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Exam>> GetAllActiveExamsAsync()
        {
            return await _context.Exams
                .Where(e => e.IsActive)
                .OrderBy(e => e.Title)
                .ToListAsync();
        }

        public async Task<int> StartExamAsync(int examId, string userId)
        {
            var exam = await _context.Exams
                .FirstOrDefaultAsync(e => e.Id == examId && e.IsActive);

            if (exam == null)
                throw new InvalidOperationException("Exam not found.");

            var questions = await _context.Questions
                .Where(q => q.ExamId == examId && q.IsActive)
                .ToListAsync();

            var attempt = new ExamAttempt
            {
                UserId = userId,
                ExamId = examId,
                TotalQuestions = questions.Count
            };

            _context.ExamAttempts.Add(attempt);
            await _context.SaveChangesAsync();

            return attempt.Id;
        }

        public async Task<TakeExamViewModel> GetExamAsync(int attemptId)
        {
            var attempt = await _context.ExamAttempts
                .Include(a => a.Exam)
                .FirstOrDefaultAsync(a => a.Id == attemptId);

            var questions = await _context.Questions
                .Where(q => q.ExamId == attempt.ExamId)
                .Include(q => q.Options)
                .ToListAsync();

            return new TakeExamViewModel
            {
                AttemptId = attempt.Id,
                ExamId = attempt.ExamId,
                Questions = questions.Select(q => new ExamQuestionVM
                {
                    QuestionId = q.Id,
                    Text = q.Text,
                    Options = q.Options.Select(o => new ExamOptionVM
                    {
                        OptionId = o.Id,
                        Text = o.Text
                    }).ToList()
                }).ToList()
            };
        }

        public async Task SubmitExamAsync(SubmitExamViewModel model)
        {
            var attempt = await _context.ExamAttempts
                .FirstOrDefaultAsync(a => a.Id == model.AttemptId);

            int correct = 0;

            foreach (var answer in model.Answers)
            {
                var option = await _context.Options
                    .FirstOrDefaultAsync(o => o.Id == answer.SelectedOptionId);

                var attemptAnswer = new AttemptAnswer
                {
                    ExamAttemptId = attempt.Id,
                    QuestionId = answer.QuestionId,
                    SelectedOptionId = answer.SelectedOptionId,
                    IsCorrect = option.IsCorrect
                };

                if (option.IsCorrect)
                    correct++;

                _context.AttemptAnswers.Add(attemptAnswer);
            }

            attempt.CorrectCount = correct;
            attempt.ScorePercent = (double)correct / attempt.TotalQuestions * 100;
            attempt.Grade = 2 + (attempt.ScorePercent / 100) * 4;
            attempt.FinishedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task<ExamResultViewModel> GetResultAsync(int attemptId, string userId)
        {
            var attempt = await _context.ExamAttempts
                .Include(a => a.Exam)
                .FirstOrDefaultAsync(a => a.Id == attemptId);

            return new ExamResultViewModel
            {
                AttemptId = attempt.Id,
                TopicName = attempt.Exam.Title,
                TotalQuestions = attempt.TotalQuestions,
                CorrectCount = attempt.CorrectCount,
                ScorePercent = attempt.ScorePercent,
                Grade = attempt.Grade,
                StartedAt = attempt.StartedAt,
                FinishedAt = attempt.FinishedAt
            };
        }

        public async Task<MyExamHistoryViewModel> GetMyHistoryAsync(string userId)
        {
            var attempts = await _context.ExamAttempts
                .Include(a => a.Exam)
                .Where(a => a.UserId == userId)
                .ToListAsync();

            return new MyExamHistoryViewModel
            {
                Attempts = attempts.Select(a => new MyExamAttemptRowVM
                {
                    AttemptId = a.Id,
                    TopicName = a.Exam.Title,
                    StartedAt = a.StartedAt,
                    FinishedAt = a.FinishedAt,
                    TotalQuestions = a.TotalQuestions,
                    CorrectCount = a.CorrectCount,
                    ScorePercent = a.ScorePercent,
                    Grade = a.Grade
                }).ToList()
            };
        }

        public async Task<List<TeacherExamRowViewModel>> GetTeacherExamListAsync()
        {
            return await _context.Exams
                .Include(e => e.Topic)
                .OrderByDescending(e => e.CreatedAt)
                .Select(e => new TeacherExamRowViewModel
                {
                    Id = e.Id,
                    Title = e.Title,
                    TopicName = e.Topic.Name,
                    IsActive = e.IsActive,
                    ConfiguredQuestionsCount = e.QuestionsCount,
                    ActualQuestionsInBank = _context.Questions.Count(q => q.ExamId == e.Id)
                })
                .ToListAsync();
        }

    }
}