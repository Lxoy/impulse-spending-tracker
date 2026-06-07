using impulse_spending_tracker.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace impulse_spending_tracker.Controllers
{
    [Authorize]
    [Route("sessions")]
    public class SpendingSessionsController : Controller
    {
        private readonly SpendingSessionRepository _sessionRepository;
        private readonly UserProfileRepository _userProfileRepository;
        private readonly Microsoft.AspNetCore.Identity.UserManager<impulse_spending_tracker.Models.AppUser> _userManager;

        public SpendingSessionsController(
            SpendingSessionRepository sessionRepository,
            UserProfileRepository userProfileRepository,
            Microsoft.AspNetCore.Identity.UserManager<impulse_spending_tracker.Models.AppUser> userManager)
        {
            _sessionRepository = sessionRepository;
            _userProfileRepository = userProfileRepository;
            _userManager = userManager;
        }

        [Authorize]
        [HttpGet("")]
        public IActionResult Index()
        {
            if (User.IsInRole("Admin"))
            {
                var sessions = _sessionRepository
                    .GetAll()
                    .OrderByDescending(s => s.StartedAt)
                    .ThenByDescending(s => s.CheckoutCompleted)
                    .ToList();

                return View(sessions);
            }

            var profileId = GetCurrentUserProfileId();
            if (!profileId.HasValue) return Forbid();

            var sessionsForUser = _sessionRepository
                .GetAll()
                .Where(s => s.UserProfileId == profileId.Value)
                .OrderByDescending(s => s.StartedAt)
                .ThenByDescending(s => s.CheckoutCompleted)
                .ToList();

            return View(sessionsForUser);
        }

        [Authorize]
        [HttpGet("filter")]
        public IActionResult Filter(string query)
        {
            if (User.IsInRole("Admin"))
            {
                var sessions = _sessionRepository
                    .GetAll()
                    .Where(s => string.IsNullOrEmpty(query) ||
                                (s.UserProfile != null && (
                                    s.UserProfile.FirstName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                                    s.UserProfile.LastName.Contains(query, StringComparison.OrdinalIgnoreCase))) ||
                                s.Platform.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                                s.Channel.Contains(query, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(s => s.StartedAt)
                    .ThenByDescending(s => s.CheckoutCompleted)
                    .ToList();

                return PartialView("_SpendingSessionTableRows", sessions);
            }

            var profileId = GetCurrentUserProfileId();
            if (!profileId.HasValue) return Forbid();

            var sessionsFiltered = _sessionRepository
                .GetAll()
                .Where(s => s.UserProfileId == profileId.Value && (
                            string.IsNullOrEmpty(query) ||
                            (s.UserProfile != null && (
                                s.UserProfile.FirstName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                                s.UserProfile.LastName.Contains(query, StringComparison.OrdinalIgnoreCase))) ||
                            s.Platform.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                            s.Channel.Contains(query, StringComparison.OrdinalIgnoreCase)))
                .OrderByDescending(s => s.StartedAt)
                .ThenByDescending(s => s.CheckoutCompleted)
                .ToList();

            return PartialView("_SpendingSessionTableRows", sessionsFiltered);
        }

        [Authorize]
        [HttpGet("{id:int}")]
        public IActionResult Details(int id)
        {
            var session = _sessionRepository.GetById(id);
            if (session is null) return NotFound();
            if (!CanManageSession(session)) return Forbid();
            return View(session);
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

        private void PopulateSelectedUser(Models.SpendingSession session)
        {
            if (session.UserProfileId > 0)
            {
                session.UserProfile = _userProfileRepository.GetById(session.UserProfileId);
            }
        }

        private int? GetCurrentUserProfileId()
        {
            var currentUserId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(currentUserId)) return null;
            var profile = _userProfileRepository.GetAll().FirstOrDefault(p => p.AppUserId == currentUserId);
            return profile?.Id;
        }

        private bool CanManageSession(Models.SpendingSession session)
        {
            if (User.IsInRole("Admin")) return true;
            var profileId = GetCurrentUserProfileId();
            return profileId.HasValue && session.UserProfileId == profileId.Value;
        }

        [Authorize]
        [HttpGet("create")]
        public IActionResult Create()
        {
            if (User.IsInRole("Admin"))
            {
                LoadDropdownData();
                ViewBag.ShowUserSelector = true;
                return View(new Models.SpendingSession());
            }

            var profileId = GetCurrentUserProfileId();
            var model = new Models.SpendingSession { UserProfileId = profileId ?? 0 };
            ViewBag.ShowUserSelector = false;
            if (profileId.HasValue)
            {
                ViewBag.CurrentUserProfile = _userProfileRepository.GetById(profileId.Value);
            }
            return View(model);
        }

        [Authorize]
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Models.SpendingSession session)
        {
            if (!User.IsInRole("Admin"))
            {
                var profileId = GetCurrentUserProfileId();
                session.UserProfileId = profileId ?? 0;
            }

            if (!ModelState.IsValid)
            {
                if (User.IsInRole("Admin")) LoadDropdownData();
                if (!User.IsInRole("Admin"))
                {
                    var pid = GetCurrentUserProfileId();
                    if (pid.HasValue) ViewBag.CurrentUserProfile = _userProfileRepository.GetById(pid.Value);
                }
                PopulateSelectedUser(session);
                return View(session);
            }

            session.StartedAt = DateTime.Now;
            session.SpentAmount = 0m;
            session.ItemsViewed = 0;
            session.ItemsAddedToCart = 0;
            _sessionRepository.Create(session);
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        [HttpGet("edit")]
        public IActionResult Edit(int id)
        {
            var session = _sessionRepository.GetById(id);
            if (session is null) return NotFound();
            if (!CanManageSession(session)) return Forbid();
            if (User.IsInRole("Admin")) LoadDropdownData();
            else
            {
                ViewBag.ShowUserSelector = false;
                ViewBag.CurrentUserProfile = _userProfileRepository.GetById(session.UserProfileId);
                LoadDropdownData();
            }

            return View(session);
        }

        [Authorize]
        [HttpPost("edit")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Models.SpendingSession session)
        {
            var existingSession = _sessionRepository.GetById(session.Id);
            if (existingSession is null) return NotFound();
            if (!CanManageSession(existingSession)) return Forbid();

            // prevent non-admins from changing owner
            if (!User.IsInRole("Admin")) session.UserProfileId = existingSession.UserProfileId;

            if (!ModelState.IsValid)
            {
                if (User.IsInRole("Admin")) LoadDropdownData();
                if (!User.IsInRole("Admin")) ViewBag.CurrentUserProfile = _userProfileRepository.GetById(session.UserProfileId);
                PopulateSelectedUser(session);
                return View(session);
            }

            session.StartedAt = existingSession.StartedAt;
            session.SpentAmount = existingSession.SpentAmount;
            session.ItemsViewed = existingSession.ItemsViewed;
            session.ItemsAddedToCart = existingSession.ItemsAddedToCart;
            _sessionRepository.Update(session);
            return RedirectToAction(nameof(Details), new { id = session.Id });
        }

        [Authorize]
        [HttpGet("delete")]
        public IActionResult Delete(int id)
        {
            var session = _sessionRepository.GetById(id);
            if (session is null) return NotFound();
            if (!CanManageSession(session)) return Forbid();
            return View(session);
        }

        [Authorize]
        [HttpPost("delete")]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(Models.SpendingSession model)
        {
            var session = _sessionRepository.GetById(model.Id);
            if (session is null) return NotFound();
            if (!CanManageSession(session)) return Forbid();

            try
            {
                _sessionRepository.Delete(session);
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "Unable to delete this session because related data exists.");
                return View(session);
            }
        }
    }
}
