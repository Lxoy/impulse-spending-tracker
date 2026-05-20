using impulse_spending_tracker.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace impulse_spending_tracker.Controllers
{
    [Route("purchases-log")]
    public class PurchasesController : Controller
    {
        private readonly PurchaseRepository _purchaseRepository;
        private readonly UserProfileRepository _userProfileRepository;
        private readonly MerchantRepository _merchantRepository;
        private readonly SpendingSessionRepository _spendingSessionRepository;
        private readonly BudgetPlanRepository _budgetPlanRepository;
        private readonly WishlistItemRepository _wishlistItemRepository;

        public PurchasesController(
            PurchaseRepository purchaseRepository,
            UserProfileRepository userProfileRepository,
            MerchantRepository merchantRepository,
            SpendingSessionRepository spendingSessionRepository,
            BudgetPlanRepository budgetPlanRepository,
            WishlistItemRepository wishlistItemRepository)
        {
            _purchaseRepository = purchaseRepository;
            _userProfileRepository = userProfileRepository;
            _merchantRepository = merchantRepository;
            _spendingSessionRepository = spendingSessionRepository;
            _budgetPlanRepository = budgetPlanRepository;
            _wishlistItemRepository = wishlistItemRepository;
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

        [HttpGet("filter")]
        public IActionResult Filter(string query)
        {
            var purchases = _purchaseRepository
                .GetAll()
                .Where(p => string.IsNullOrEmpty(query) || 
                            p.Title.Contains(query, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(p => p.PurchasedAt)
                .ThenByDescending(p => p.Amount)
                .ToList();

            return PartialView("_PurchaseTableRows", purchases);
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

        private void LoadDropdownData()
        {
            var users = _userProfileRepository.GetAll()
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = $"{u.FirstName} {u.LastName} (ID: {u.Id})"
                })
                .ToList();

            var merchants = _merchantRepository.GetAll()
                .OrderBy(m => m.Name)
                .Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = $"{m.Name} (ID: {m.Id})"
                })
                .ToList();

            var sessions = _spendingSessionRepository.GetAll()
                .OrderByDescending(s => s.StartedAt)
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = $"Session on {s.StartedAt:yyyy-MM-dd} (ID: {s.Id})"
                })
                .ToList();

            var budgetPlans = _budgetPlanRepository.GetAll()
                .OrderBy(b => b.Name)
                .Select(b => new SelectListItem
                {
                    Value = b.Id.ToString(),
                    Text = $"{b.Name} (ID: {b.Id})"
                })
                .ToList();

            var wishlistItems = _wishlistItemRepository.GetAll()
                .OrderBy(w => w.Name)
                .Select(w => new SelectListItem
                {
                    Value = w.Id.ToString(),
                    Text = $"{w.Name} (ID: {w.Id})"
                })
                .ToList();

            ViewBag.UserProfileId = users;
            ViewBag.MerchantId = merchants;
            
            var sessionsList = new List<SelectListItem> { new SelectListItem { Value = "", Text = "-- Select (Optional) --" } };
            sessionsList.AddRange(sessions);
            ViewBag.SpendingSessionId = sessionsList;
            
            var budgetPlansList = new List<SelectListItem> { new SelectListItem { Value = "", Text = "-- Select (Optional) --" } };
            budgetPlansList.AddRange(budgetPlans);
            ViewBag.BudgetPlanId = budgetPlansList;
            
            var wishlistItemsList = new List<SelectListItem> { new SelectListItem { Value = "", Text = "-- Select (Optional) --" } };
            wishlistItemsList.AddRange(wishlistItems);
            ViewBag.WishlistItemId = wishlistItemsList;
        }

        private void PopulateSelectedEntities(Models.Purchase purchase)
        {
            if (purchase.UserProfileId > 0)
            {
                purchase.UserProfile = _userProfileRepository.GetById(purchase.UserProfileId);
            }

            if (purchase.MerchantId > 0)
            {
                purchase.Merchant = _merchantRepository.GetById(purchase.MerchantId);
            }
        }

        [HttpGet("create")]
        public IActionResult Create()
        {
            LoadDropdownData();
            return View(new Models.Purchase { PurchasedAt = DateTime.Now });
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Models.Purchase purchase)
        {
            if (!ModelState.IsValid)
            {
                LoadDropdownData();
                PopulateSelectedEntities(purchase);
                return View(purchase);
            }

            _purchaseRepository.Create(purchase);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("edit")]
        public IActionResult Edit(int id)
        {
            var purchase = _purchaseRepository.GetById(id);
            if (purchase is null)
            {
                return NotFound();
            }

            LoadDropdownData();
            return View(purchase);
        }

        [HttpPost("edit")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Models.Purchase purchase)
        {
            if (!ModelState.IsValid)
            {
                LoadDropdownData();
                PopulateSelectedEntities(purchase);
                return View(purchase);
            }

            var existingPurchase = _purchaseRepository.GetById(purchase.Id);
            if (existingPurchase is null)
            {
                return NotFound();
            }

            purchase.PurchasedAt = existingPurchase.PurchasedAt;
            _purchaseRepository.Update(purchase);
            return RedirectToAction(nameof(Details), new { id = purchase.Id });
        }

        [HttpGet("delete")]
        public IActionResult Delete(int id)
        {
            var purchase = _purchaseRepository.GetById(id);
            if (purchase is null)
            {
                return NotFound();
            }

            return View(purchase);
        }

        [HttpPost("delete")]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(Models.Purchase model)
        {
            var purchase = _purchaseRepository.GetById(model.Id);
            if (purchase is null)
            {
                return NotFound();
            }

            try
            {
                _purchaseRepository.Delete(purchase);
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "Unable to delete this purchase because related data exists.");
                return View(purchase);
            }
        }
    }
}
