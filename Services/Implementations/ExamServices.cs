using BeReadyForExam.Data;
using BeReadyForExam.Models;
using BeReadyForExam.Services.Interfaces;
using BeReadyForExam.ViewModel.Exam;
using Microsoft.EntityFrameworkCore;

namespace BeReadyForExam.Services.Implementations
{
    public class ExamServices
    {
        public class ExamService : IExamService
        {
            private readonly ApplicationDbContext _context;

            public ExamService(ApplicationDbContext context)
            {
                _context = context;
            }

            public async Task<int> StartExamAsync(int topicId, string userId)
            {
                var questions = await _context.Questions
                    .Where(q => q.TopicId == topicId)
                    .ToListAsync();

                var attempt = new ExamAttempt
                {
                    UserId = userId,
                    TopicId = topicId,
                    TotalQuestions = questions.Count
                };

                _context.ExamAttempts.Add(attempt);
                await _context.SaveChangesAsync();

                return attempt.Id;
            }

            public async Task<TakeExamViewModel> GetExamAsync(int attemptId)
            {
                var attempt = await _context.ExamAttempts.FindAsync(attemptId);

                var questions = await _context.Questions
                    .Where(q => q.TopicId == attempt.TopicId)
                    .Include(q => q.Options)
                    .ToListAsync();

                return new TakeExamViewModel
                {
                    AttemptId = attemptId,
                    TopicId = attempt.TopicId,
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
                    var option = await _context.Options.FindAsync(answer.SelectedOptionId);

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
                attempt.FinishedAt = DateTime.UtcNow;
                attempt.ScorePercent = (double)correct / attempt.TotalQuestions * 100;
                attempt.Grade = 2 + (attempt.ScorePercent / 100) * 4;

                await _context.SaveChangesAsync();
            }

            public async Task<ExamResultViewModel> GetResultAsync(int attemptId, string userId)
            {
                var attempt = await _context.ExamAttempts
                    .Include(a => a.Topic)
                    .FirstOrDefaultAsync(a => a.Id == attemptId);

                if (attempt == null)
                    throw new InvalidOperationException("Attempt not found.");

                
                if (attempt.UserId != userId)
                    throw new UnauthorizedAccessException("Not allowed.");

                var answers = await _context.AttemptAnswers
                    .Where(x => x.ExamAttemptId == attemptId)
                    .Include(x => x.Question)
                    .Include(x => x.SelectedOption)
                    .ToListAsync();

              
                var wrong = new List<WrongAnswerVM>();

                foreach (var a in answers.Where(x => !x.IsCorrect))
                {
                    var correctOptionText = await _context.Options
                        .Where(o => o.QuestionId == a.QuestionId && o.IsCorrect)
                        .Select(o => o.Text)
                        .FirstOrDefaultAsync();

                    wrong.Add(new WrongAnswerVM
                    {
                        QuestionText = a.Question?.Text ?? "",
                        SelectedOptionText = a.SelectedOption?.Text ?? "",
                        CorrectOptionText = correctOptionText ?? ""
                    });
                }

                return new ExamResultViewModel
                {
                    AttemptId = attempt.Id,
                    TopicName = attempt.Topic?.Name ?? "",
                    TotalQuestions = attempt.TotalQuestions,
                    CorrectCount = attempt.CorrectCount,
                    ScorePercent = attempt.ScorePercent,
                    Grade = attempt.Grade,
                    StartedAt = attempt.StartedAt,
                    FinishedAt = attempt.FinishedAt,
                    WrongAnswers = wrong
                };
            }

            public async Task<MyExamHistoryViewModel> GetMyHistoryAsync(string userId)
            {
                var attempts = await _context.ExamAttempts
                    .Include(a => a.Topic)
                    .Where(a => a.UserId == userId)
                    .OrderByDescending(a => a.StartedAt)
                    .ToListAsync();

                return new MyExamHistoryViewModel
                {
                    Attempts = attempts.Select(a => new MyExamAttemptRowVM
                    {
                        AttemptId = a.Id,
                        TopicName = a.Topic?.Name ?? "",
                        StartedAt = a.StartedAt,
                        FinishedAt = a.FinishedAt,
                        TotalQuestions = a.TotalQuestions,
                        CorrectCount = a.CorrectCount,
                        ScorePercent = a.ScorePercent,
                        Grade = a.Grade
                    }).ToList()
                };
            }
        }
    }
}
