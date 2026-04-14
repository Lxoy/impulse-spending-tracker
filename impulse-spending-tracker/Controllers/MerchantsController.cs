using impulse_spending_tracker.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace impulse_spending_tracker.Controllers
{
    public class MerchantsController : Controller
    {
        private readonly MerchantMockRepository _merchantRepository;

        public MerchantsController(MerchantMockRepository merchantRepository)
        {
            _merchantRepository = merchantRepository;
        }

        public IActionResult Index()
        {
            var merchants = _merchantRepository
                .GetAll()
                .OrderBy(m => m.Name)
                .ThenBy(m => m.Category)
                .ToList();

            return View(merchants);
        }

        public IActionResult Details(Guid id)
        {
            var merchant = _merchantRepository.GetById(id);
            if (merchant is null)
            {
                return NotFound();
            }

            return View(merchant);
        }
    }
}
