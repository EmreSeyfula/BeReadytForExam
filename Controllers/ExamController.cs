using BeReadyForExam.Models;
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
            var model = new ExamEditorViewModel
            {
                IsActive = true,
                RandomizeQuestions = true,
                QuestionsCount = 5,
                Questions = new List<ExamQuestionEditorViewModel>
                {
                    CreateDefaultQuestion()
                }
            };

            await LoadEditorSelectionsAsync(model);
            return View("Editor", model);
        }

        [Authorize(Roles = "Teacher,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExamEditorViewModel model)
        {
            NormalizeEditorModel(model);
            await ValidateEditorModelAsync(model);

            if (!ModelState.IsValid)
            {
                await LoadEditorSelectionsAsync(model);
                return View("Editor", model);
            }

            try
            {
                await _examService.CreateWithQuestionsAsync(model);
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await LoadEditorSelectionsAsync(model);
                return View("Editor", model);
            }
        }
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _examService.GetEditorByIdAsync(id);
            if (model == null) return NotFound();

            await LoadEditorSelectionsAsync(model);
            return View("Editor", model);
        }
        [Authorize(Roles = "Teacher,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ExamEditorViewModel model)
        {
            NormalizeEditorModel(model);
            await ValidateEditorModelAsync(model);

            if (!ModelState.IsValid)
            {
                await LoadEditorSelectionsAsync(model);
                return View("Editor", model);
            }

            try
            {
                await _examService.UpdateWithQuestionsAsync(model);
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await LoadEditorSelectionsAsync(model);
                return View("Editor", model);
            }
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

        private async Task ValidateEditorModelAsync(ExamEditorViewModel model)
        {
            var topics = await _topicService.GetAllAsync();
            var topicList = topics ?? new List<Topic>();
            var selectedTopic = topicList.FirstOrDefault(topic => topic.Id == model.TopicId);

            if (!model.SubjectId.HasValue || model.SubjectId.Value <= 0)
            {
                ModelState.AddModelError(nameof(model.SubjectId), "Моля, изберете предмет.");
            }

            if (model.TopicId <= 0 || selectedTopic == null)
            {
                ModelState.AddModelError(nameof(model.TopicId), "Моля, изберете тема.");
            }
            else if (model.SubjectId.HasValue && selectedTopic.SubjectId != model.SubjectId.Value)
            {
                ModelState.AddModelError(nameof(model.TopicId), "Избраната тема не принадлежи на избрания предмет.");
            }

            if (model.Questions == null || model.Questions.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "Добавете поне един въпрос към теста.");
                return;
            }

            for (int questionIndex = 0; questionIndex < model.Questions.Count; questionIndex++)
            {
                var question = model.Questions[questionIndex];
                var questionLabel = $"Въпрос #{questionIndex + 1}";

                if (string.IsNullOrWhiteSpace(question.Text))
                {
                    ModelState.AddModelError($"Questions[{questionIndex}].Text", $"{questionLabel} трябва да има текст.");
                }

                if (question.Options.Count < 2)
                {
                    ModelState.AddModelError(string.Empty, $"{questionLabel} трябва да има поне 2 непразни отговора.");
                }

                if (!question.Options.Any(option => option.IsCorrect))
                {
                    ModelState.AddModelError(string.Empty, $"{questionLabel} трябва да има поне един верен отговор.");
                }
            }
        }

        private async Task LoadEditorSelectionsAsync(ExamEditorViewModel model)
        {
            var topics = await _topicService.GetAllAsync();
            var topicList = topics ?? new List<Topic>();

            if ((!model.SubjectId.HasValue || model.SubjectId.Value <= 0) && model.TopicId > 0)
            {
                model.SubjectId = topicList
                    .FirstOrDefault(topic => topic.Id == model.TopicId)
                    ?.SubjectId;
            }

            var filteredTopics = model.SubjectId.HasValue
                ? topicList.Where(topic => topic.SubjectId == model.SubjectId.Value).ToList()
                : topicList;

            model.Subjects = topicList
                .Where(topic => topic.Subject != null)
                .Select(topic => topic.Subject!)
                .GroupBy(subject => subject.Id)
                .OrderBy(group => group.First().Name)
                .Select(group => new SelectListItem
                {
                    Value = group.Key.ToString(),
                    Text = group.First().Name,
                    Selected = model.SubjectId.HasValue && group.Key == model.SubjectId.Value
                })
                .ToList();

            model.Topics = filteredTopics
                .OrderBy(topic => topic.Name)
                .Select(topic => new SelectListItem
                {
                    Value = topic.Id.ToString(),
                    Text = topic.Name,
                    Selected = topic.Id == model.TopicId
                })
                .ToList();

            model.Questions ??= new List<ExamQuestionEditorViewModel>();
            if (model.Questions.Count == 0)
            {
                model.Questions.Add(CreateDefaultQuestion());
            }
        }
    }
}
