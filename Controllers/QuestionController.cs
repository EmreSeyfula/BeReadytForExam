using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using BeReadyForExam.Services.Interfaces;
using BeReadyForExam.Models;
using BeReadyForExam.ViewModel;

namespace BeReadyForExam.Controllers
{
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

        

        public async Task<IActionResult> Index()
        {
            var questions = await _questionService.GetAllAsync();
            return View(questions);
        }



        public async Task<IActionResult> Create()
        {
            var exams = await _examService.GetAllActiveExamsAsync();

            var vm = new QuestionCreateViewModel
            {
                Exams = exams.Select(e => new SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text = e.Title
                }).ToList(),

                Options = new List<OptionInputModel>
                {
                    new OptionInputModel(),
                    new OptionInputModel()
                }
            };

            return View(vm);
        }

       
        [HttpPost]
        public async Task<IActionResult> Create(QuestionCreateViewModel model)
        {
            if (!model.Options.Any(o => o.IsCorrect))
            {
                ModelState.AddModelError("", "Select at least one correct answer.");
            }

            if (!ModelState.IsValid)
            {
                var exams = await _examService.GetAllActiveExamsAsync();
                model.Exams = exams.Select(e => new SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text = e.Title
                }).ToList();

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

            return RedirectToAction(nameof(Index));
        }

        // ================= EDIT GET =================

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

        // ================= EDIT POST =================

        [HttpPost]
        public async Task<IActionResult> Edit(QuestionCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var exams = await _examService.GetAllActiveExamsAsync();
                model.Exams = exams.Select(e => new SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text = e.Title
                }).ToList();

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

       

        public async Task<IActionResult> Delete(int id)
        {
            var question = await _questionService.GetByIdAsync(id);
            if (question == null) return NotFound();

            return View(question);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _questionService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}