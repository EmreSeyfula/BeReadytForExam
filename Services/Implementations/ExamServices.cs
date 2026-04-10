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
                .Include(a => a.AttemptAnswers)
                .FirstOrDefaultAsync(a => a.Id == attemptId);

            if (attempt == null)
                throw new InvalidOperationException("Опитът не е намерен.");

            if (!canAccessAllAttempts && attempt.UserId != userId)
                throw new UnauthorizedAccessException("Нямате достъп до този опит.");

            return attempt;
        }

        private readonly ApplicationDbContext _context;

        private static string BuildQuestionSnapshot(IEnumerable<int> questionIds)
            => string.Join(",", questionIds);

        private static List<int> ParseQuestionSnapshot(string? snapshot)
        {
            if (string.IsNullOrWhiteSpace(snapshot))
            {
                return new List<int>();
            }

            return snapshot
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(id => int.TryParse(id, out var parsedId) ? parsedId : 0)
                .Where(id => id > 0)
                .Distinct()
                .ToList();
        }

        private async Task<List<int>> GetAttemptQuestionIdsAsync(ExamAttempt attempt)
        {
            var snapshotIds = ParseQuestionSnapshot(attempt.QuestionSnapshot);
            if (snapshotIds.Count > 0)
            {
                return snapshotIds;
            }

            var answeredQuestionIds = attempt.AttemptAnswers?
                .Select(a => a.QuestionId)
                .Distinct()
                .ToList();

            if (answeredQuestionIds is { Count: > 0 })
            {
                return answeredQuestionIds;
            }

            var questions = await _context.Questions
                .Where(q => q.ExamId == attempt.ExamId && q.IsActive)
                .Select(q => q.Id)
                .ToListAsync();

            questions = attempt.Exam.RandomizeQuestions
                ? questions.OrderBy(id => id ^ attempt.Id).ToList()
                : questions.OrderBy(id => id).ToList();

            return attempt.TotalQuestions > 0
                ? questions.Take(attempt.TotalQuestions).ToList()
                : questions;
        }

        private static void NormalizeEditorModel(ExamEditorViewModel model)
        {
            model.Title = model.Title?.Trim() ?? string.Empty;
            model.Description = string.IsNullOrWhiteSpace(model.Description)
                ? null
                : model.Description.Trim();
            model.Questions ??= new List<ExamQuestionEditorViewModel>();

            foreach (var question in model.Questions)
            {
                question.Text = question.Text?.Trim() ?? string.Empty;
                question.Options = (question.Options ?? new List<ExamOptionEditorViewModel>())
                    .Where(option => !string.IsNullOrWhiteSpace(option.Text))
                    .Select(option => new ExamOptionEditorViewModel
                    {
                        Id = option.Id,
                        Text = option.Text.Trim(),
                        IsCorrect = option.IsCorrect
                    })
                    .ToList();
            }
        }

        private static void ValidateEditorModelOrThrow(ExamEditorViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Title))
                throw new InvalidOperationException("Полето „Заглавие“ е задължително.");

            if (model.TopicId <= 0)
                throw new InvalidOperationException("Моля, изберете тема.");

            if (model.Questions == null || model.Questions.Count == 0)
                throw new InvalidOperationException("Добавете поне един въпрос към теста.");

            foreach (var question in model.Questions)
            {
                if (string.IsNullOrWhiteSpace(question.Text))
                    throw new InvalidOperationException("Всеки въпрос трябва да има текст.");

                if (question.Options.Count < 2)
                    throw new InvalidOperationException($"Въпросът „{question.Text}“ трябва да има поне 2 отговора.");

                if (!question.Options.Any(option => option.IsCorrect))
                    throw new InvalidOperationException($"Въпросът „{question.Text}“ трябва да има поне един верен отговор.");
            }
        }

        private static ExamQuestionEditorViewModel CreateDefaultQuestion()
        {
            return new ExamQuestionEditorViewModel
            {
                IsActive = true,
                Options = new List<ExamOptionEditorViewModel>
                {
                    new(),
                    new()
                }
            };
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
                .OrderBy(e => e.Topic != null ? e.Topic.Name : string.Empty)
                .ThenBy(e => e.Title)
                .ToListAsync();
        }

        public async Task<Exam?> GetByIdAsync(int id)
        {
            return await _context.Exams
                .Include(e => e.Topic)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<ExamEditorViewModel?> GetEditorByIdAsync(int id)
        {
            var exam = await _context.Exams
                .Include(e => e.Topic)
                .ThenInclude(t => t!.Subject)
                .Include(e => e.Questions)
                .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (exam == null)
            {
                return null;
            }

            var model = new ExamEditorViewModel
            {
                Id = exam.Id,
                Title = exam.Title,
                Description = exam.Description,
                SubjectId = exam.Topic?.SubjectId,
                TopicId = exam.TopicId,
                QuestionsCount = exam.QuestionsCount,
                TimeLimitMinutes = exam.TimeLimitMinutes,
                RandomizeQuestions = exam.RandomizeQuestions,
                IsActive = exam.IsActive,
                Questions = exam.Questions
                    .OrderBy(q => q.Id)
                    .Select(q => new ExamQuestionEditorViewModel
                    {
                        Id = q.Id,
                        Text = q.Text,
                        IsActive = q.IsActive,
                        Options = q.Options
                            .OrderBy(o => o.Id)
                            .Select(o => new ExamOptionEditorViewModel
                            {
                                Id = o.Id,
                                Text = o.Text,
                                IsCorrect = o.IsCorrect
                            })
                            .ToList()
                    })
                    .ToList()
            };

            if (model.Questions.Count == 0)
            {
                model.Questions.Add(CreateDefaultQuestion());
            }

            return model;
        }

        public async Task<int> CreateAsync(Exam exam)
        {
            _context.Exams.Add(exam);
            await _context.SaveChangesAsync();
            return exam.Id;
        }

        public async Task<int> CreateWithQuestionsAsync(ExamEditorViewModel model)
        {
            NormalizeEditorModel(model);
            ValidateEditorModelOrThrow(model);

            var exam = new Exam
            {
                Title = model.Title,
                Description = model.Description,
                TopicId = model.TopicId,
                QuestionsCount = model.QuestionsCount,
                TimeLimitMinutes = model.TimeLimitMinutes,
                RandomizeQuestions = model.RandomizeQuestions,
                IsActive = model.IsActive,
                CreatedAt = DateTime.UtcNow,
                Questions = model.Questions.Select(question => new Question
                {
                    Text = question.Text,
                    IsActive = question.IsActive,
                    Options = question.Options.Select(option => new Option
                    {
                        Text = option.Text,
                        IsCorrect = option.IsCorrect
                    }).ToList()
                }).ToList()
            };

            _context.Exams.Add(exam);
            await _context.SaveChangesAsync();
            return exam.Id;
        }

        public async Task UpdateAsync(Exam exam)
        {
            var dbExam = await _context.Exams.FirstOrDefaultAsync(e => e.Id == exam.Id);
            if (dbExam == null)
                throw new InvalidOperationException("Тестът не е намерен.");

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
                throw new InvalidOperationException("Тестът не е намерен.");

            var questions = await _context.Questions
                .Where(q => q.ExamId == examId && q.IsActive)
                .ToListAsync();

            if (!questions.Any())
                throw new InvalidOperationException("Този тест няма активни въпроси.");

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

            var selectedQuestionIds = questions
                .Select(q => q.Id)
                .ToList();

            var attempt = new ExamAttempt
            {
                UserId = userId,
                ExamId = examId,
                StartedAt = DateTime.UtcNow,
                QuestionSnapshot = BuildQuestionSnapshot(selectedQuestionIds),
                TotalQuestions = selectedQuestionIds.Count
            };

            _context.ExamAttempts.Add(attempt);
            await _context.SaveChangesAsync();

            return attempt.Id;
        }


        public async Task<TakeExamViewModel> GetExamAsync(int attemptId, string userId, bool canAccessAllAttempts = false)
        {
            var attempt = await GetAccessibleAttemptAsync(attemptId, userId, canAccessAllAttempts);
            var questionIds = await GetAttemptQuestionIdsAsync(attempt);

            var questions = await _context.Questions
                .Where(q => questionIds.Contains(q.Id))
                .Include(q => q.Options)
                .ToListAsync();

            var questionOrder = questionIds
                .Select((id, index) => new { id, index })
                .ToDictionary(x => x.id, x => x.index);

            questions = questions
                .OrderBy(q => questionOrder.TryGetValue(q.Id, out var index) ? index : int.MaxValue)
                .ToList();

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
            var questionIds = await GetAttemptQuestionIdsAsync(attempt);

            if (exam.TimeLimitMinutes.HasValue)
            {
                var deadlineUtc = attempt.StartedAt.AddMinutes(exam.TimeLimitMinutes.Value);
                if (DateTime.UtcNow > deadlineUtc)
                {
                    attempt.FinishedAt = deadlineUtc;
                }
            }

            var allowedQuestions = await _context.Questions
                .Where(q => questionIds.Contains(q.Id))
                .Select(q => new
                {
                    q.Id,
                    OptionIds = q.Options.Select(o => new { o.Id, o.IsCorrect }).ToList()
                })
                .ToListAsync();

            var allowedQuestionMap = allowedQuestions.ToDictionary(q => q.Id);

            var existingAnswers = await _context.AttemptAnswers
                .Where(a => a.ExamAttemptId == attempt.Id)
                .ToListAsync();

            if (existingAnswers.Any())
            {
                _context.AttemptAnswers.RemoveRange(existingAnswers);
            }

            int correct = 0;

            var submittedAnswers = (model.Answers ?? new List<ExamAnswerInputVM>())
                .GroupBy(a => a.QuestionId)
                .Select(g => g.First())
                .ToList();

            foreach (var answer in submittedAnswers)
            {
                if (!allowedQuestionMap.TryGetValue(answer.QuestionId, out var allowedQuestion))
                    continue;

                var option = allowedQuestion.OptionIds.FirstOrDefault(o => o.Id == answer.SelectedOptionId);
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

            attempt.TotalQuestions = questionIds.Count;
            attempt.CorrectCount = correct;
            attempt.ScorePercent = attempt.TotalQuestions == 0 ? 0 : (double)correct / attempt.TotalQuestions * 100;
            attempt.Grade = 2 + (attempt.ScorePercent / 100.0) * 4;
            attempt.FinishedAt ??= DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task UpdateWithQuestionsAsync(ExamEditorViewModel model)
        {
            NormalizeEditorModel(model);
            ValidateEditorModelOrThrow(model);

            var exam = await _context.Exams
                .Include(e => e.Questions)
                .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(e => e.Id == model.Id);

            if (exam == null)
                throw new InvalidOperationException("Тестът не е намерен.");

            exam.Title = model.Title;
            exam.Description = model.Description;
            exam.TopicId = model.TopicId;
            exam.QuestionsCount = model.QuestionsCount;
            exam.TimeLimitMinutes = model.TimeLimitMinutes;
            exam.RandomizeQuestions = model.RandomizeQuestions;
            exam.IsActive = model.IsActive;

            var questionMap = exam.Questions.ToDictionary(q => q.Id);

            foreach (var questionModel in model.Questions)
            {
                if (questionModel.Id > 0)
                {
                    if (!questionMap.TryGetValue(questionModel.Id, out var existingQuestion))
                        throw new InvalidOperationException("Невалиден въпрос в редактора.");

                    existingQuestion.Text = questionModel.Text;
                    existingQuestion.IsActive = questionModel.IsActive;

                    var optionMap = existingQuestion.Options.ToDictionary(o => o.Id);

                    foreach (var optionModel in questionModel.Options)
                    {
                        if (optionModel.Id > 0)
                        {
                            if (!optionMap.TryGetValue(optionModel.Id, out var existingOption))
                                throw new InvalidOperationException("Невалиден отговор във въпроса.");

                            existingOption.Text = optionModel.Text;
                            existingOption.IsCorrect = optionModel.IsCorrect;
                        }
                        else
                        {
                            existingQuestion.Options.Add(new Option
                            {
                                Text = optionModel.Text,
                                IsCorrect = optionModel.IsCorrect
                            });
                        }
                    }

                    var keptOptionIds = questionModel.Options
                        .Where(o => o.Id > 0)
                        .Select(o => o.Id)
                        .ToHashSet();

                    var optionsToRemove = existingQuestion.Options
                        .Where(o => o.Id > 0 && !keptOptionIds.Contains(o.Id))
                        .ToList();

                    if (optionsToRemove.Count > 0)
                    {
                        var optionIdsToRemove = optionsToRemove.Select(o => o.Id).ToList();
                        var usedOptionExists = await _context.AttemptAnswers
                            .AnyAsync(answer => optionIdsToRemove.Contains(answer.SelectedOptionId));

                        if (usedOptionExists)
                            throw new InvalidOperationException("Не можеш да изтриеш отговор, който вече е използван в решен тест.");

                        _context.Options.RemoveRange(optionsToRemove);
                    }
                }
                else
                {
                    exam.Questions.Add(new Question
                    {
                        Text = questionModel.Text,
                        IsActive = questionModel.IsActive,
                        Options = questionModel.Options.Select(option => new Option
                        {
                            Text = option.Text,
                            IsCorrect = option.IsCorrect
                        }).ToList()
                    });
                }
            }

            var keptQuestionIds = model.Questions
                .Where(q => q.Id > 0)
                .Select(q => q.Id)
                .ToHashSet();

            var questionsToRemove = exam.Questions
                .Where(q => q.Id > 0 && !keptQuestionIds.Contains(q.Id))
                .ToList();

            if (questionsToRemove.Count > 0)
            {
                var questionIdsToRemove = questionsToRemove.Select(q => q.Id).ToList();
                var usedQuestionExists = await _context.AttemptAnswers
                    .AnyAsync(answer => questionIdsToRemove.Contains(answer.QuestionId));

                if (usedQuestionExists)
                    throw new InvalidOperationException("Не можеш да изтриеш въпрос, който вече е използван в решен тест.");

                _context.Questions.RemoveRange(questionsToRemove);
            }

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
                    TopicName = e.Topic != null ? e.Topic.Name : "Без тема",
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
