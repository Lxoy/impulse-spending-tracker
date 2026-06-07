using impulse_spending_tracker.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace impulse_spending_tracker.Controllers
{
    [Authorize]
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

        [HttpGet("filter")]
        public IActionResult Filter(string query)
        {
            var merchants = _merchantRepository
                .GetAll()
                .Where(m => string.IsNullOrEmpty(query) || 
                            m.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
                .OrderBy(m => m.Name)
                .ThenBy(m => m.Category)
                .ToList();

            return PartialView("_MerchantTableRows", merchants);
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

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("create")]
        public IActionResult Create()
        {
            return View(new Models.Merchant());
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Models.Merchant merchant)
        {
            if (!ModelState.IsValid)
            {
                return View(merchant);
            }

            _merchantRepository.Create(merchant);
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("edit")]
        public IActionResult Edit(int id)
        {
            var merchant = _merchantRepository.GetById(id);
            if (merchant is null)
            {
                return NotFound();
            }

            return View(merchant);
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost("edit")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Models.Merchant merchant)
        {
            if (!ModelState.IsValid)
            {
                return View(merchant);
            }

            _merchantRepository.Update(merchant);
            return RedirectToAction(nameof(Details), new { id = merchant.Id });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("delete")]
        public IActionResult Delete(int id)
        {
            var merchant = _merchantRepository.GetById(id);
            if (merchant is null)
            {
                return NotFound();
            }

            return View(merchant);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("delete")]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(Models.Merchant model)
        {
            var merchant = _merchantRepository.GetById(model.Id);
            if (merchant is null)
            {
                return NotFound();
            }

            try
            {
                _merchantRepository.Delete(merchant);
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "Unable to delete this merchant because related purchases exist.");
                return View(merchant);
            }
        }
    }
}
