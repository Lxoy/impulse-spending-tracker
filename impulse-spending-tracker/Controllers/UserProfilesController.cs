using impulse_spending_tracker.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace impulse_spending_tracker.Controllers
{
    [Route("users")]
    public class UserProfilesController : Controller
    {
        private readonly UserProfileRepository _userProfileRepository;

        public UserProfilesController(UserProfileRepository userProfileRepository)
        {
            _userProfileRepository = userProfileRepository;
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

            return View(user);
        }

        [HttpGet("create")]
        public IActionResult Create()
        {
            return View(new Models.UserProfile());
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Models.UserProfile user)
        {
            if (!ModelState.IsValid)
            {
                return View(user);
            }

            user.CreatedAt = DateTime.UtcNow;
            _userProfileRepository.Create(user);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("edit")]
        public IActionResult Edit(int id)
        {
            var user = _userProfileRepository.GetById(id);
            if (user is null)
            {
                return NotFound();
            }

            return View(user);
        }

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

            user.CreatedAt = existingUser.CreatedAt;
            _userProfileRepository.Update(user);
            return RedirectToAction(nameof(Details), new { id = user.Id });
        }

        [HttpGet("delete")]
        public IActionResult Delete(int id)
        {
            var user = _userProfileRepository.GetById(id);
            if (user is null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost("delete")]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(Models.UserProfile model)
        {
            var user = _userProfileRepository.GetById(model.Id);
            if (user is null)
            {
                return NotFound();
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