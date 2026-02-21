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
        private readonly ITopicService _topicService;

        public QuestionController(
            IQuestionService questionService,
            ITopicService topicService)
        {
            _questionService = questionService;
            _topicService = topicService;
        }

        public async Task<IActionResult> Index()
        {
            var questions = await _questionService.GetAllAsync();
            return View(questions);
        }

     
        public async Task<IActionResult> Create()
        {
            var topics = await _topicService.GetAllAsync();

            var vm = new QuestionCreateViewModel
            {
                Topics = topics.Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Name
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
            if (!ModelState.IsValid)
            {
                // ВРЪЩАМЕ dropdown пак
                var topics = await _topicService.GetAllAsync();
                model.Topics = topics.Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Name
                }).ToList();

                return View(model);
            }

            var question = new Question
            {
                Text = model.Text,
                TopicId = model.TopicId,
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

      
        public async Task<IActionResult> Edit(int id)
        {
            var question = await _questionService.GetByIdAsync(id);
            if (question == null) return NotFound();

            return View(question);
        }

       
        [HttpPost]
        public async Task<IActionResult> Edit(Question question)
        {
            if (!ModelState.IsValid) return View(question);

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