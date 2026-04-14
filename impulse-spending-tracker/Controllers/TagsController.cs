using impulse_spending_tracker.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace impulse_spending_tracker.Controllers
{
    public class TagsController : Controller
    {
        private readonly TagMockRepository _tagRepository;

        public TagsController(TagMockRepository tagRepository)
        {
            _tagRepository = tagRepository;
        }

        public IActionResult Index()
        {
            var tags = _tagRepository
                .GetAll()
                .OrderBy(t => t.Name)
                .ToList();

            return View(tags);
        }

        public IActionResult Details(Guid id)
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
