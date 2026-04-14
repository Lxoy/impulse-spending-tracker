using impulse_spending_tracker.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace impulse_spending_tracker.Controllers
{
    public class PurchasesController : Controller
    {
        private readonly PurchaseMockRepository _purchaseRepository;

        public PurchasesController(PurchaseMockRepository purchaseRepository)
        {
            _purchaseRepository = purchaseRepository;
        }

        public IActionResult Index()
        {
            var purchases = _purchaseRepository
                .GetAll()
                .OrderByDescending(p => p.PurchasedAt)
                .ThenByDescending(p => p.Amount)
                .ToList();

            return View(purchases);
        }

        public IActionResult Details(Guid id)
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
