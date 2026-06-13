using impulse_spending_tracker.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace impulse_spending_tracker.Controllers
{
    [Authorize]
    [Route("trigger-types")]
    public class TriggerTypesController : Controller
    {
        private readonly TriggerTypeRepository _tagRepository;
        private readonly ILogger<TriggerTypesController> _logger;

        public TriggerTypesController(
            TriggerTypeRepository tagRepository,
            ILogger<TriggerTypesController> logger)
        {
            _tagRepository = tagRepository;
            _logger = logger;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            var tags = _tagRepository
                .GetAll()
                .OrderBy(t => t.Name)
                .ToList();

            _logger.LogInformation("Loaded {Count} trigger types.", tags.Count);
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
                _logger.LogWarning("Trigger type details requested for missing id {TriggerTypeId}.", id);
                return NotFound();
            }

            ViewBag.CanManageTag = User.IsInRole("Admin");
            return View(tag);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("create")]
        public IActionResult Create()
        {
            return View(new Models.TriggerType());
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Models.TriggerType tag)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Trigger type create validation failed for {TriggerTypeName}.", tag.Name);
                return View(tag);
            }

            _tagRepository.Create(tag);
            _logger.LogInformation("Trigger type {TriggerTypeId} created: {TriggerTypeName}.", tag.Id, tag.Name);
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("edit")]
        public IActionResult Edit(int id)
        {
            var tag = _tagRepository.GetById(id);
            if (tag is null)
            {
                _logger.LogWarning("Trigger type edit requested for missing id {TriggerTypeId}.", id);
                return NotFound();
            }

            return View(tag);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("edit")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Models.TriggerType tag)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Trigger type edit validation failed for id {TriggerTypeId}.", tag.Id);
                return View(tag);
            }

            _tagRepository.Update(tag);
            _logger.LogInformation("Trigger type {TriggerTypeId} updated.", tag.Id);
            return RedirectToAction(nameof(Details), new { id = tag.Id });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("delete")]
        public IActionResult Delete(int id)
        {
            var tag = _tagRepository.GetById(id);
            if (tag is null)
            {
                _logger.LogWarning("Trigger type delete requested for missing id {TriggerTypeId}.", id);
                return NotFound();
            }

            return View(tag);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("delete")]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(Models.TriggerType model)
        {
            var tag = _tagRepository.GetById(model.Id);
            if (tag is null)
            {
                _logger.LogWarning("Trigger type delete submitted for missing id {TriggerTypeId}.", model.Id);
                return NotFound();
            }

            try
            {
                _tagRepository.Delete(tag);
                _logger.LogInformation("Trigger type {TriggerTypeId} deleted.", tag.Id);
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogWarning(ex, "Trigger type {TriggerTypeId} could not be deleted because it is used by purchases.", tag.Id);
                ModelState.AddModelError(string.Empty, "Unable to delete this trigger type because it is used by purchases.");
                return View(tag);
            }
        }
    }
}
