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
        private readonly ILogger<MerchantsController> _logger;

        public MerchantsController(
            MerchantRepository merchantRepository,
            ILogger<MerchantsController> logger)
        {
            _merchantRepository = merchantRepository;
            _logger = logger;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            var merchants = _merchantRepository
                .GetAll()
                .OrderBy(m => m.Name)
                .ThenBy(m => m.Category)
                .ToList();

            _logger.LogInformation("Loaded {Count} merchants.", merchants.Count);
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
                _logger.LogWarning("Merchant details requested for missing id {MerchantId}.", id);
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
                _logger.LogWarning("Merchant create validation failed for {MerchantName}.", merchant.Name);
                return View(merchant);
            }

            _merchantRepository.Create(merchant);
            _logger.LogInformation("Merchant {MerchantId} created: {MerchantName}.", merchant.Id, merchant.Name);
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("edit")]
        public IActionResult Edit(int id)
        {
            var merchant = _merchantRepository.GetById(id);
            if (merchant is null)
            {
                _logger.LogWarning("Merchant edit requested for missing id {MerchantId}.", id);
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
                _logger.LogWarning("Merchant edit validation failed for id {MerchantId}.", merchant.Id);
                return View(merchant);
            }

            _merchantRepository.Update(merchant);
            _logger.LogInformation("Merchant {MerchantId} updated.", merchant.Id);
            return RedirectToAction(nameof(Details), new { id = merchant.Id });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("delete")]
        public IActionResult Delete(int id)
        {
            var merchant = _merchantRepository.GetById(id);
            if (merchant is null)
            {
                _logger.LogWarning("Merchant delete requested for missing id {MerchantId}.", id);
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
                _logger.LogWarning("Merchant delete submitted for missing id {MerchantId}.", model.Id);
                return NotFound();
            }

            try
            {
                _merchantRepository.Delete(merchant);
                _logger.LogInformation("Merchant {MerchantId} deleted.", merchant.Id);
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogWarning(ex, "Merchant {MerchantId} could not be deleted because related purchases exist.", merchant.Id);
                ModelState.AddModelError(string.Empty, "Unable to delete this merchant because related purchases exist.");
                return View(merchant);
            }
        }
    }
}
