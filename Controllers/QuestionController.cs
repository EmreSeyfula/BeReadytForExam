using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using BeReadyForExam.ViewModel.Question;
using BeReadyForExam.Services.Interfaces;
using BeReadyForExam.Models;
using BeReadyForExam.ViewModel;
using Microsoft.AspNetCore.Authorization;

namespace BeReadyForExam.Controllers
{
    [Authorize(Roles = "Teacher,Admin")]
    public class QuestionController : Controller
    {
        private readonly IQuestionService _questionService;
        private readonly IExamService _examService;
        private readonly ISubjectService _subjectService;
        private readonly ITopicService _topicService;

        public QuestionController(
            IQuestionService questionService,
            IExamService examService,
            ISubjectService subjectService,
            ITopicService topicService)
        {
            _questionService = questionService;
            _examService = examService;
            _subjectService = subjectService;
            _topicService = topicService;
        }


        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> Index(string? search, int? subjectId, int? topicId, int? examId, bool? isActive)
        {
            var questions = await _questionService.GetFilteredAsync(search, subjectId, topicId, examId, isActive);
            var subjects = await _subjectService.GetAllAsync();
            var topics = await _topicService.GetAllAsync();
            var exams = await _examService.GetAllAsync();

            var filteredTopics = subjectId.HasValue
                ? topics.Where(t => t.SubjectId == subjectId.Value).ToList()
                : topics;

            var filteredExams = exams.AsEnumerable();

            if (subjectId.HasValue)
                filteredExams = filteredExams.Where(e => e.Topic != null && e.Topic.SubjectId == subjectId.Value);

            if (topicId.HasValue)
                filteredExams = filteredExams.Where(e => e.TopicId == topicId.Value);

            var vm = new QuestionIndexViewModel
            {
                Questions = questions,
                Search = search,
                SubjectId = subjectId,
                TopicId = topicId,
                ExamId = examId,
                IsActive = isActive,

                Subjects = subjects.Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name,
                    Selected = subjectId.HasValue && s.Id == subjectId.Value
                }).ToList(),

                Topics = filteredTopics.Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Name,
                    Selected = topicId.HasValue && t.Id == topicId.Value
                }).ToList(),

                Exams = filteredExams.Select(e => new SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text = e.Title,
                    Selected = examId.HasValue && e.Id == examId.Value
                }).ToList()
            };

            return View(vm);
        }

        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> Create(int? examId)
        {
            var exams = await _examService.GetAllAsync();
            var subjects = await _subjectService.GetAllAsync();
            var topics = await _topicService.GetAllAsync();

            var selectedExam = examId.HasValue ? exams.FirstOrDefault(e => e.Id == examId.Value) : null;
            var selectedTopicId = selectedExam?.TopicId;
            var selectedSubjectId = selectedExam?.Topic?.SubjectId;
            var filteredTopics = selectedSubjectId.HasValue
                ? topics.Where(t => t.SubjectId == selectedSubjectId.Value).ToList()
                : topics;
            var filteredExams = exams.AsEnumerable();

            if (selectedSubjectId.HasValue)
            {
                filteredExams = filteredExams.Where(e => e.Topic != null && e.Topic.SubjectId == selectedSubjectId.Value);
            }

            if (selectedTopicId.HasValue)
            {
                filteredExams = filteredExams.Where(e => e.TopicId == selectedTopicId.Value);
            }

            var vm = new QuestionCreateViewModel
            {
                SubjectId = selectedSubjectId,
                TopicId = selectedTopicId,
                ExamId = examId ?? 0,
                Subjects = subjects.Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name,
                    Selected = selectedSubjectId.HasValue && s.Id == selectedSubjectId.Value
                }).ToList(),
                Topics = filteredTopics.Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Name,
                    Selected = selectedTopicId.HasValue && t.Id == selectedTopicId.Value
                }).ToList(),
                Exams = filteredExams.Select(e => new SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text = e.Title,
                    Selected = examId.HasValue && e.Id == examId.Value
                }).ToList(),
                Options = new List<OptionInputModel> { new(), new() }
            };

            return View(vm);
        }

        [Authorize(Roles = "Teacher,Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(QuestionCreateViewModel model)
        {
            

            

            NormalizeOptions(model);
            ValidateQuestionModel(model);

            if (!ModelState.IsValid)
            {
                await LoadExamsAsync(model);
                return View(model);
            }

            var question = new Question
            {
                Text = model.Text,
                ExamId = model.ExamId,
                IsActive = model.IsActive,
                Options = model.Options.Select(o => new Option
                {
                    Text = o.Text,
                    IsCorrect = o.IsCorrect
                }).ToList()
            };

            await _questionService.CreateAsync(question);

            return RedirectToAction(nameof(Index), new {examId = model.ExamId});
        }


        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var question = await _questionService.GetByIdAsync(id);
            if (question == null) return NotFound();

            var exams = await _examService.GetAllAsync();
            var subjects = await _subjectService.GetAllAsync();
            var topics = await _topicService.GetAllAsync();

            var selectedExam = exams.FirstOrDefault(e => e.Id == question.ExamId);
            var selectedTopicId = selectedExam?.TopicId;
            var selectedSubjectId = selectedExam?.Topic?.SubjectId;
            var filteredTopics = selectedSubjectId.HasValue
                ? topics.Where(t => t.SubjectId == selectedSubjectId.Value).ToList()
                : topics;
            var filteredExams = exams.AsEnumerable();

            if (selectedSubjectId.HasValue)
            {
                filteredExams = filteredExams.Where(e => e.Topic != null && e.Topic.SubjectId == selectedSubjectId.Value);
            }

            if (selectedTopicId.HasValue)
            {
                filteredExams = filteredExams.Where(e => e.TopicId == selectedTopicId.Value);
            }

            var vm = new QuestionCreateViewModel
            {
                Id = question.Id,
                Text = question.Text,
                SubjectId = selectedSubjectId,
                TopicId = selectedTopicId,
                ExamId = question.ExamId,
                IsActive = question.IsActive,

                Subjects = subjects.Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name,
                    Selected = selectedSubjectId.HasValue && s.Id == selectedSubjectId.Value
                }).ToList(),

                Topics = filteredTopics.Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Name,
                    Selected = selectedTopicId.HasValue && t.Id == selectedTopicId.Value
                }).ToList(),

                Exams = filteredExams.Select(e => new SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text = e.Title,
                    Selected = e.Id == question.ExamId
                }).ToList(),

                Options = question.Options.Select(o => new OptionInputModel
                {
                    Id = o.Id,
                    Text = o.Text,
                    IsCorrect = o.IsCorrect
                }).ToList()
            };

            return View(vm);
        }


        [Authorize(Roles = "Teacher,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(QuestionCreateViewModel model)
        {
            NormalizeOptions(model);
            ValidateQuestionModel(model);

            if (!ModelState.IsValid)
            {
                await LoadExamsAsync(model);
                return View(model);
            }

            var question = new Question
            {
                Id = model.Id,
                Text = model.Text,
                ExamId = model.ExamId,
                IsActive = model.IsActive,
                Options = model.Options.Select(o => new Option
                {
                    Id = o.Id,
                    Text = o.Text,
                    IsCorrect = o.IsCorrect,
                    QuestionId = model.Id
                }).ToList()
            };

            await _questionService.UpdateAsync(question);

            return RedirectToAction(nameof(Index), new { examId = model.ExamId });
        }


        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var question = await _questionService.GetByIdAsync(id);
            if (question == null) return NotFound();

            return View(question);
        }
        [Authorize(Roles = "Teacher,Admin")]
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _questionService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private void ValidateQuestionModel(QuestionCreateViewModel model)
        {
            if (model.Options.Count < 2)
            {
                ModelState.AddModelError("", "Въпросът трябва да има поне 2 отговора.");
            }

            if (!model.Options.Any(o => o.IsCorrect))
            {
                ModelState.AddModelError("", "Избери поне един верен отговор.");
            }
        }
        private static void NormalizeOptions(QuestionCreateViewModel model)
        {
            model.Options = (model.Options ?? new List<OptionInputModel>())
                .Where(o => !string.IsNullOrWhiteSpace(o.Text))
                .Select(o => new OptionInputModel
                {
                    Id = o.Id,
                    Text = o.Text.Trim(),
                    IsCorrect = o.IsCorrect
                })
                .ToList();
        }
        private async Task LoadExamsAsync(QuestionCreateViewModel model)
        {
            var subjects = await _subjectService.GetAllAsync();
            var topics = await _topicService.GetAllAsync();
            var exams = await _examService.GetAllAsync();
            var filteredTopics = model.SubjectId.HasValue
                ? topics.Where(t => t.SubjectId == model.SubjectId.Value).ToList()
                : topics;
            var filteredExams = exams.AsEnumerable();

            if (model.SubjectId.HasValue)
            {
                filteredExams = filteredExams.Where(e => e.Topic != null && e.Topic.SubjectId == model.SubjectId.Value);
            }

            if (model.TopicId.HasValue)
            {
                filteredExams = filteredExams.Where(e => e.TopicId == model.TopicId.Value);
            }

            model.Subjects = subjects.Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = s.Name,
                Selected = model.SubjectId.HasValue && s.Id == model.SubjectId.Value
            }).ToList();

            model.Topics = filteredTopics.Select(t => new SelectListItem
            {
                Value = t.Id.ToString(),
                Text = t.Name,
                Selected = model.TopicId.HasValue && t.Id == model.TopicId.Value
            }).ToList();

            model.Exams = filteredExams.Select(e => new SelectListItem
            {
                Value = e.Id.ToString(),
                Text = e.Title,
                Selected = e.Id == model.ExamId
            }).ToList();
        }

        public IActionResult BulkCreate(int examId)
        {
            var vm = new BulkCreateQuestionsViewModel
            {
                ExamId = examId,
                Questions = new List<QuestionInputModel>
        {
            new QuestionInputModel()
        }
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkCreate(BulkCreateQuestionsViewModel model)
        {
            model.Questions ??= new List<QuestionInputModel>();

            for (int i = 0; i < model.Questions.Count; i++)
            {
                var q = model.Questions[i];
                q.Options = (q.Options ?? new List<OptionInputModel>())
                    .Where(o => !string.IsNullOrWhiteSpace(o.Text))
                    .Select(o => new OptionInputModel
                    {
                        Id = o.Id,
                        Text = o.Text.Trim(),
                        IsCorrect = o.IsCorrect
                    })
                    .ToList();

                if (string.IsNullOrWhiteSpace(q.Text)) continue;

                if (q.Options.Count < 2)
                {
                    ModelState.AddModelError(string.Empty, $"Въпрос #{i + 1} трябва да има поне 2 отговора.");
                }

                if (!q.Options.Any(o => o.IsCorrect))
                {
                    ModelState.AddModelError(string.Empty, $"Въпрос #{i + 1} трябва да има поне един верен отговор.");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            foreach (var q in model.Questions)
            {
                if (string.IsNullOrWhiteSpace(q.Text)) continue;

                var question = new Question
                {
                    Text = q.Text.Trim(),
                    ExamId = model.ExamId,
                    IsActive = q.IsActive,
                    Options = q.Options
                        .Select(o => new Option
                        {
                            Text = o.Text,
                            IsCorrect = o.IsCorrect
                        }).ToList()
                };

                await _questionService.CreateAsync(question);
            }

            return RedirectToAction("Index", new { examId = model.ExamId });
        }
    }
}
