using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BeReadyForExam.Controllers;

[Authorize(Roles = "Student,Admin")]
    public class StudentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }

