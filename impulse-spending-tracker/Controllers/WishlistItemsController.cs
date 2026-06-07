using impulse_spending_tracker.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace impulse_spending_tracker.Controllers
{
    [Authorize]
    [Route("wishlist")]
    public class WishlistItemsController : Controller
    {
        private readonly WishlistItemRepository _wishlistRepository;
        private readonly UserProfileRepository _userProfileRepository;
        private readonly Microsoft.AspNetCore.Identity.UserManager<impulse_spending_tracker.Models.AppUser> _userManager;

        public WishlistItemsController(
            WishlistItemRepository wishlistRepository,
            UserProfileRepository userProfileRepository,
            Microsoft.AspNetCore.Identity.UserManager<impulse_spending_tracker.Models.AppUser> userManager)
        {
            _wishlistRepository = wishlistRepository;
            _userProfileRepository = userProfileRepository;
            _userManager = userManager;
        }

        [Authorize]
        [HttpGet("")]
        public IActionResult Index()
        {
            if (User.IsInRole("Admin"))
            {
                var items = _wishlistRepository
                    .GetAll()
                    .OrderBy(w => w.IsPurchased)
                    .ThenByDescending(w => w.Priority)
                    .ThenByDescending(w => w.AddedAt)
                    .ToList();

                return View(items);
            }

            var profileId = GetCurrentUserProfileId();
            if (!profileId.HasValue) return Forbid();

            var itemsForUser = _wishlistRepository
                .GetAll()
                .Where(w => w.UserProfileId == profileId.Value)
                .OrderBy(w => w.IsPurchased)
                .ThenByDescending(w => w.Priority)
                .ThenByDescending(w => w.AddedAt)
                .ToList();

            return View(itemsForUser);
        }

        [Authorize]
        [HttpGet("filter")]
        public IActionResult Filter(string query)
        {
            if (User.IsInRole("Admin"))
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

            var profileId = GetCurrentUserProfileId();
            if (!profileId.HasValue) return Forbid();

            var filtered = _wishlistRepository
                .GetAll()
                .Where(w => w.UserProfileId == profileId.Value &&
                            (string.IsNullOrEmpty(query) || w.Name.Contains(query, StringComparison.OrdinalIgnoreCase)))
                .OrderBy(w => w.IsPurchased)
                .ThenByDescending(w => w.Priority)
                .ThenByDescending(w => w.AddedAt)
                .ToList();

            return PartialView("_WishlistItemTableRows", filtered);
        }

        [Authorize]
        [HttpGet("{id:int}")]
        public IActionResult Details(int id)
        {
            var item = _wishlistRepository.GetById(id);
            if (item is null) return NotFound();
            if (!CanManageWishlistItem(item)) return Forbid();
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

        private int? GetCurrentUserProfileId()
        {
            var currentUserId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(currentUserId))
            {
                return null;
            }

            var profile = _userProfileRepository.GetAll().FirstOrDefault(p => p.AppUserId == currentUserId);
            return profile?.Id;
        }

        private bool CanManageWishlistItem(Models.WishlistItem item)
        {
            if (User.IsInRole("Admin"))
            {
                return true;
            }

            var currentProfileId = GetCurrentUserProfileId();
            return currentProfileId.HasValue && item.UserProfileId == currentProfileId.Value;
        }

        [Authorize]
        [HttpGet("create")]
        public IActionResult Create()
        {
            if (User.IsInRole("Admin"))
            {
                LoadDropdownData();
                ViewBag.ShowUserSelector = true;
                return View(new Models.WishlistItem());
            }

            var currentProfileId = GetCurrentUserProfileId();
            if (!currentProfileId.HasValue)
            {
                return Forbid();
            }

            ViewBag.ShowUserSelector = false;
            ViewBag.CurrentUserProfile = _userProfileRepository.GetById(currentProfileId.Value);
            return View(new Models.WishlistItem { UserProfileId = currentProfileId.Value });
        }

        [Authorize]
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Models.WishlistItem item)
        {
            if (!User.IsInRole("Admin"))
            {
                var currentProfileId = GetCurrentUserProfileId();
                if (!currentProfileId.HasValue)
                {
                    return Forbid();
                }

                item.UserProfileId = currentProfileId.Value;
            }

            if (!ModelState.IsValid)
            {
                if (User.IsInRole("Admin"))
                {
                    LoadDropdownData();
                }

                if (!User.IsInRole("Admin"))
                {
                    var pid = GetCurrentUserProfileId();
                    if (pid.HasValue) ViewBag.CurrentUserProfile = _userProfileRepository.GetById(pid.Value);
                }

                PopulateSelectedUser(item);
                return View(item);
            }

            item.AddedAt = DateTime.Now;
            _wishlistRepository.Create(item);
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        [HttpGet("edit")]
        public IActionResult Edit(int id)
        {
            var item = _wishlistRepository.GetById(id);
            if (item is null)
            {
                return NotFound();
            }

            if (!CanManageWishlistItem(item))
            {
                return Forbid();
            }

            if (User.IsInRole("Admin"))
            {
                LoadDropdownData();
            }
            else
            {
                ViewBag.ShowUserSelector = false;
                ViewBag.CurrentUserProfile = _userProfileRepository.GetById(item.UserProfileId);
            }

            return View(item);
        }

        [Authorize]
        [HttpPost("edit")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Models.WishlistItem item)
        {
            var existingItem = _wishlistRepository.GetById(item.Id);
            if (existingItem is null)
            {
                return NotFound();
            }

            if (!CanManageWishlistItem(existingItem))
            {
                return Forbid();
            }

            if (!User.IsInRole("Admin"))
            {
                item.UserProfileId = existingItem.UserProfileId;
            }

            if (!ModelState.IsValid)
            {
                if (User.IsInRole("Admin"))
                {
                    LoadDropdownData();
                }

                if (!User.IsInRole("Admin")) ViewBag.CurrentUserProfile = _userProfileRepository.GetById(item.UserProfileId);

                PopulateSelectedUser(item);
                return View(item);
            }

            item.AddedAt = existingItem.AddedAt;
            _wishlistRepository.Update(item);
            return RedirectToAction(nameof(Details), new { id = item.Id });
        }

        [Authorize]
        [HttpGet("delete")]
        public IActionResult Delete(int id)
        {
            var item = _wishlistRepository.GetById(id);
            if (item is null)
            {
                return NotFound();
            }

            if (!CanManageWishlistItem(item))
            {
                return Forbid();
            }

            return View(item);
        }

        [Authorize]
        [HttpPost("delete")]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(Models.WishlistItem model)
        {
            var item = _wishlistRepository.GetById(model.Id);
            if (item is null)
            {
                return NotFound();
            }

            if (!CanManageWishlistItem(item))
            {
                return Forbid();
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
