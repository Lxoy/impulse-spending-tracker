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

        private static List<SelectListItem> CreateOptionalSelectList(IEnumerable<SelectListItem> items, string placeholder)
        {
            var list = new List<SelectListItem>
            {
                new SelectListItem { Value = string.Empty, Text = placeholder }
            };

            list.AddRange(items);
            return list;
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

        private IReadOnlyList<SelectListItem> BuildSpendingSessionOptions(int? userProfileId)
        {
            if (!userProfileId.HasValue || userProfileId.Value <= 0)
            {
                return Array.Empty<SelectListItem>();
            }

            return _spendingSessionRepository.GetAll()
                .Where(s => s.UserProfileId == userProfileId.Value)
                .OrderByDescending(s => s.StartedAt)
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = $"Session on {s.StartedAt:yyyy-MM-dd} (ID: {s.Id})"
                })
                .ToList();
        }

        private IReadOnlyList<SelectListItem> BuildBudgetPlanOptions(int? userProfileId)
        {
            if (!userProfileId.HasValue || userProfileId.Value <= 0)
            {
                return Array.Empty<SelectListItem>();
            }

            return _budgetPlanRepository.GetAll()
                .Where(b => b.UserProfileId == userProfileId.Value)
                .OrderBy(b => b.Name)
                .Select(b => new SelectListItem
                {
                    Value = b.Id.ToString(),
                    Text = $"{b.Name} (ID: {b.Id})"
                })
                .ToList();
        }

        private IReadOnlyList<SelectListItem> BuildWishlistItemOptions(int? userProfileId)
        {
            if (!userProfileId.HasValue || userProfileId.Value <= 0)
            {
                return Array.Empty<SelectListItem>();
            }

            return _wishlistItemRepository.GetAll()
                .Where(w => w.UserProfileId == userProfileId.Value)
                .OrderBy(w => w.Name)
                .Select(w => new SelectListItem
                {
                    Value = w.Id.ToString(),
                    Text = $"{w.Name} (ID: {w.Id})"
                })
                .ToList();
        }

        [HttpGet("dependent-options")]
        public IActionResult DependentOptions(int userProfileId)
        {
            var spendingSessions = BuildSpendingSessionOptions(userProfileId)
                .Select(option => new { id = option.Value, text = option.Text })
                .ToList();

            var budgetPlans = BuildBudgetPlanOptions(userProfileId)
                .Select(option => new { id = option.Value, text = option.Text })
                .ToList();

            var wishlistItems = BuildWishlistItemOptions(userProfileId)
                .Select(option => new { id = option.Value, text = option.Text })
                .ToList();

            var wishlistPrices = _wishlistItemRepository.GetAll()
                .Where(w => w.UserProfileId == userProfileId)
                .Select(w => new
                {
                    id = w.Id.ToString(),
                    currentPrice = w.CurrentPrice
                })
                .ToList();

            return Json(new
            {
                spendingSessions,
                budgetPlans,
                wishlistItems,
                wishlistPrices
            });
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

        private void LoadDropdownData(int? userProfileId = null)
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

            ViewBag.UserProfileId = users;
            ViewBag.MerchantId = merchants;

            ViewBag.SpendingSessionId = CreateOptionalSelectList(BuildSpendingSessionOptions(userProfileId), "-- Select (Optional) --");
            ViewBag.BudgetPlanId = CreateOptionalSelectList(BuildBudgetPlanOptions(userProfileId), "-- Select (Optional) --");
            ViewBag.WishlistItemId = CreateOptionalSelectList(BuildWishlistItemOptions(userProfileId), "-- Select (Optional) --");
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

        private bool ValidateUserScopedSelections(Models.Purchase purchase)
        {
            var isValid = true;

            if (purchase.SpendingSessionId.HasValue)
            {
                var session = _spendingSessionRepository.GetById(purchase.SpendingSessionId.Value);
                if (session is null || session.UserProfileId != purchase.UserProfileId)
                {
                    ModelState.AddModelError(nameof(Models.Purchase.SpendingSessionId), "Select a spending session that belongs to the selected user.");
                    isValid = false;
                }
            }

            if (purchase.BudgetPlanId.HasValue)
            {
                var budgetPlan = _budgetPlanRepository.GetById(purchase.BudgetPlanId.Value);
                if (budgetPlan is null || budgetPlan.UserProfileId != purchase.UserProfileId)
                {
                    ModelState.AddModelError(nameof(Models.Purchase.BudgetPlanId), "Select a budget plan that belongs to the selected user.");
                    isValid = false;
                }
            }

            if (purchase.WishlistItemId.HasValue)
            {
                var wishlistItem = _wishlistItemRepository.GetById(purchase.WishlistItemId.Value);
                if (wishlistItem is null || wishlistItem.UserProfileId != purchase.UserProfileId)
                {
                    ModelState.AddModelError(nameof(Models.Purchase.WishlistItemId), "Select a wishlist item that belongs to the selected user.");
                    isValid = false;
                }
            }

            return isValid;
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
            if (!ModelState.IsValid || !ValidateUserScopedSelections(purchase))
            {
                LoadDropdownData(purchase.UserProfileId > 0 ? purchase.UserProfileId : null);
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

            LoadDropdownData(purchase.UserProfileId);
            return View(purchase);
        }

        [HttpPost("edit")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Models.Purchase purchase)
        {
            if (!ModelState.IsValid || !ValidateUserScopedSelections(purchase))
            {
                LoadDropdownData(purchase.UserProfileId > 0 ? purchase.UserProfileId : null);
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
