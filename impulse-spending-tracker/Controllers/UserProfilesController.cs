using impulse_spending_tracker.Repositories;
using Microsoft.AspNetCore.Mvc;

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
    }
}