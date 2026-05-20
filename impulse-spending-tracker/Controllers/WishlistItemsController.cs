using impulse_spending_tracker.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace impulse_spending_tracker.Controllers
{
    [Route("wishlist")]
    public class WishlistItemsController : Controller
    {
        private readonly WishlistItemRepository _wishlistRepository;
        private readonly UserProfileRepository _userProfileRepository;

        public WishlistItemsController(
            WishlistItemRepository wishlistRepository,
            UserProfileRepository userProfileRepository)
        {
            _wishlistRepository = wishlistRepository;
            _userProfileRepository = userProfileRepository;
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

        [HttpGet("filter")]
        public IActionResult Filter(string query)
        {
            var items = _wishlistRepository
                .GetAll()
                .Where(w => string.IsNullOrEmpty(query) || 
                            w.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
                .OrderBy(w => w.IsPurchased)
                .ThenByDescending(w => w.Priority)
                .ThenByDescending(w => w.AddedAt)
                .ToList();

            return PartialView("_WishlistItemTableRows", items);
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

        private void LoadDropdownData()
        {
            var users = _userProfileRepository.GetAll()
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = $"{u.FirstName} {u.LastName} (ID: {u.Id})"
                })
                .ToList();

            ViewBag.UserProfileId = users;
        }

        private void PopulateSelectedUser(Models.WishlistItem item)
        {
            if (item.UserProfileId > 0)
            {
                item.UserProfile = _userProfileRepository.GetById(item.UserProfileId);
            }
        }

        [HttpGet("create")]
        public IActionResult Create()
        {
            LoadDropdownData();
            return View(new Models.WishlistItem());
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Models.WishlistItem item)
        {
            if (!ModelState.IsValid)
            {
                LoadDropdownData();
                PopulateSelectedUser(item);
                return View(item);
            }

            item.AddedAt = DateTime.Now;
            _wishlistRepository.Create(item);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("edit")]
        public IActionResult Edit(int id)
        {
            var item = _wishlistRepository.GetById(id);
            if (item is null)
            {
                return NotFound();
            }

            LoadDropdownData();
            return View(item);
        }

        [HttpPost("edit")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Models.WishlistItem item)
        {
            if (!ModelState.IsValid)
            {
                LoadDropdownData();
                PopulateSelectedUser(item);
                return View(item);
            }

            var existingItem = _wishlistRepository.GetById(item.Id);
            if (existingItem is null)
            {
                return NotFound();
            }

            item.AddedAt = existingItem.AddedAt;
            _wishlistRepository.Update(item);
            return RedirectToAction(nameof(Details), new { id = item.Id });
        }

        [HttpGet("delete")]
        public IActionResult Delete(int id)
        {
            var item = _wishlistRepository.GetById(id);
            if (item is null)
            {
                return NotFound();
            }

            return View(item);
        }

        [HttpPost("delete")]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(Models.WishlistItem model)
        {
            var item = _wishlistRepository.GetById(model.Id);
            if (item is null)
            {
                return NotFound();
            }

            try
            {
                _wishlistRepository.Delete(item);
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "Unable to delete this wishlist item because related data exists.");
                return View(item);
            }
        }
    }
}
