using impulse_spending_tracker.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace impulse_spending_tracker.Controllers
{
    [Route("sessions")]
    public class SpendingSessionsController : Controller
    {
        private readonly SpendingSessionRepository _sessionRepository;

        public SpendingSessionsController(SpendingSessionRepository sessionRepository)
        {
            _sessionRepository = sessionRepository;
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
    }
}
