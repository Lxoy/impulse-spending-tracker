using impulse_spending_tracker.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace impulse_spending_tracker.Controllers
{
    [Route("merchants")]
    public class MerchantsController : Controller
    {
        private readonly MerchantRepository _merchantRepository;

        public MerchantsController(MerchantRepository merchantRepository)
        {
            _merchantRepository = merchantRepository;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            var merchants = _merchantRepository
                .GetAll()
                .OrderBy(m => m.Name)
                .ThenBy(m => m.Category)
                .ToList();

            return View(merchants);
        }

        [HttpGet("{id:int}")]
        public IActionResult Details(int id)
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
