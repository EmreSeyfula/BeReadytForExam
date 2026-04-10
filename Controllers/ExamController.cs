using BeReadyForExam.Models;
using BeReadyForExam.Services.Implementations;
using BeReadyForExam.Services.Interfaces;
using BeReadyForExam.ViewModel.Exam;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
namespace BeReadyForExam.Controllers
{

    public class ExamController : Controller
    {
        private readonly IExamService _examService;
        private readonly ITopicService _topicService;
        private readonly UserManager<IdentityUser> _userManager;

        public ExamController(IExamService examService, ITopicService topicService,
            UserManager<IdentityUser> userManager)
        {
            _examService = examService;
            _topicService = topicService;
            _userManager = userManager;
        }
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> Index()
        {
            var exams = await _examService.GetTeacherExamListAsync();
            return View(exams);
        }
        [Authorize]
        public async Task<IActionResult> Available()
        {
            var exams = await _examService.GetAvailableExamsAsync();
            return View(exams);
        }

        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> Create()
        {
            await LoadTopicsAsync();
            return View(new Exam { IsActive = true, RandomizeQuestions = true, QuestionsCount = 5 });
        }

        [Authorize(Roles = "Teacher,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Exam exam)
        {
            if (!ModelState.IsValid)
            {
                await LoadTopicsAsync(exam.TopicId);
                return View(exam);
            }

            exam.CreatedAt = DateTime.UtcNow;

            var examId = await _examService.CreateAsync(exam);

            return RedirectToAction("BulkCreate", "Question", new { examId });
        }

        private async Task LoadTopicsAsync(int? selectedTopicId = null)
        {
            var topics = await _topicService.GetAllAsync();
            var topicList = topics ?? new List<Topic>();

            ViewBag.Topics = topicList
                .Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = $"{t.Subject?.Name} / {t.Name}",
                    Selected = selectedTopicId.HasValue && t.Id == selectedTopicId.Value
                })
                .ToList();

            ViewBag.Subjects = topicList
                .Where(t => t.Subject != null)
                .Select(t => t.Subject!)
                .GroupBy(s => s.Id)
                .Select(g => new SelectListItem
                {
                    Value = g.Key.ToString(),
                    Text = g.First().Name,
                    Selected = selectedTopicId.HasValue && topicList.Any(t => t.Id == selectedTopicId.Value && t.SubjectId == g.Key)
                })
                .ToList();
        }


        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var exam = await _examService.GetByIdAsync(id);
            if (exam == null) return NotFound();

            await LoadTopicsAsync(exam.TopicId);
            return View(exam);
        }
        [Authorize(Roles = "Teacher,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Exam exam)
        {
            if (!ModelState.IsValid)
            {
                await LoadTopicsAsync(exam.TopicId);
                return View(exam);
            }

            await _examService.UpdateAsync(exam);
            return RedirectToAction(nameof(Index));
        }


        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var exam = await _examService.GetByIdAsync(id);
            if (exam == null) return NotFound();

            return View(exam);
        }
        [Authorize(Roles = "Teacher,Admin")]
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _examService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }


        [Authorize]
        public async Task<IActionResult> Start(int examId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var attemptId = await _examService.StartExamAsync(examId, userId);
            return RedirectToAction("Take", new { attemptId });
        }
        [Authorize]
        public async Task<IActionResult> Take(int attemptId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var canAccessAllAttempts = User.IsInRole("Teacher") || User.IsInRole("Admin");

            var exam = await _examService.GetExamAsync(attemptId, userId, canAccessAllAttempts);
            return View(exam);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Submit(SubmitExamViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var canAccessAllAttempts = User.IsInRole("Teacher") || User.IsInRole("Admin");

            await _examService.SubmitExamAsync(model, userId, canAccessAllAttempts);
            return RedirectToAction("Result", new { attemptId = model.AttemptId });
        }

        [Authorize]
        public async Task<IActionResult> Result(int attemptId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var canAccessAllAttempts = User.IsInRole("Teacher") || User.IsInRole("Admin");

            var vm = await _examService.GetResultAsync(attemptId, userId, canAccessAllAttempts);
            return View(vm);
        }

        [Authorize]
        public async Task<IActionResult> MyHistory()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var vm = await _examService.GetMyHistoryAsync(userId);
            return View(vm);
        }

        [Authorize(Roles = "Teacher,Admin")]
        [HttpGet]
        public async Task<IActionResult> ByFilter(int? subjectId, int? topicId)
        {
            var exams = await _examService.GetAllAsync();

            var filteredExams = exams.AsEnumerable();

            if (subjectId.HasValue)
            {
                filteredExams = filteredExams.Where(e => e.Topic != null && e.Topic.SubjectId == subjectId.Value);
            }

            if (topicId.HasValue)
            {
                filteredExams = filteredExams.Where(e => e.TopicId == topicId.Value);
            }

            return Json(filteredExams
                .OrderBy(e => e.Title)
                .Select(e => new
                {
                    id = e.Id,
                    title = e.Title,
                    topicId = e.TopicId,
                    subjectId = e.Topic?.SubjectId
                }));
        }
    }
}
