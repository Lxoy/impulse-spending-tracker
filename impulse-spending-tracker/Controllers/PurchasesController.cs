using impulse_spending_tracker.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace impulse_spending_tracker.Controllers
{
    [Authorize]
    [Route("purchases-log")]
    public class PurchasesController : Controller
    {
        private readonly PurchaseRepository _purchaseRepository;
        private readonly UserProfileRepository _userProfileRepository;
        private readonly MerchantRepository _merchantRepository;
        private readonly SpendingSessionRepository _spendingSessionRepository;
        private readonly BudgetPlanRepository _budgetPlanRepository;
        private readonly WishlistItemRepository _wishlistItemRepository;
        private readonly TriggerTypeRepository _tagRepository;
        private readonly PurchaseAttachmentRepository _purchaseAttachmentRepository;
        private readonly Microsoft.AspNetCore.Identity.UserManager<impulse_spending_tracker.Models.AppUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<PurchasesController> _logger;

        public PurchasesController(
            PurchaseRepository purchaseRepository,
            UserProfileRepository userProfileRepository,
            MerchantRepository merchantRepository,
            SpendingSessionRepository spendingSessionRepository,
            BudgetPlanRepository budgetPlanRepository,
            WishlistItemRepository wishlistItemRepository,
            TriggerTypeRepository tagRepository,
            PurchaseAttachmentRepository purchaseAttachmentRepository,
            Microsoft.AspNetCore.Identity.UserManager<impulse_spending_tracker.Models.AppUser> userManager,
            IWebHostEnvironment webHostEnvironment,
            ILogger<PurchasesController> logger)
        {
            _purchaseRepository = purchaseRepository;
            _userProfileRepository = userProfileRepository;
            _merchantRepository = merchantRepository;
            _spendingSessionRepository = spendingSessionRepository;
            _budgetPlanRepository = budgetPlanRepository;
            _wishlistItemRepository = wishlistItemRepository;
            _tagRepository = tagRepository;
            _purchaseAttachmentRepository = purchaseAttachmentRepository;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        [Authorize]
        [HttpGet("")]
        public IActionResult Index()
        {
            if (User.IsInRole("Admin"))
            {
                var purchases = _purchaseRepository
                    .GetAll()
                    .OrderByDescending(p => p.PurchasedAt)
                    .ThenByDescending(p => p.Amount)
                    .ToList();

                return View(purchases);
            }

            var profileId = GetCurrentUserProfileId();
            if (!profileId.HasValue) return Forbid();

            var userPurchases = _purchaseRepository
                .GetAll()
                .Where(p => p.UserProfileId == profileId.Value)
                .OrderByDescending(p => p.PurchasedAt)
                .ThenByDescending(p => p.Amount)
                .ToList();

            return View(userPurchases);
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

        [Authorize]
        [HttpGet("filter")]
        public IActionResult Filter(string query)
        {
            if (User.IsInRole("Admin"))
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

            var profileId = GetCurrentUserProfileId();
            if (!profileId.HasValue) return Forbid();

            var purchasesFiltered = _purchaseRepository
                .GetAll()
                .Where(p => p.UserProfileId == profileId.Value &&
                            (string.IsNullOrEmpty(query) || p.Title.Contains(query, StringComparison.OrdinalIgnoreCase)))
                .OrderByDescending(p => p.PurchasedAt)
                .ThenByDescending(p => p.Amount)
                .ToList();

            return PartialView("_PurchaseTableRows", purchasesFiltered);
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

        [Authorize]
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

        [Authorize]
        [HttpGet("{id:int}/attachments")]
        public IActionResult GetAttachments(int id)
        {
            try
            {
                var purchase = _purchaseRepository.GetByIdBasic(id);
                if (purchase is null)
                {
                    return NotFound();
                }

                if (!CanManagePurchase(purchase))
                {
                    return Forbid();
                }

                var attachments = _purchaseAttachmentRepository.GetByPurchaseId(id);
                return PartialView("_AttachmentList", attachments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch attachments for purchase {PurchaseId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Unable to load attachments.");
            }
        }

        [Authorize]
        [HttpPost("attachments/upload")]
        [RequestSizeLimit(10 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 10 * 1024 * 1024)]
        public async Task<IActionResult> UploadAttachment(int purchaseId, IFormFile? file)
        {
            try
            {
                var purchase = _purchaseRepository.GetByIdBasic(purchaseId);
                if (purchase is null)
                {
                    return NotFound();
                }

                if (!CanManagePurchase(purchase))
                {
                    return Forbid();
                }

                if (file is null || file.Length == 0)
                {
                    return BadRequest(new { success = false, message = "No file was uploaded." });
                }

                const long maxFileSize = 10 * 1024 * 1024;
                if (file.Length > maxFileSize)
                {
                    return BadRequest(new { success = false, message = "File exceeds 10 MB limit." });
                }

                var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    ".jpg", ".jpeg", ".png", ".webp", ".pdf"
                };

                var extension = Path.GetExtension(file.FileName);
                if (string.IsNullOrWhiteSpace(extension) || !allowedExtensions.Contains(extension))
                {
                    return BadRequest(new { success = false, message = "Only JPG, PNG, WEBP, and PDF files are allowed." });
                }

                var originalFileName = Path.GetFileName(file.FileName);
                if (string.IsNullOrWhiteSpace(originalFileName))
                {
                    originalFileName = "attachment" + extension;
                }
                else if (originalFileName.Length > 255)
                {
                    originalFileName = originalFileName[..255];
                }

                var contentType = string.IsNullOrWhiteSpace(file.ContentType)
                    ? "application/octet-stream"
                    : file.ContentType.Trim();

                if (contentType.Length > 120)
                {
                    contentType = contentType[..120];
                }

                var uploadsRoot = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "impulse-spending-tracker",
                    "uploads",
                    "purchases",
                    purchaseId.ToString());
                Directory.CreateDirectory(uploadsRoot);

                var storedFileName = $"{Guid.NewGuid():N}{extension}";
                var physicalPath = Path.Combine(uploadsRoot, storedFileName);

                using (var stream = new FileStream(physicalPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var attachment = new Models.PurchaseAttachment
                {
                    PurchaseId = purchaseId,
                    FileName = originalFileName,
                    FilePath = physicalPath,
                    ContentType = contentType,
                    FileSize = file.Length,
                    CreatedAt = DateTime.UtcNow
                };

                try
                {
                    _purchaseAttachmentRepository.Create(attachment);
                }
                catch
                {
                    if (System.IO.File.Exists(physicalPath))
                    {
                        System.IO.File.Delete(physicalPath);
                    }

                    throw;
                }

                return Json(new
                {
                    success = true,
                    attachmentId = attachment.Id,
                    originalName = attachment.FileName
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload attachment for purchase {PurchaseId}.", purchaseId);
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "Upload failed due to server error."
                });
            }
        }

        [Authorize]
        [HttpPost("attachments/delete")]
        public IActionResult DeleteAttachment(int id)
        {
            try
            {
                var attachment = _purchaseAttachmentRepository.GetById(id);
                if (attachment is null)
                {
                    return NotFound();
                }

                if (attachment.Purchase is null || !CanManagePurchase(attachment.Purchase))
                {
                    return Forbid();
                }

                if (!string.IsNullOrWhiteSpace(attachment.FilePath) && System.IO.File.Exists(attachment.FilePath))
                {
                    System.IO.File.Delete(attachment.FilePath);
                }

                _purchaseAttachmentRepository.Delete(attachment);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete attachment {AttachmentId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "Delete failed due to server error."
                });
            }
        }

        [Authorize]
        [HttpGet("attachments/file/{id:int}")]
        public IActionResult AttachmentFile(int id)
        {
            var attachment = _purchaseAttachmentRepository.GetById(id);
            if (attachment is null)
            {
                return NotFound();
            }

            if (attachment.Purchase is null || !CanManagePurchase(attachment.Purchase))
            {
                return Forbid();
            }

            if (string.IsNullOrWhiteSpace(attachment.FilePath) || !System.IO.File.Exists(attachment.FilePath))
            {
                return NotFound();
            }

            return PhysicalFile(attachment.FilePath, attachment.ContentType, attachment.FileName);
        }

        [Authorize]
        [HttpGet("{id:int}")]
        public IActionResult Details(int id)
        {
            var purchase = _purchaseRepository.GetById(id);
            if (purchase is null) return NotFound();
            if (!CanManagePurchase(purchase)) return Forbid();
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
            ViewBag.TriggerTypeId = _tagRepository.GetAll()
                .OrderBy(tag => tag.Name)
                .Select(tag => new SelectListItem
                {
                    Value = tag.Id.ToString(),
                    Text = tag.Name
                })
                .ToList();
        }

        private int? GetCurrentUserProfileId()
        {
            var currentUserId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(currentUserId)) return null;
            var profile = _userProfileRepository.GetAll().FirstOrDefault(p => p.AppUserId == currentUserId);
            return profile?.Id;
        }

        private bool CanManagePurchase(Models.Purchase purchase)
        {
            if (User.IsInRole("Admin")) return true;
            var profileId = GetCurrentUserProfileId();
            return profileId.HasValue && purchase.UserProfileId == profileId.Value;
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

            if (purchase.WishlistItemId.HasValue && _purchaseRepository.IsWishlistItemLinked(purchase.WishlistItemId.Value, purchase.Id > 0 ? purchase.Id : null))
            {
                ModelState.AddModelError(nameof(Models.Purchase.WishlistItemId), "Selected wishlist item is already converted to a purchase.");
                isValid = false;
            }

            if (purchase.BudgetPlanId.HasValue)
            {
                var budgetPlan = _budgetPlanRepository.GetById(purchase.BudgetPlanId.Value);
                if (budgetPlan is null || budgetPlan.UserProfileId != purchase.UserProfileId)
                {
                    ModelState.AddModelError(nameof(Models.Purchase.BudgetPlanId), "Select a budget plan that belongs to the selected user.");
                    isValid = false;
                }
                else
                {
                    var spentOnPlan = _purchaseRepository.GetBudgetPlanSpentAmount(purchase.BudgetPlanId.Value, purchase.Id > 0 ? purchase.Id : null);
                    if (spentOnPlan + purchase.Amount > budgetPlan.MonthlyLimit)
                    {
                        ModelState.AddModelError(nameof(Models.Purchase.BudgetPlanId), $"Budget plan limit would be exceeded. Current total is {spentOnPlan:F2} EUR and this purchase would push it above {budgetPlan.MonthlyLimit:F2} EUR.");
                        isValid = false;
                    }
                }
            }

            return isValid;
        }

        [Authorize]
        [HttpGet("create")]
        public IActionResult Create()
        {
            if (User.IsInRole("Admin"))
            {
                LoadDropdownData();
                ViewBag.ShowUserSelector = true;
                return View(new Models.Purchase { PurchasedAt = DateTime.Now });
            }

            var profileId = GetCurrentUserProfileId();
            var model = new Models.Purchase { PurchasedAt = DateTime.Now, UserProfileId = profileId ?? 0 };
            LoadDropdownData(profileId > 0 ? profileId : null);
            ViewBag.ShowUserSelector = false;
            if (profileId.HasValue)
            {
                ViewBag.CurrentUserProfile = _userProfileRepository.GetById(profileId.Value);
            }
            return View(model);
        }

        [Authorize]
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Models.Purchase purchase)
        {
            if (!User.IsInRole("Admin"))
            {
                var profileId = GetCurrentUserProfileId();
                purchase.UserProfileId = profileId ?? 0;
            }

            // Prevent purchase dates in the future
            try
            {
                if (purchase.PurchasedAt > DateTime.Now)
                {
                    ModelState.AddModelError(nameof(Models.Purchase.PurchasedAt), "Purchase date cannot be in the future.");
                }
            }
            catch
            {
                // ignore parse issues here; ModelState will catch invalid values
            }

            if (!ModelState.IsValid || !ValidateUserScopedSelections(purchase))
            {
                LoadDropdownData(purchase.UserProfileId > 0 ? purchase.UserProfileId : null);
                if (!User.IsInRole("Admin"))
                {
                    var pid = GetCurrentUserProfileId();
                    if (pid.HasValue) ViewBag.CurrentUserProfile = _userProfileRepository.GetById(pid.Value);
                }
                PopulateSelectedEntities(purchase);
                return View(purchase);
            }

            _purchaseRepository.Create(purchase);
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        [HttpGet("edit")]
        public IActionResult Edit(int id)
        {
            var purchase = _purchaseRepository.GetById(id);
            if (purchase is null) return NotFound();
            if (!CanManagePurchase(purchase)) return Forbid();
            if (User.IsInRole("Admin"))
            {
                LoadDropdownData(purchase.UserProfileId);
            }
            else
            {
                ViewBag.ShowUserSelector = false;
                ViewBag.CurrentUserProfile = _userProfileRepository.GetById(purchase.UserProfileId);
                LoadDropdownData(purchase.UserProfileId);
            }

            return View(purchase);
        }

        [Authorize]
        [HttpPost("edit")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Models.Purchase purchase)
        {
            try
            {
                var existingPurchase = _purchaseRepository.GetById(purchase.Id);
                if (existingPurchase is null) return NotFound();
                if (!CanManagePurchase(existingPurchase)) return Forbid();

                // prevent non-admins from changing owner
                if (!User.IsInRole("Admin")) purchase.UserProfileId = existingPurchase.UserProfileId;

                if (!ModelState.IsValid || !ValidateUserScopedSelections(purchase))
                {
                    LoadDropdownData(purchase.UserProfileId > 0 ? purchase.UserProfileId : null);
                    PopulateSelectedEntities(purchase);
                    return View(purchase);
                }

                purchase.PurchasedAt = existingPurchase.PurchasedAt;
                _purchaseRepository.Update(purchase);
                return RedirectToAction(nameof(Details), new { id = purchase.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to edit purchase {PurchaseId}.", purchase.Id);
                ModelState.AddModelError(string.Empty, "An unexpected server error occurred while saving changes.");
                LoadDropdownData(purchase.UserProfileId > 0 ? purchase.UserProfileId : null);
                PopulateSelectedEntities(purchase);
                return View(purchase);
            }
        }

        [Authorize]
        [HttpGet("delete")]
        public IActionResult Delete(int id)
        {
            var purchase = _purchaseRepository.GetById(id);
            if (purchase is null) return NotFound();
            if (!CanManagePurchase(purchase)) return Forbid();
            return View(purchase);
        }

        [Authorize]
        [HttpPost("delete")]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(Models.Purchase model)
        {
            var purchase = _purchaseRepository.GetById(model.Id);
            if (purchase is null) return NotFound();
            if (!CanManagePurchase(purchase)) return Forbid();

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
