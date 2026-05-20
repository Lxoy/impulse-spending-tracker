using impulse_spending_tracker.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace impulse_spending_tracker.Controllers
{
    [Route("tags")]
    public class TagsController : Controller
    {
        private readonly TagRepository _tagRepository;

        public TagsController(TagRepository tagRepository)
        {
            _tagRepository = tagRepository;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            var tags = _tagRepository
                .GetAll()
                .OrderBy(t => t.Name)
                .ToList();

            return View(tags);
        }

        [HttpGet("filter")]
        public IActionResult Filter(string query)
        {
            var tags = _tagRepository
                .GetAll()
                .Where(t => string.IsNullOrEmpty(query) || 
                            t.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
                .OrderBy(t => t.Name)
                .ToList();

            return PartialView("_TagTableRows", tags);
        }

        [HttpGet("{id:int}")]
        public IActionResult Details(int id)
        {
            var tag = _tagRepository.GetById(id);
            if (tag is null)
            {
                return NotFound();
            }

            return View(tag);
        }

        [HttpGet("create")]
        public IActionResult Create()
        {
            return View(new Models.Tag());
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Models.Tag tag)
        {
            if (!ModelState.IsValid)
            {
                return View(tag);
            }

            _tagRepository.Create(tag);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("edit")]
        public IActionResult Edit(int id)
        {
            var tag = _tagRepository.GetById(id);
            if (tag is null)
            {
                return NotFound();
            }

            return View(tag);
        }

        [HttpPost("edit")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Models.Tag tag)
        {
            if (!ModelState.IsValid)
            {
                return View(tag);
            }

            _tagRepository.Update(tag);
            return RedirectToAction(nameof(Details), new { id = tag.Id });
        }

        [HttpGet("delete")]
        public IActionResult Delete(int id)
        {
            var tag = _tagRepository.GetById(id);
            if (tag is null)
            {
                return NotFound();
            }

            return View(tag);
        }

        [HttpPost("delete")]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(Models.Tag model)
        {
            var tag = _tagRepository.GetById(model.Id);
            if (tag is null)
            {
                return NotFound();
            }

            try
            {
                _tagRepository.Delete(tag);
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "Unable to delete this tag because it is used by purchases.");
                return View(tag);
            }
        }
    }
}
