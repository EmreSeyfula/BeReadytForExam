using BeReadyForExam.Models;
using BeReadyForExam.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BeReadyForExam.Controllers
{
    public class TopicsController : Controller
    {
       private readonly ITopicService _service;

            public TopicsController(ITopicService service) 
            {
                _service = service;
            }
            public async Task<IActionResult> Index()
            {
              var topics = await _service.GetAllAsync();
              return View(topics);
            }

            public async Task<IActionResult> Create()
            {
                var subjects = await _service.GetAllSubjectsAsync();
                ViewBag.Subjects = new SelectList(subjects, "Id", "Name");
                return View();
            }

            [HttpPost]
            public async Task<IActionResult> Create(Topic topic)
            {
                if (!ModelState.IsValid)
                {
                    var subjects = await _service.GetAllSubjectsAsync();
                    ViewBag.Subjects = new SelectList(subjects, "Id", "Name");
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
            public async Task<IActionResult> DeleteConfirmed(int id)
            {
                await _service.DeleteAsync(id);
                return RedirectToAction(nameof(Index));
            }

    }   

}


