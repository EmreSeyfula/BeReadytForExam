using BeReadyForExam.Data;
using BeReadyForExam.Models;
using BeReadyForExam.Services.Interfaces;
using BeReadyForExam.ViewModel.Exam;
using Microsoft.EntityFrameworkCore;
using BeReadyForExam.ViewModel.Teacher;

namespace BeReadyForExam.Services.Implementations
{
    public class ExamService : IExamService
    {

      

        public ExamService(ApplicationDbContext context)
        {
            _context = context;
        }

        private async Task<ExamAttempt> GetAccessibleAttemptAsync(int attemptId, string userId, bool canAccessAllAttempts)
        {
            var attempt = await _context.ExamAttempts
                .Include(a => a.Exam)
                .FirstOrDefaultAsync(a => a.Id == attemptId);

            if (attempt == null)
                throw new InvalidOperationException("Attempt not found.");

            if (!canAccessAllAttempts && attempt.UserId != userId)
                throw new UnauthorizedAccessException("You do not have access to this attempt.");

            return attempt;
        }

        private readonly ApplicationDbContext _context;
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
            var dbExam = await _context.Exams.FirstOrDefaultAsync(e => e.Id == exam.Id);
            if (dbExam == null)
                throw new InvalidOperationException("Exam not found.");

            dbExam.Title = exam.Title;
            dbExam.TopicId = exam.TopicId;
            dbExam.QuestionsCount = exam.QuestionsCount;
            dbExam.RandomizeQuestions = exam.RandomizeQuestions;
            dbExam.IsActive = exam.IsActive;
            dbExam.Description = exam.Description;
            dbExam.TimeLimitMinutes = exam.TimeLimitMinutes;

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

            if (!questions.Any())
                throw new InvalidOperationException("This exam has no active questions.");

            if (exam.RandomizeQuestions)
            {
                questions = questions
                    .OrderBy(q => Guid.NewGuid())
                    .ToList();
            }

            if (exam.QuestionsCount > 0 && questions.Count > exam.QuestionsCount)
            {
                questions = questions
                    .Take(exam.QuestionsCount)
                    .ToList();
            }

            var attempt = new ExamAttempt
            {
                UserId = userId,
                ExamId = examId,
                StartedAt = DateTime.UtcNow,
                TotalQuestions = questions.Count
            };

            _context.ExamAttempts.Add(attempt);
            await _context.SaveChangesAsync();

            return attempt.Id;
        }


        public async Task<TakeExamViewModel> GetExamAsync(int attemptId, string userId, bool canAccessAllAttempts = false)
        {
            var attempt = await GetAccessibleAttemptAsync(attemptId, userId, canAccessAllAttempts);

            var questions = await _context.Questions
                .Where(q => q.ExamId == attempt.ExamId && q.IsActive)
                .Include(q => q.Options)
                .ToListAsync();

            if (attempt.Exam.RandomizeQuestions)
            {
                questions = questions
                    .OrderBy(q => q.Id ^ attempt.Id)
                    .ToList();
            }

            if (attempt.Exam.QuestionsCount > 0 && questions.Count > attempt.TotalQuestions)
            {
                questions = questions
                    .Take(attempt.TotalQuestions)
                    .ToList();
            }

            var deadlineUtc = attempt.Exam.TimeLimitMinutes.HasValue
                ? attempt.StartedAt.AddMinutes(attempt.Exam.TimeLimitMinutes.Value)
                : (DateTime?)null;

            var secondsRemaining = deadlineUtc.HasValue
                ? Math.Max(0, (int)(deadlineUtc.Value - DateTime.UtcNow).TotalSeconds)
                : (int?)null;

            return new TakeExamViewModel
            {
                AttemptId = attempt.Id,
                ExamId = attempt.ExamId,
                StartedAt = attempt.StartedAt,
                TimeLimitMinutes = attempt.Exam.TimeLimitMinutes,
                DeadlineUtc = deadlineUtc,
                SecondsRemaining = secondsRemaining,
                IsExpired = secondsRemaining.HasValue && secondsRemaining.Value <= 0,
                Questions = questions.Select(q => new ExamQuestionVM
                {
                    QuestionId = q.Id,
                    Text = q.Text,
                    Options = q.Options!
                        .OrderBy(o => o.Id)
                        .Select(o => new ExamOptionVM
                        {
                            OptionId = o.Id,
                            Text = o.Text
                        }).ToList()
                }).ToList()
            };
        }

        public async Task SubmitExamAsync(SubmitExamViewModel model, string userId, bool canAccessAllAttempts = false)
        {
            var attempt = await GetAccessibleAttemptAsync(model.AttemptId, userId, canAccessAllAttempts);

            if (attempt.FinishedAt.HasValue)
                return;

            var exam = attempt.Exam;

            if (exam.TimeLimitMinutes.HasValue)
            {
                var deadlineUtc = attempt.StartedAt.AddMinutes(exam.TimeLimitMinutes.Value);
                if (DateTime.UtcNow > deadlineUtc)
                {
                    attempt.FinishedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return;
                }
            }

            var existingAnswers = await _context.AttemptAnswers
                .Where(a => a.ExamAttemptId == attempt.Id)
                .ToListAsync();

            if (existingAnswers.Any())
            {
                _context.AttemptAnswers.RemoveRange(existingAnswers);
            }

            int correct = 0;

            foreach (var answer in model.Answers)
            {
                var option = await _context.Options
                    .FirstOrDefaultAsync(o => o.Id == answer.SelectedOptionId);

                if (option == null)
                    continue;

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
            attempt.ScorePercent = attempt.TotalQuestions == 0 ? 0 : (double)correct / attempt.TotalQuestions * 100;
            attempt.Grade = 2 + (attempt.ScorePercent / 100.0) * 4;
            attempt.FinishedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task<ExamResultViewModel> GetResultAsync(int attemptId, string userId, bool canAccessAllAttempts = false)
        {

            var attempt = await GetAccessibleAttemptAsync(attemptId, userId, canAccessAllAttempts);
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
                  .OrderByDescending(a => a.StartedAt)
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

        public async Task<TeacherDashboardViewModel> GetTeacherDashboardAsync()
        {
            var totalExams = await _context.Exams.CountAsync();
            var activeExams = await _context.Exams.CountAsync(e => e.IsActive);
            var totalQuestions = await _context.Questions.CountAsync();
            var totalAttempts = await _context.ExamAttempts.CountAsync();
            var completedAttempts = await _context.ExamAttempts.CountAsync(a => a.FinishedAt != null);

            var averageScorePercent = await _context.ExamAttempts
                .Where(a => a.FinishedAt != null)
                .Select(a => (double?)a.ScorePercent)
                .AverageAsync() ?? 0;

            var recentAttempts = await _context.ExamAttempts
                .Include(a => a.Exam)
                .OrderByDescending(a => a.StartedAt)
                .Take(10)
                .Select(a => new TeacherRecentAttemptVM
                {
                    AttemptId = a.Id,
                    ExamTitle = a.Exam.Title,
                    UserId = a.UserId,
                    TotalQuestions = a.TotalQuestions,
                    CorrectCount = a.CorrectCount,
                    ScorePercent = a.ScorePercent,
                    StartedAt = a.StartedAt,
                    FinishedAt = a.FinishedAt
                })
                .ToListAsync();

            return new TeacherDashboardViewModel
            {
                TotalExams = totalExams,
                ActiveExams = activeExams,
                TotalQuestions = totalQuestions,
                TotalAttempts = totalAttempts,
                CompletedAttempts = completedAttempts,
                AverageScorePercent = averageScorePercent,
                RecentAttempts = recentAttempts
            };
        }

    }
}