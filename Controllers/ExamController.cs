using BeReadyForExam.Services.Interfaces;
using BeReadyForExam.ViewModel.Exam;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BeReadyForExam.Controllers
{
    public class ExamController : Controller
    {
        private readonly IExamService _examService;
        private readonly UserManager<IdentityUser> _userManager;

        public ExamController(IExamService examService,
            UserManager<IdentityUser> userManager)
        {
            _examService = examService;
            _userManager = userManager;
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
