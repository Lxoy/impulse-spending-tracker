using impulse_spending_tracker.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace impulse_spending_tracker.Controllers
{
    [Authorize]
    [Route("users")]
    public class UserProfilesController : Controller
    {
        private readonly UserProfileRepository _userProfileRepository;
        private readonly UserManager<impulse_spending_tracker.Models.AppUser> _userManager;

        public UserProfilesController(
            UserProfileRepository userProfileRepository,
            UserManager<impulse_spending_tracker.Models.AppUser> userManager)
        {
            _userProfileRepository = userProfileRepository;
            _userManager = userManager;
        }

        private bool CanManageUserProfile(impulse_spending_tracker.Models.UserProfile user)
        {
            if (User.IsInRole("Admin"))
            {
                return true;
            }

            var currentUserId = _userManager.GetUserId(User);
            return !string.IsNullOrEmpty(currentUserId) && user.AppUserId == currentUserId;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            var users = _userProfileRepository
                .GetAll()
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ToList();

            return View(users);
        }

        [HttpGet("filter")]
        public IActionResult Filter(string query)
        {
            var users = _userProfileRepository
                .GetAll()
                .Where(u => string.IsNullOrEmpty(query) || 
                            u.FirstName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                            u.LastName.Contains(query, StringComparison.OrdinalIgnoreCase))
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ToList();

            return PartialView("_UserProfileTableRows", users);
        }

        [HttpGet("{id:int}")]
        public IActionResult Details(int id)
        {
            var user = _userProfileRepository.GetById(id);
            if (user is null)
            {
                return NotFound();
            }

            ViewBag.CanManageUserProfile = CanManageUserProfile(user);
            return View(user);
        }

        [Authorize]
        [HttpGet("edit")]
        public IActionResult Edit(int id)
        {
            var user = _userProfileRepository.GetById(id);
            if (user is null)
            {
                return NotFound();
            }

            if (!CanManageUserProfile(user))
            {
                return Forbid();
            }

            return View(user);
        }

        [Authorize]
        [HttpPost("edit")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Models.UserProfile user)
        {
            if (!ModelState.IsValid)
            {
                return View(user);
            }

            var existingUser = _userProfileRepository.GetById(user.Id);
            if (existingUser is null)
            {
                return NotFound();
            }

            if (!CanManageUserProfile(existingUser))
            {
                return Forbid();
            }

            user.AppUserId = existingUser.AppUserId;
            user.CreatedAt = existingUser.CreatedAt;
            _userProfileRepository.Update(user);
            return RedirectToAction(nameof(Details), new { id = user.Id });
        }

        [Authorize]
        [HttpGet("delete")]
        public IActionResult Delete(int id)
        {
            var user = _userProfileRepository.GetById(id);
            if (user is null)
            {
                return NotFound();
            }

            if (!CanManageUserProfile(user))
            {
                return Forbid();
            }

            return View(user);
        }

        [Authorize]
        [HttpPost("delete")]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(Models.UserProfile model)
        {
            var user = _userProfileRepository.GetById(model.Id);
            if (user is null)
            {
                return NotFound();
            }

            if (!CanManageUserProfile(user))
            {
                return Forbid();
            }

            try
            {
                _userProfileRepository.Delete(user);
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "Unable to delete this user profile because related data exists.");
                return View(user);
            }
        }
    }
}