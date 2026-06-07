using impulse_spending_tracker.Api;
using impulse_spending_tracker.Data;
using impulse_spending_tracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace impulse_spending_tracker.Controllers.Api
{
    [Route("api/purchases")]
    public sealed class PurchasesApiController : ApiControllerBase
    {
        public PurchasesApiController(ImpulseSpendingDbContext dbContext)
            : base(dbContext)
        {
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PurchaseDto>>> GetAll(
            string? query = null,
            int? userProfileId = null,
            int? merchantId = null,
            ImpulseTriggerType? triggerType = null)
        {
            IQueryable<Purchase> purchases = DbContext.Purchases
                .AsNoTracking()
                .Include(purchase => purchase.UserProfile)
                .Include(purchase => purchase.Merchant)
                .Include(purchase => purchase.SpendingSession)
                .Include(purchase => purchase.BudgetPlan)
                .Include(purchase => purchase.WishlistItem)
                .Include(purchase => purchase.TriggerTypes);

            if (!string.IsNullOrWhiteSpace(query))
            {
                var search = query.Trim();
                purchases = purchases.Where(purchase =>
                    EF.Functions.Like(purchase.Title, $"%{search}%") ||
                    EF.Functions.Like(purchase.Currency, $"%{search}%") ||
                    EF.Functions.Like(purchase.MoodBeforePurchase, $"%{search}%") ||
                    EF.Functions.Like(purchase.Notes ?? string.Empty, $"%{search}%") ||
                    EF.Functions.Like(purchase.UserProfile!.FirstName, $"%{search}%") ||
                    EF.Functions.Like(purchase.UserProfile!.LastName, $"%{search}%") ||
                    EF.Functions.Like(purchase.Merchant!.Name, $"%{search}%") ||
                    EF.Functions.Like(purchase.Merchant!.Category, $"%{search}%"));
            }

            if (userProfileId.HasValue)
            {
                purchases = purchases.Where(purchase => purchase.UserProfileId == userProfileId.Value);
            }

            if (merchantId.HasValue)
            {
                purchases = purchases.Where(purchase => purchase.MerchantId == merchantId.Value);
            }

            if (triggerType.HasValue)
            {
                purchases = purchases.Where(purchase => purchase.TriggerType == triggerType.Value);
            }

            var result = (await purchases
                    .OrderByDescending(purchase => purchase.PurchasedAt)
                    .ToListAsync())
                .Select(purchase => purchase.ToDto())
                .ToList();

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<PurchaseDto>> GetById(int id)
        {
            var purchase = await DbContext.Purchases
                .AsNoTracking()
                .Include(item => item.UserProfile)
                .Include(item => item.Merchant)
                .Include(item => item.SpendingSession)
                .Include(item => item.BudgetPlan)
                .Include(item => item.WishlistItem)
                .Include(item => item.TriggerTypes)
                .SingleOrDefaultAsync(item => item.Id == id);

            if (purchase is null)
            {
                return NotFound();
            }

            return Ok(purchase.ToDto());
        }

        [HttpPost]
        public async Task<ActionResult<PurchaseDto>> Create([FromBody] PurchaseUpsertDto model)
        {
            var relatedValidation = await ValidatePurchaseRelations(model);
            if (relatedValidation is not null)
            {
                return relatedValidation;
            }

            var purchase = new Purchase
            {
                UserProfileId = model.UserProfileId,
                MerchantId = model.MerchantId,
                SpendingSessionId = model.SpendingSessionId,
                BudgetPlanId = model.BudgetPlanId,
                WishlistItemId = model.WishlistItemId,
                Title = model.Title,
                Amount = model.Amount,
                Currency = model.Currency,
                PurchasedAt = model.PurchasedAt,
                MoodBeforePurchase = model.MoodBeforePurchase,
                NeedLevel = model.NeedLevel,
                TriggerType = model.TriggerType,
                Installments = model.Installments,
                Notes = model.Notes
            };

            var selectedTriggerTypes = await LoadTriggerTypesAsync(model.TriggerTypeIds);
            if (selectedTriggerTypes is null)
            {
                return new ActionResult<PurchaseDto>(ValidationProblem(ModelState));
            }

            purchase.TriggerTypes = selectedTriggerTypes;

            var validationResult = ValidateEntity(purchase);
            if (validationResult is not OkResult)
            {
                return validationResult;
            }

            DbContext.Purchases.Add(purchase);
            await DbContext.SaveChangesAsync();

            purchase = await LoadPurchaseForResponseAsync(purchase.Id);
            return CreatedAtAction(nameof(GetById), new { id = purchase.Id }, purchase.ToDto());
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<PurchaseDto>> Update(int id, [FromBody] PurchaseUpsertDto model)
        {
            var purchase = await DbContext.Purchases
                .Include(item => item.TriggerTypes)
                .SingleOrDefaultAsync(item => item.Id == id);

            if (purchase is null)
            {
                return NotFound();
            }

            var relatedValidation = await ValidatePurchaseRelations(model, id);
            if (relatedValidation is not null)
            {
                return relatedValidation;
            }

            purchase.UserProfileId = model.UserProfileId;
            purchase.MerchantId = model.MerchantId;
            purchase.SpendingSessionId = model.SpendingSessionId;
            purchase.BudgetPlanId = model.BudgetPlanId;
            purchase.WishlistItemId = model.WishlistItemId;
            purchase.Title = model.Title;
            purchase.Amount = model.Amount;
            purchase.Currency = model.Currency;
            purchase.PurchasedAt = model.PurchasedAt;
            purchase.MoodBeforePurchase = model.MoodBeforePurchase;
            purchase.NeedLevel = model.NeedLevel;
            purchase.TriggerType = model.TriggerType;
            purchase.Installments = model.Installments;
            purchase.Notes = model.Notes;

            var selectedTriggerTypes = await LoadTriggerTypesAsync(model.TriggerTypeIds);
            if (selectedTriggerTypes is null)
            {
                return new ActionResult<PurchaseDto>(ValidationProblem(ModelState));
            }

            purchase.TriggerTypes = selectedTriggerTypes;

            var validationResult = ValidateEntity(purchase);
            if (validationResult is not OkResult)
            {
                return validationResult;
            }

            await DbContext.SaveChangesAsync();

            purchase = await LoadPurchaseForResponseAsync(purchase.Id);
            return Ok(purchase.ToDto());
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var purchase = await DbContext.Purchases.SingleOrDefaultAsync(item => item.Id == id);
            if (purchase is null)
            {
                return NotFound();
            }

            purchase.IsDeleted = true;
            await DbContext.SaveChangesAsync();

            return NoContent();
        }

        private async Task<ActionResult?> ValidatePurchaseRelations(PurchaseUpsertDto model, int? currentPurchaseId = null)
        {
            var userExists = await DbContext.UserProfiles.AnyAsync(user => user.Id == model.UserProfileId);
            if (!userExists)
            {
                return RelatedEntityNotFound(nameof(model.UserProfileId), "Selected user profile does not exist.");
            }

            var merchantExists = await DbContext.Merchants.AnyAsync(merchant => merchant.Id == model.MerchantId);
            if (!merchantExists)
            {
                return RelatedEntityNotFound(nameof(model.MerchantId), "Selected merchant does not exist.");
            }

            if (model.SpendingSessionId.HasValue)
            {
                var sessionExists = await DbContext.SpendingSessions.AnyAsync(session => session.Id == model.SpendingSessionId.Value);
                if (!sessionExists)
                {
                    return RelatedEntityNotFound(nameof(model.SpendingSessionId), "Selected spending session does not exist.");
                }
            }

            if (model.BudgetPlanId.HasValue)
            {
                var budgetPlanExists = await DbContext.BudgetPlans.AnyAsync(plan => plan.Id == model.BudgetPlanId.Value);
                if (!budgetPlanExists)
                {
                    return RelatedEntityNotFound(nameof(model.BudgetPlanId), "Selected budget plan does not exist.");
                }
            }

            if (model.WishlistItemId.HasValue)
            {
                var wishlistItemExists = await DbContext.WishlistItems.AnyAsync(item => item.Id == model.WishlistItemId.Value);
                if (!wishlistItemExists)
                {
                    return RelatedEntityNotFound(nameof(model.WishlistItemId), "Selected wishlist item does not exist.");
                }

                var wishlistItemAlreadyConverted = await DbContext.Purchases
                    .IgnoreQueryFilters()
                    .AnyAsync(purchase =>
                        purchase.WishlistItemId == model.WishlistItemId.Value &&
                        (!currentPurchaseId.HasValue || purchase.Id != currentPurchaseId.Value));

                if (wishlistItemAlreadyConverted)
                {
                    return RelatedEntityNotFound(nameof(model.WishlistItemId), "Selected wishlist item is already converted to a purchase.");
                }
            }

            if (model.BudgetPlanId.HasValue)
            {
                var budgetPlan = await DbContext.BudgetPlans
                    .AsNoTracking()
                    .SingleOrDefaultAsync(plan => plan.Id == model.BudgetPlanId.Value);

                if (budgetPlan is null || budgetPlan.UserProfileId != model.UserProfileId)
                {
                    return RelatedEntityNotFound(nameof(model.BudgetPlanId), "Selected budget plan does not exist.");
                }

                var spentOnPlan = await DbContext.Purchases
                    .IgnoreQueryFilters()
                    .Where(purchase => purchase.BudgetPlanId == model.BudgetPlanId.Value && (!currentPurchaseId.HasValue || purchase.Id != currentPurchaseId.Value))
                    .Select(purchase => (decimal?)purchase.Amount)
                    .SumAsync() ?? 0m;

                if (spentOnPlan + model.Amount > budgetPlan.MonthlyLimit)
                {
                    ModelState.AddModelError(nameof(model.BudgetPlanId), $"Budget plan limit would be exceeded. Current total is {spentOnPlan:F2} EUR and this purchase would push it above {budgetPlan.MonthlyLimit:F2} EUR.");
                    return ValidationProblem(ModelState);
                }
            }

            return null;
        }

        private async Task<List<TriggerType>?> LoadTriggerTypesAsync(IEnumerable<int> triggerTypeIds)
        {
            var distinctTriggerTypeIds = triggerTypeIds.Distinct().ToList();
            if (distinctTriggerTypeIds.Count == 0)
            {
                return new List<TriggerType>();
            }

            var tags = await DbContext.TriggerTypes
                .Where(tag => distinctTriggerTypeIds.Contains(tag.Id))
                .ToListAsync();

            if (tags.Count != distinctTriggerTypeIds.Count)
            {
                var missingIds = distinctTriggerTypeIds.Except(tags.Select(tag => tag.Id)).ToList();
                ModelState.AddModelError(nameof(PurchaseUpsertDto.TriggerTypeIds), $"Unknown trigger type ids: {string.Join(", ", missingIds)}.");
                return null;
            }

            return tags;
        }

        private async Task<Purchase> LoadPurchaseForResponseAsync(int id)
        {
            return await DbContext.Purchases
                .AsNoTracking()
                .Include(item => item.UserProfile)
                .Include(item => item.Merchant)
                .Include(item => item.SpendingSession)
                .Include(item => item.BudgetPlan)
                .Include(item => item.WishlistItem)
                .Include(item => item.TriggerTypes)
                .SingleAsync(item => item.Id == id);
        }
    }
}