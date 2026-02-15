using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace BeReadyForExam.Controllers; 
[Authorize(Roles = "Teacher,Admin")]
public class TeacherController : Controller
{ 
    public IActionResult Index() => View();
}