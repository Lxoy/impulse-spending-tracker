using impulse_spending_tracker.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace impulse_spending_tracker.Controllers
{
    [Route("wishlist")]
    public class WishlistItemsController : Controller
    {
        private readonly WishlistItemRepository _wishlistRepository;

        public WishlistItemsController(WishlistItemRepository wishlistRepository)
        {
            _wishlistRepository = wishlistRepository;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            var items = _wishlistRepository
                .GetAll()
                .OrderBy(w => w.IsPurchased)
                .ThenByDescending(w => w.Priority)
                .ThenByDescending(w => w.AddedAt)
                .ToList();

            return View(items);
        }

        [HttpGet("{id:int}")]
        public IActionResult Details(int id)
        {
            var item = _wishlistRepository.GetById(id);
            if (item is null)
            {
                return NotFound();
            }

            return View(item);
        }
    }
}
