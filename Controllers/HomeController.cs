using System.Diagnostics;
using BeReadyForExam.Data;
using BeReadyForExam.Models;
using BeReadyForExam.ViewModel.Home;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeReaytForExam.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var model = new HomeIndexViewModel
            {
                SubjectsCount = await _context.Subjects.CountAsync(),
                QuestionsCount = await _context.Questions.CountAsync(),
                UsersCount = await _context.Users.CountAsync(),
                IsAuthenticated = User.Identity?.IsAuthenticated == true,
                IsAdmin = User.IsInRole("Admin")
            };

            if (!model.IsAuthenticated)
            {
                return View(model);
            }

            if (model.IsAdmin)
            {
                var completedAttemptsCount = await _context.ExamAttempts
                    .CountAsync(attempt => attempt.FinishedAt.HasValue);

                model.HasProgressData = completedAttemptsCount > 0;
                model.ProgressTitle = "Среден прогрес";
                model.ProgressDescription = "Обобщени резултати от всички завършени опити в системата.";
                model.ProgressPercent = model.HasProgressData
                    ? await _context.ExamAttempts
                        .Where(attempt => attempt.FinishedAt.HasValue)
                        .AverageAsync(attempt => attempt.ScorePercent)
                    : 0;
                model.ProgressValueText = $"{model.ProgressPercent:F1}%";
                model.SecondaryMetricLabel = "Всички въпроси";
                model.SecondaryMetricValueText = model.QuestionsCount.ToString();
                model.SecondaryMetricPercent = 100;
                model.DetailsButtonText = "Вижте учителското табло";
                model.DetailsController = "Teacher";
                model.DetailsAction = "Dashboard";

                return View(model);
            }

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return View(model);
            }

            var completedUserAttemptsCount = await _context.ExamAttempts
                .CountAsync(attempt => attempt.UserId == userId && attempt.FinishedAt.HasValue);

            var totalAvailableQuestions = await _context.Questions.CountAsync(question => question.IsActive);
            var answeredQuestionIds = await _context.AttemptAnswers
                .Where(answer => answer.ExamAttempt.UserId == userId)
                .Select(answer => answer.QuestionId)
                .Distinct()
                .CountAsync();

            model.HasProgressData = completedUserAttemptsCount > 0 || answeredQuestionIds > 0;
            model.ProgressTitle = "Среден резултат";
            model.ProgressDescription = "Личният ви напредък се изчислява от вече решените тестове.";
            model.ProgressPercent = completedUserAttemptsCount > 0
                ? await _context.ExamAttempts
                    .Where(attempt => attempt.UserId == userId && attempt.FinishedAt.HasValue)
                    .AverageAsync(attempt => attempt.ScorePercent)
                : 0;
            model.ProgressValueText = $"{model.ProgressPercent:F1}%";
            model.SecondaryMetricLabel = "Решени въпроси";
            model.SecondaryMetricValueText = totalAvailableQuestions > 0
                ? $"{answeredQuestionIds}/{totalAvailableQuestions}"
                : answeredQuestionIds.ToString();
            model.SecondaryMetricPercent = totalAvailableQuestions > 0
                ? Math.Min(100, answeredQuestionIds * 100.0 / totalAvailableQuestions)
                : 0;
            model.DetailsButtonText = "Вижте историята си";
            model.DetailsController = "Exam";
            model.DetailsAction = "MyHistory";

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
