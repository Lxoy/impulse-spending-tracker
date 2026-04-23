using impulse_spending_tracker.Repositories;
using Microsoft.AspNetCore.Mvc;

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
    }
}
