using impulse_spending_tracker.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace impulse_spending_tracker.Controllers
{
    [Route("purchases-log")]
    public class PurchasesController : Controller
    {
        private readonly PurchaseRepository _purchaseRepository;

        public PurchasesController(PurchaseRepository purchaseRepository)
        {
            _purchaseRepository = purchaseRepository;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            var purchases = _purchaseRepository
                .GetAll()
                .OrderByDescending(p => p.PurchasedAt)
                .ThenByDescending(p => p.Amount)
                .ToList();

            return View(purchases);
        }

        [HttpGet("{id:int}")]
        public IActionResult Details(int id)
        {
            var purchase = _purchaseRepository.GetById(id);
            if (purchase is null)
            {
                return NotFound();
            }

            return View(purchase);
        }
    }
}
