using impulse_spending_tracker.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace impulse_spending_tracker.Controllers
{
    [Authorize]
    [Route("lookup")]
    public class LookupController : Controller
    {
        private readonly UserProfileRepository _userProfileRepository;
        private readonly MerchantRepository _merchantRepository;

        public LookupController(
            UserProfileRepository userProfileRepository,
            MerchantRepository merchantRepository)
        {
            _userProfileRepository = userProfileRepository;
            _merchantRepository = merchantRepository;
        }

        [HttpGet("users")]
        public IActionResult Users(string? query)
        {
            var users = _userProfileRepository
                .GetAll()
                .Where(u => string.IsNullOrWhiteSpace(query)
                            || u.FirstName.Contains(query, StringComparison.OrdinalIgnoreCase)
                            || u.LastName.Contains(query, StringComparison.OrdinalIgnoreCase)
                            || u.Email.Contains(query, StringComparison.OrdinalIgnoreCase))
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .Take(10)
                .Select(u => new
                {
                    id = u.Id,
                    text = $"{u.FirstName} {u.LastName} ({u.Email})"
                })
                .ToList();

            return Json(users);
        }

        [HttpGet("merchants")]
        public IActionResult Merchants(string? query)
        {
            var merchants = _merchantRepository
                .GetAll()
                .Where(m => string.IsNullOrWhiteSpace(query)
                            || m.Name.Contains(query, StringComparison.OrdinalIgnoreCase)
                            || m.Category.Contains(query, StringComparison.OrdinalIgnoreCase)
                            || m.CountryCode.Contains(query, StringComparison.OrdinalIgnoreCase))
                .OrderBy(m => m.Name)
                .ThenBy(m => m.Category)
                .Take(10)
                .Select(m => new
                {
                    id = m.Id,
                    text = $"{m.Name} ({m.Category}, {m.CountryCode})"
                })
                .ToList();

            return Json(merchants);
        }
    }
}
