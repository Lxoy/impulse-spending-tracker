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
        private readonly ILogger<UserProfilesController> _logger;

        public UserProfilesController(
            UserProfileRepository userProfileRepository,
            UserManager<impulse_spending_tracker.Models.AppUser> userManager,
            ILogger<UserProfilesController> logger)
        {
            _userProfileRepository = userProfileRepository;
            _userManager = userManager;
            _logger = logger;
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

        private bool IsEditingOwnProfile(impulse_spending_tracker.Models.UserProfile user)
        {
            var currentUserId = _userManager.GetUserId(User);
            return !string.IsNullOrEmpty(currentUserId) && user.AppUserId == currentUserId;
        }

        private async Task<bool> IsGoogleAccountAsync(impulse_spending_tracker.Models.UserProfile user)
        {
            if (string.IsNullOrWhiteSpace(user.AppUserId))
            {
                return false;
            }

            var appUser = await _userManager.FindByIdAsync(user.AppUserId);
            if (appUser is null)
            {
                return false;
            }

            var logins = await _userManager.GetLoginsAsync(appUser);
            return logins.Any(login => string.Equals(login.LoginProvider, "Google", StringComparison.OrdinalIgnoreCase));
        }

        private async Task LoadEditPermissionsAsync(impulse_spending_tracker.Models.UserProfile user)
        {
            var isOwnProfile = IsEditingOwnProfile(user);
            var isGoogleAccount = await IsGoogleAccountAsync(user);

            ViewBag.CanEditRiskToleranceScore = User.IsInRole("Admin") || !isOwnProfile;
            ViewBag.CanEditEmail = !isGoogleAccount || !isOwnProfile;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            var users = _userProfileRepository
                .GetAll()
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ToList();

            _logger.LogInformation("Loaded {Count} user profiles.", users.Count);
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
                _logger.LogWarning("User profile details requested for missing id {UserProfileId}.", id);
                return NotFound();
            }

            ViewBag.CanManageUserProfile = CanManageUserProfile(user);
            return View(user);
        }

        [Authorize]
        [HttpGet("edit")]
        public async Task<IActionResult> Edit(int id)
        {
            var user = _userProfileRepository.GetById(id);
            if (user is null)
            {
                _logger.LogWarning("User profile edit requested for missing id {UserProfileId}.", id);
                return NotFound();
            }

            if (!CanManageUserProfile(user))
            {
                _logger.LogWarning("User profile {UserProfileId} edit forbidden for current user.", id);
                return Forbid();
            }

            await LoadEditPermissionsAsync(user);
            return View(user);
        }

        [Authorize]
        [HttpPost("edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Models.UserProfile user)
        {
            var existingUser = _userProfileRepository.GetById(user.Id);
            if (existingUser is null)
            {
                _logger.LogWarning("User profile edit submitted for missing id {UserProfileId}.", user.Id);
                return NotFound();
            }

            if (!CanManageUserProfile(existingUser))
            {
                _logger.LogWarning("User profile {UserProfileId} edit submit forbidden for current user.", user.Id);
                return Forbid();
            }

            if (IsEditingOwnProfile(existingUser) && !User.IsInRole("Admin"))
            {
                user.RiskToleranceScore = existingUser.RiskToleranceScore;
                ModelState.Remove(nameof(Models.UserProfile.RiskToleranceScore));
            }

            if (IsEditingOwnProfile(existingUser) && await IsGoogleAccountAsync(existingUser))
            {
                user.Email = existingUser.Email;
                ModelState.Remove(nameof(Models.UserProfile.Email));
            }

            if (!ModelState.IsValid)
            {
                await LoadEditPermissionsAsync(existingUser);
                user.AppUserId = existingUser.AppUserId;
                user.CreatedAt = existingUser.CreatedAt;
                return View(user);
            }

            user.AppUserId = existingUser.AppUserId;
            user.CreatedAt = existingUser.CreatedAt;
            _userProfileRepository.Update(user);
            _logger.LogInformation("User profile {UserProfileId} updated.", user.Id);
            return RedirectToAction(nameof(Details), new { id = user.Id });
        }

        [Authorize]
        [HttpGet("delete")]
        public IActionResult Delete(int id)
        {
            var user = _userProfileRepository.GetById(id);
            if (user is null)
            {
                _logger.LogWarning("User profile delete requested for missing id {UserProfileId}.", id);
                return NotFound();
            }

            if (!CanManageUserProfile(user))
            {
                _logger.LogWarning("User profile {UserProfileId} delete forbidden for current user.", id);
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
                _logger.LogWarning("User profile delete submitted for missing id {UserProfileId}.", model.Id);
                return NotFound();
            }

            if (!CanManageUserProfile(user))
            {
                _logger.LogWarning("User profile {UserProfileId} delete submit forbidden for current user.", model.Id);
                return Forbid();
            }

            try
            {
                _userProfileRepository.Delete(user);
                _logger.LogInformation("User profile {UserProfileId} deleted.", user.Id);
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogWarning(ex, "User profile {UserProfileId} could not be deleted because related data exists.", user.Id);
                ModelState.AddModelError(string.Empty, "Unable to delete this user profile because related data exists.");
                return View(user);
            }
        }
    }
}
