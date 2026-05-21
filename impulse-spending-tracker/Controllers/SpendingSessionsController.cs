using impulse_spending_tracker.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace impulse_spending_tracker.Controllers
{
    [Route("sessions")]
    public class SpendingSessionsController : Controller
    {
        private readonly SpendingSessionRepository _sessionRepository;
        private readonly UserProfileRepository _userProfileRepository;

        public SpendingSessionsController(
            SpendingSessionRepository sessionRepository,
            UserProfileRepository userProfileRepository)
        {
            _sessionRepository = sessionRepository;
            _userProfileRepository = userProfileRepository;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            var sessions = _sessionRepository
                .GetAll()
                .OrderByDescending(s => s.StartedAt)
                .ThenByDescending(s => s.CheckoutCompleted)
                .ToList();

            return View(sessions);
        }

        [HttpGet("filter")]
        public IActionResult Filter(string query)
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

        [HttpGet("{id:int}")]
        public IActionResult Details(int id)
        {
            var session = _sessionRepository.GetById(id);
            if (session is null)
            {
                return NotFound();
            }

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

        [HttpGet("create")]
        public IActionResult Create()
        {
            LoadDropdownData();
            return View(new Models.SpendingSession());
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Models.SpendingSession session)
        {
            if (!ModelState.IsValid)
            {
                LoadDropdownData();
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

        [HttpGet("edit")]
        public IActionResult Edit(int id)
        {
            var session = _sessionRepository.GetById(id);
            if (session is null)
            {
                return NotFound();
            }

            LoadDropdownData();
            return View(session);
        }

        [HttpPost("edit")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Models.SpendingSession session)
        {
            if (!ModelState.IsValid)
            {
                LoadDropdownData();
                PopulateSelectedUser(session);
                return View(session);
            }

            var existingSession = _sessionRepository.GetById(session.Id);
            if (existingSession is null)
            {
                return NotFound();
            }

            session.StartedAt = existingSession.StartedAt;
            session.SpentAmount = existingSession.SpentAmount;
            session.ItemsViewed = existingSession.ItemsViewed;
            session.ItemsAddedToCart = existingSession.ItemsAddedToCart;
            _sessionRepository.Update(session);
            return RedirectToAction(nameof(Details), new { id = session.Id });
        }

        [HttpGet("delete")]
        public IActionResult Delete(int id)
        {
            var session = _sessionRepository.GetById(id);
            if (session is null)
            {
                return NotFound();
            }

            return View(session);
        }

        [HttpPost("delete")]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(Models.SpendingSession model)
        {
            var session = _sessionRepository.GetById(model.Id);
            if (session is null)
            {
                return NotFound();
            }

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
