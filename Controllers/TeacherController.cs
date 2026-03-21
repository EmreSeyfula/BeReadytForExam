using BeReadyForExam.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace BeReadyForExam.Controllers;
[Authorize(Roles = "Teacher,Admin")]
public class TeacherController : Controller
{
    private readonly IExamService _examService;

    public TeacherController(IExamService examService)
    {
        _examService = examService;
    }

    public async Task<IActionResult> Dashboard()
    {
        var vm = await _examService.GetTeacherDashboardAsync();
        return View(vm);
    }
}