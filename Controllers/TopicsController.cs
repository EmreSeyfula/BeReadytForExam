using BeReadyForExam.Models;
using BeReadyForExam.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BeReadyForExam.Controllers
{
    [Authorize(Roles = "Teacher,Admin")]
    public class TopicsController : Controller
    {
        private readonly ITopicService _service;

        public TopicsController(ITopicService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index(int? subjectId)
        {
            var topics = await _service.GetAllAsync();

            if (subjectId.HasValue)
            {
                topics = topics
                    .Where(topic => topic.SubjectId == subjectId.Value)
                    .ToList();
            }

            return View(topics);
        }

        public async Task<IActionResult> Create()
        {
            var subjects = await _service.GetAllSubjectsAsync();
            ViewBag.Subjects = new SelectList(subjects, "Id", "Name");
            return View(new Topic { IsActive = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Topic topic)
        {
            if (!ModelState.IsValid)
            {
                var subjects = await _service.GetAllSubjectsAsync();
                ViewBag.Subjects = new SelectList(subjects, "Id", "Name", topic.SubjectId);
                return View(topic);
            }

            await _service.CreateAsync(topic);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var topic = await _service.GetByIdAsync(id);
            if (topic == null) return NotFound();

            var subjects = await _service.GetAllSubjectsAsync();
            ViewBag.Subjects = new SelectList(subjects, "Id", "Name", topic.SubjectId);

            return View(topic);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Topic topic)
        {
            if (!ModelState.IsValid)
            {
                var subjects = await _service.GetAllSubjectsAsync();
                ViewBag.Subjects = new SelectList(subjects, "Id", "Name", topic.SubjectId);
                return View(topic);
            }

            await _service.UpdateAsync(topic);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var topic = await _service.GetByIdAsync(id);
            if (topic == null) return NotFound();

            return View(topic);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _service.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> BySubject(int? subjectId)
        {
            var topics = await _service.GetAllAsync();

            var filteredTopics = subjectId.HasValue
                ? topics.Where(t => t.SubjectId == subjectId.Value)
                : topics;

            return Json(filteredTopics
                .OrderBy(t => t.Name)
                .Select(t => new
                {
                    id = t.Id,
                    name = t.Name,
                    subjectId = t.SubjectId
                }));
        }
    }
}
