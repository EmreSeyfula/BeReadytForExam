using BeReadyForExam.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BeReadyForExam.Data
{
    public static class AppDataSeed
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

            await db.Database.MigrateAsync();

            if (await db.Subjects.AnyAsync() ||
               await db.Topics.AnyAsync() ||
               await db.Exams.AnyAsync() ||
               await db.Questions.AnyAsync())
            {
                return;
            }
            var admin = await userManager.FindByEmailAsync("admin@school.local");
            var teacher = await userManager.FindByEmailAsync("teacher@school.local");
            var student = await userManager.FindByEmailAsync("student@school.local");

            if (student == null)
            {
                throw new InvalidOperationException("Липсва student@school.local. Първо пусни identity seed.");
            }

            
            var math = new Subject
            {
                Name = "Математика",
                Description = "Подготовка по алгебра, геометрия и тригонометрия."
            };

            var bulgarian = new Subject
            {
                Name = "Български език и литература",
                Description = "Подготовка по граматика, литература и анализ на текст."
            };

            var programming = new Subject
            {
                Name = "Програмиране",
                Description = "Подготовка по C#, бази данни, алгоритми и ООП."
            };

            db.Subjects.AddRange(math, bulgarian, programming);
            await db.SaveChangesAsync();

        
            var topics = new List<Topic>
            {
                new Topic
                {
                    SubjectId = math.Id,
                    Name = "Квадратни уравнения",
                    IsActive = true
                },
                new Topic
                {
                    SubjectId = math.Id,
                    Name = "Тригонометрия",
                    IsActive = true
                },
                new Topic
                {
                    SubjectId = bulgarian.Id,
                    Name = "Литература",
                    IsActive = true
                },
                new Topic
                {
                    SubjectId = bulgarian.Id,
                    Name = "Граматика",
                    IsActive = true
                },
                new Topic
                {
                    SubjectId = programming.Id,
                    Name = "C# основи",
                    IsActive = true
                },
                new Topic
                {
                    SubjectId = programming.Id,
                    Name = "LINQ",
                    IsActive = true
                }
            };

            db.Topics.AddRange(topics);
            await db.SaveChangesAsync();

            var quadraticTopic = topics.First(t => t.Name == "Квадратни уравнения");
            var trigTopic = topics.First(t => t.Name == "Тригонометрия");
            var literatureTopic = topics.First(t => t.Name == "Литература");
            var grammarTopic = topics.First(t => t.Name == "Граматика");
            var csharpTopic = topics.First(t => t.Name == "C# основи");
            var linqTopic = topics.First(t => t.Name == "LINQ");

          
            var exams = new List<Exam>
            {
                new Exam
                {
                    TopicId = quadraticTopic.Id,
                    Title = "Тест: Квадратни уравнения",
                    Description = "Основен тест върху формули, дискриминанта и корени.",
                    IsActive = true,
                    TimeLimitMinutes = 20,
                    QuestionsCount = 3,
                    RandomizeQuestions = false,
                    CreatedAt = DateTime.UtcNow.AddDays(-10)
                },
                new Exam
                {
                    TopicId = trigTopic.Id,
                    Title = "Тест: Тригонометрия",
                    Description = "Тест върху тъждества и стойности на ъгли.",
                    IsActive = true,
                    TimeLimitMinutes = 20,
                    QuestionsCount = 3,
                    RandomizeQuestions = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-9)
                },
                new Exam
                {
                    TopicId = literatureTopic.Id,
                    Title = "Тест: Литература",
                    Description = "Разпознаване на автори, жанрове и произведения.",
                    IsActive = true,
                    TimeLimitMinutes = 15,
                    QuestionsCount = 3,
                    RandomizeQuestions = false,
                    CreatedAt = DateTime.UtcNow.AddDays(-8)
                },
                new Exam
                {
                    TopicId = grammarTopic.Id,
                    Title = "Тест: Граматика",
                    Description = "Правопис, части на речта и синтаксис.",
                    IsActive = true,
                    TimeLimitMinutes = 15,
                    QuestionsCount = 3,
                    RandomizeQuestions = false,
                    CreatedAt = DateTime.UtcNow.AddDays(-7)
                },
                new Exam
                {
                    TopicId = csharpTopic.Id,
                    Title = "Тест: C# основи",
                    Description = "Типове данни, цикли, условни конструкции.",
                    IsActive = true,
                    TimeLimitMinutes = 25,
                    QuestionsCount = 3,
                    RandomizeQuestions = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-6)
                },
                new Exam
                {
                    TopicId = linqTopic.Id,
                    Title = "Тест: LINQ",
                    Description = "Основни LINQ операции и заявки.",
                    IsActive = true,
                    TimeLimitMinutes = 25,
                    QuestionsCount = 3,
                    RandomizeQuestions = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                }
            };

            db.Exams.AddRange(exams);
            await db.SaveChangesAsync();

            var quadraticExam = exams.First(e => e.Title == "Тест: Квадратни уравнения");
            var trigExam = exams.First(e => e.Title == "Тест: Тригонометрия");
            var literatureExam = exams.First(e => e.Title == "Тест: Литература");
            var grammarExam = exams.First(e => e.Title == "Тест: Граматика");
            var csharpExam = exams.First(e => e.Title == "Тест: C# основи");
            var linqExam = exams.First(e => e.Title == "Тест: LINQ");

           
            var questions = new List<Question>
            {
                new Question
                {
                    ExamId = quadraticExam.Id,
                    Text = "Колко корена има уравнението x² - 5x + 6 = 0?",
                    IsActive = true,
                    Options = new List<Option>
                    {
                        new Option { Text = "0", IsCorrect = false },
                        new Option { Text = "1", IsCorrect = false },
                        new Option { Text = "2", IsCorrect = true }
                    }
                },
                new Question
                {
                    ExamId = quadraticExam.Id,
                    Text = "Кой е дискриминантът на x² + 2x + 1 = 0?",
                    IsActive = true,
                    Options = new List<Option>
                    {
                        new Option { Text = "0", IsCorrect = true },
                        new Option { Text = "2", IsCorrect = false },
                        new Option { Text = "4", IsCorrect = false }
                    }
                },
                new Question
                {
                    ExamId = quadraticExam.Id,
                    Text = "Кои са корените на x² - 9 = 0?",
                    IsActive = true,
                    Options = new List<Option>
                    {
                        new Option { Text = "x = 3", IsCorrect = false },
                        new Option { Text = "x = -3 и x = 3", IsCorrect = true },
                        new Option { Text = "Няма реални корени", IsCorrect = false }
                    }
                },

                new Question
                {
                    ExamId = trigExam.Id,
                    Text = "На какво е равно sin 90°?",
                    IsActive = true,
                    Options = new List<Option>
                    {
                        new Option { Text = "0", IsCorrect = false },
                        new Option { Text = "1", IsCorrect = true },
                        new Option { Text = "-1", IsCorrect = false }
                    }
                },
                new Question
                {
                    ExamId = trigExam.Id,
                    Text = "На какво е равно cos 0°?",
                    IsActive = true,
                    Options = new List<Option>
                    {
                        new Option { Text = "1", IsCorrect = true },
                        new Option { Text = "0", IsCorrect = false },
                        new Option { Text = "-1", IsCorrect = false }
                    }
                },
                new Question
                {
                    ExamId = trigExam.Id,
                    Text = "Кое тъждество е вярно?",
                    IsActive = true,
                    Options = new List<Option>
                    {
                        new Option { Text = "sin²x + cos²x = 1", IsCorrect = true },
                        new Option { Text = "sin x + cos x = 1", IsCorrect = false },
                        new Option { Text = "tan x = sin x · cos x", IsCorrect = false }
                    }
                },

                new Question
                {
                    ExamId = literatureExam.Id,
                    Text = "Кой е авторът на „Под игото“?",
                    IsActive = true,
                    Options = new List<Option>
                    {
                        new Option { Text = "Иван Вазов", IsCorrect = true },
                        new Option { Text = "Елин Пелин", IsCorrect = false },
                        new Option { Text = "Христо Ботев", IsCorrect = false }
                    }
                },
                new Question
                {
                    ExamId = literatureExam.Id,
                    Text = "„Бай Ганьо“ е произведение на:",
                    IsActive = true,
                    Options = new List<Option>
                    {
                        new Option { Text = "Алеко Константинов", IsCorrect = true },
                        new Option { Text = "Йордан Йовков", IsCorrect = false },
                        new Option { Text = "Иван Вазов", IsCorrect = false }
                    }
                },
                new Question
                {
                    ExamId = literatureExam.Id,
                    Text = "Какъв жанр е „Немили-недраги“?",
                    IsActive = true,
                    Options = new List<Option>
                    {
                        new Option { Text = "Повест", IsCorrect = true },
                        new Option { Text = "Ода", IsCorrect = false },
                        new Option { Text = "Разказ", IsCorrect = false }
                    }
                },

                new Question
                {
                    ExamId = grammarExam.Id,
                    Text = "Коя част на речта е думата „бързо“ в изречението „Той бяга бързо.“?",
                    IsActive = true,
                    Options = new List<Option>
                    {
                        new Option { Text = "Прилагателно име", IsCorrect = false },
                        new Option { Text = "Наречие", IsCorrect = true },
                        new Option { Text = "Съществително име", IsCorrect = false }
                    }
                },
                new Question
                {
                    ExamId = grammarExam.Id,
                    Text = "Кое е правилно изписано?",
                    IsActive = true,
                    Options = new List<Option>
                    {
                        new Option { Text = "незнам", IsCorrect = false },
                        new Option { Text = "не знам", IsCorrect = true },
                        new Option { Text = "нез нам", IsCorrect = false }
                    }
                },
                new Question
                {
                    ExamId = grammarExam.Id,
                    Text = "Кое изречение е съобщително?",
                    IsActive = true,
                    Options = new List<Option>
                    {
                        new Option { Text = "Колко е часът?", IsCorrect = false },
                        new Option { Text = "Затвори вратата!", IsCorrect = false },
                        new Option { Text = "Днес времето е хубаво.", IsCorrect = true }
                    }
                },

                new Question
                {
                    ExamId = csharpExam.Id,
                    Text = "Кой тип се използва за текст в C#?",
                    IsActive = true,
                    Options = new List<Option>
                    {
                        new Option { Text = "int", IsCorrect = false },
                        new Option { Text = "string", IsCorrect = true },
                        new Option { Text = "bool", IsCorrect = false }
                    }
                },
                new Question
                {
                    ExamId = csharpExam.Id,
                    Text = "Кой цикъл се използва, когато броят повторения е известен?",
                    IsActive = true,
                    Options = new List<Option>
                    {
                        new Option { Text = "for", IsCorrect = true },
                        new Option { Text = "try", IsCorrect = false },
                        new Option { Text = "switch", IsCorrect = false }
                    }
                },
                new Question
                {
                    ExamId = csharpExam.Id,
                    Text = "Как се пише условна конструкция в C#?",
                    IsActive = true,
                    Options = new List<Option>
                    {
                        new Option { Text = "if", IsCorrect = true },
                        new Option { Text = "loop", IsCorrect = false },
                        new Option { Text = "caseonly", IsCorrect = false }
                    }
                },

                new Question
                {
                    ExamId = linqExam.Id,
                    Text = "Кой метод филтрира елементи в LINQ?",
                    IsActive = true,
                    Options = new List<Option>
                    {
                        new Option { Text = "Select", IsCorrect = false },
                        new Option { Text = "Where", IsCorrect = true },
                        new Option { Text = "OrderBy", IsCorrect = false }
                    }
                },
                new Question
                {
                    ExamId = linqExam.Id,
                    Text = "Кой метод сортира във възходящ ред?",
                    IsActive = true,
                    Options = new List<Option>
                    {
                        new Option { Text = "GroupBy", IsCorrect = false },
                        new Option { Text = "OrderBy", IsCorrect = true },
                        new Option { Text = "FirstOrDefault", IsCorrect = false }
                    }
                },
                new Question
                {
                    ExamId = linqExam.Id,
                    Text = "Кой метод взима първия елемент или null/default?",
                    IsActive = true,
                    Options = new List<Option>
                    {
                        new Option { Text = "FirstOrDefault", IsCorrect = true },
                        new Option { Text = "ToList", IsCorrect = false },
                        new Option { Text = "Count", IsCorrect = false }
                    }
                }
            };

            db.Questions.AddRange(questions);
            await db.SaveChangesAsync();

          
            var quadraticQuestions = questions.Where(q => q.ExamId == quadraticExam.Id).ToList();
            var csharpQuestions = questions.Where(q => q.ExamId == csharpExam.Id).ToList();

            var attempt1 = new ExamAttempt
            {
                UserId = student.Id,
                ExamId = quadraticExam.Id,
                StartedAt = DateTime.UtcNow.AddDays(-2).AddMinutes(-20),
                FinishedAt = DateTime.UtcNow.AddDays(-2),
                TotalQuestions = quadraticQuestions.Count,
                CorrectCount = 2,
                ScorePercent = 66.67,
                Grade = 4.50
            };

            var attempt2 = new ExamAttempt
            {
                UserId = student.Id,
                ExamId = csharpExam.Id,
                StartedAt = DateTime.UtcNow.AddDays(-1).AddMinutes(-18),
                FinishedAt = DateTime.UtcNow.AddDays(-1),
                TotalQuestions = csharpQuestions.Count,
                CorrectCount = 3,
                ScorePercent = 100,
                Grade = 6.00
            };

            db.ExamAttempts.AddRange(attempt1, attempt2);
            await db.SaveChangesAsync();

        
            var q1 = quadraticQuestions[0];
            var q2 = quadraticQuestions[1];
            var q3 = quadraticQuestions[2];

            db.AttemptAnswers.AddRange(
                new AttemptAnswer
                {
                    ExamAttemptId = attempt1.Id,
                    QuestionId = q1.Id,
                    SelectedOptionId = q1.Options!.First(o => o.IsCorrect).Id,
                    IsCorrect = true
                },
                new AttemptAnswer
                {
                    ExamAttemptId = attempt1.Id,
                    QuestionId = q2.Id,
                    SelectedOptionId = q2.Options!.First(o => o.Text == "4").Id,
                    IsCorrect = false
                },
                new AttemptAnswer
                {
                    ExamAttemptId = attempt1.Id,
                    QuestionId = q3.Id,
                    SelectedOptionId = q3.Options!.First(o => o.IsCorrect).Id,
                    IsCorrect = true
                }
            );

            var cq1 = csharpQuestions[0];
            var cq2 = csharpQuestions[1];
            var cq3 = csharpQuestions[2];

            db.AttemptAnswers.AddRange(
                new AttemptAnswer
                {
                    ExamAttemptId = attempt2.Id,
                    QuestionId = cq1.Id,
                    SelectedOptionId = cq1.Options!.First(o => o.IsCorrect).Id,
                    IsCorrect = true
                },
                new AttemptAnswer
                {
                    ExamAttemptId = attempt2.Id,
                    QuestionId = cq2.Id,
                    SelectedOptionId = cq2.Options!.First(o => o.IsCorrect).Id,
                    IsCorrect = true
                },
                new AttemptAnswer
                {
                    ExamAttemptId = attempt2.Id,
                    QuestionId = cq3.Id,
                    SelectedOptionId = cq3.Options!.First(o => o.IsCorrect).Id,
                    IsCorrect = true
                }
            );

            await db.SaveChangesAsync();
        }
    }
}