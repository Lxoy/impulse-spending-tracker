using impulse_spending_tracker.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace impulse_spending_tracker.Controllers
{
    public class SpendingSessionsController : Controller
    {
        private readonly SpendingSessionMockRepository _sessionRepository;

        public SpendingSessionsController(SpendingSessionMockRepository sessionRepository)
        {
            _sessionRepository = sessionRepository;
        }

        public IActionResult Index()
        {
            var sessions = _sessionRepository
                .GetAll()
                .OrderByDescending(s => s.StartedAt)
                .ThenByDescending(s => s.CheckoutCompleted)
                .ToList();

            return View(sessions);
        }

        public IActionResult Details(Guid id)
        {
            var session = _sessionRepository.GetById(id);
            if (session is null)
            {
                return NotFound();
            }

            return View(session);
        }
    }
}
