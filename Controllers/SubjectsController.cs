using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BeReadyForExam.Data;
using BeReadyForExam.Models;
using BeReadyForExam.Services.Interfaces;

namespace BeReadyForExam.Controllers
{
   
   public class SubjectsController : Controller
    {
        private readonly ISubjectService _service;

        public SubjectsController(ISubjectService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index()
        {
            var subjects = await _service.GetAllAsync();
            return View(subjects);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Subject subject)
        {
            if (!ModelState.IsValid) return View(subject);

            await _service.CreateAsync(subject);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var subject = await _service.GetByIdAsync(id);
            if (subject == null) return NotFound();

            return View(subject);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Subject subject)
        {
            if (!ModelState.IsValid) return View(subject);

            await _service.UpdateAsync(subject);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var subject = await _service.GetByIdAsync(id);
            if (subject == null) return NotFound();

            return View(subject);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _service.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}