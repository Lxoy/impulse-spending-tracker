using impulse_spending_tracker.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace impulse_spending_tracker.Controllers
{
    public class UserProfilesController : Controller
    {
        private readonly UserProfileMockRepository _userProfileRepository;

        public UserProfilesController(UserProfileMockRepository userProfileRepository)
        {
            _userProfileRepository = userProfileRepository;
        }

        public IActionResult Index()
        {
            var users = _userProfileRepository
                .GetAll()
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ToList();

            return View(users);
        }

        public IActionResult Details(Guid id)
        {
            var user = _userProfileRepository.GetById(id);
            if (user is null)
            {
                return NotFound();
            }

            return View(user);
        }
    }
}