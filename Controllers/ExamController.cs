using BeReadyForExam.Models;
using BeReadyForExam.Services.Implementations;
using BeReadyForExam.Services.Interfaces;
using BeReadyForExam.ViewModel.Exam;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

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

        public async Task<IActionResult> Index()
        {
            var exams = await _examService.GetAllAsync();
            return View(exams);
        }

   

        
            public async Task<IActionResult> Create()
        {
            await LoadTopicsAsync();
            return View(new Exam { IsActive = true, RandomizeQuestions = true, QuestionsCount = 10 });
        }
        

        [HttpPost]
        public async Task<IActionResult> Create(Exam exam)
        {
            if (!ModelState.IsValid)
            {
                await LoadTopicsAsync(exam.TopicId);  
                return View(exam);
            }

            exam.CreatedAt = DateTime.UtcNow;
            await _examService.CreateAsync(exam);

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadTopicsAsync(int? selectedTopicId = null)
        {
            var topics = await _topicService.GetAllAsync();

            ViewBag.Topics = (topics ?? new List<Topic>())
                .Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Name,
                    Selected = selectedTopicId.HasValue && t.Id == selectedTopicId.Value
                })
                .ToList();
        }



        public async Task<IActionResult> Edit(int id)
        {
            var exam = await _examService.GetByIdAsync(id);
            if (exam == null) return NotFound();

            await LoadTopicsAsync(exam.TopicId);
            return View(exam);
        }

        [HttpPost]
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

      

        public async Task<IActionResult> Delete(int id)
        {
            var exam = await _examService.GetByIdAsync(id);
            if (exam == null) return NotFound();

            return View(exam);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _examService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    


        public async Task<IActionResult> Start(int topicId)
        {
            var userId = _userManager.GetUserId(User);

            var attemptId = await _examService.StartExamAsync(topicId, userId);

            return RedirectToAction("Take", new { attemptId });
        }

        public async Task<IActionResult> Take(int attemptId)
        {
            var exam = await _examService.GetExamAsync(attemptId);
            return View(exam);
        }

        [HttpPost]
        public async Task<IActionResult> Submit(SubmitExamViewModel model)
        {
            await _examService.SubmitExamAsync(model);
            return RedirectToAction("Result", new { attemptId = model.AttemptId });
        }

        [Authorize] 
        public async Task<IActionResult> Result(int attemptId)
        {
            var userId = _userManager.GetUserId(User);
            var vm = await _examService.GetResultAsync(attemptId, userId);
            return View(vm);
        }

        [Authorize]
        public async Task<IActionResult> MyHistory()
        {
            var userId = _userManager.GetUserId(User);
            var vm = await _examService.GetMyHistoryAsync(userId);
            return View(vm);
        }
    }
}
