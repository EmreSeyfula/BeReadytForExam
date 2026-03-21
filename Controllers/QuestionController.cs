using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        public QuestionController(
            IQuestionService questionService,
            IExamService examService)
        {
            _questionService = questionService;
            _examService = examService;
        }


        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> Index( int? examId)
        {
            var questions = await _questionService.GetAllAsync(examId);
            ViewBag.ExamId = examId;
            return View(questions);
        }


        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> Create(int? examId)
        {
            var exams = await _examService.GetAllActiveExamsAsync();

            var vm = new QuestionCreateViewModel
            {
                ExamId = examId ?? 0,
                Exams = exams.Select(e => new SelectListItem
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
            

            if (!model.Options.Any(o => o.IsCorrect))
            {
                ModelState.AddModelError("", "Select at least one correct answer.");
            }

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
                IsActive = true,
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

            var exams = await _examService.GetAllActiveExamsAsync();

            var vm = new QuestionCreateViewModel
            {
                Id = question.Id,
                Text = question.Text,
                ExamId = question.ExamId,

                Exams = exams.Select(e => new SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text = e.Title
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
                IsActive = true,
                Options = model.Options.Select(o => new Option
                {
                    Id = o.Id,
                    Text = o.Text,
                    IsCorrect = o.IsCorrect,
                    QuestionId = model.Id
                }).ToList()
            };

            await _questionService.UpdateAsync(question);

            return RedirectToAction(nameof(Index));
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
                ModelState.AddModelError("", "Question must have at least 2 options.");
            }

            if (!model.Options.Any(o => o.IsCorrect))
            {
                ModelState.AddModelError("", "Select at least one correct answer.");
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
            var exams = await _examService.GetAllActiveExamsAsync();

            model.Exams = exams.Select(e => new SelectListItem
            {
                Value = e.Id.ToString(),
                Text = e.Title,
                Selected = e.Id == model.ExamId
            }).ToList();
        }
    }
}