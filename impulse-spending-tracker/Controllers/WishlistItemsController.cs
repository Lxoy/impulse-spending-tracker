using impulse_spending_tracker.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace impulse_spending_tracker.Controllers
{
    public class WishlistItemsController : Controller
    {
        private readonly WishlistItemMockRepository _wishlistRepository;

        public WishlistItemsController(WishlistItemMockRepository wishlistRepository)
        {
            _wishlistRepository = wishlistRepository;
        }

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

        public IActionResult Details(Guid id)
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
