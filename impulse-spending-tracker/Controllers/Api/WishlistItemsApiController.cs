using impulse_spending_tracker.Api;
using impulse_spending_tracker.Data;
using impulse_spending_tracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace impulse_spending_tracker.Controllers.Api
{
    [Route("api/wishlistitems")]
    public sealed class WishlistItemsApiController : ApiControllerBase
    {
        public WishlistItemsApiController(ImpulseSpendingDbContext dbContext)
            : base(dbContext)
        {
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<WishlistItemDto>>> GetAll(
            string? query = null,
            int? userProfileId = null,
            bool? isPurchased = null)
        {
            IQueryable<WishlistItem> items = DbContext.WishlistItems
                .AsNoTracking()
                .Include(item => item.UserProfile)
                .Include(item => item.ConvertedPurchase);

            if (!string.IsNullOrWhiteSpace(query))
            {
                var search = query.Trim();
                items = items.Where(item =>
                    EF.Functions.Like(item.Name, $"%{search}%") ||
                    EF.Functions.Like(item.Reason ?? string.Empty, $"%{search}%") ||
                    EF.Functions.Like(item.UserProfile!.FirstName, $"%{search}%") ||
                    EF.Functions.Like(item.UserProfile!.LastName, $"%{search}%") ||
                    EF.Functions.Like(item.UserProfile!.Email, $"%{search}%"));
            }

            if (userProfileId.HasValue)
            {
                items = items.Where(item => item.UserProfileId == userProfileId.Value);
            }

            if (isPurchased.HasValue)
            {
                items = items.Where(item => item.IsPurchased == isPurchased.Value);
            }

            var result = (await items
                    .OrderByDescending(item => item.AddedAt)
                    .ToListAsync())
                .Select(item => item.ToDto())
                .ToList();

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<WishlistItemDto>> GetById(int id)
        {
            var item = await DbContext.WishlistItems
                .AsNoTracking()
                .Include(value => value.UserProfile)
                .Include(value => value.ConvertedPurchase)
                .SingleOrDefaultAsync(value => value.Id == id);

            if (item is null)
            {
                return NotFound();
            }

            return Ok(item.ToDto());
        }

        [HttpPost]
        public async Task<ActionResult<WishlistItemDto>> Create([FromBody] WishlistItemUpsertDto model)
        {
            var userExists = await DbContext.UserProfiles.AnyAsync(user => user.Id == model.UserProfileId);
            if (!userExists)
            {
                return RelatedEntityNotFound(nameof(model.UserProfileId), "Selected user profile does not exist.");
            }

            var item = new WishlistItem
            {
                UserProfileId = model.UserProfileId,
                Name = model.Name,
                DesiredPrice = model.DesiredPrice,
                CurrentPrice = model.CurrentPrice,
                Priority = model.Priority,
                AddedAt = model.AddedAt ?? DateTime.UtcNow,
                TargetPurchaseDate = model.TargetPurchaseDate,
                Reason = model.Reason,
                IsPurchased = model.IsPurchased,
                LinkUrl = model.LinkUrl
            };

            var validationResult = ValidateEntity(item);
            if (validationResult is not OkResult)
            {
                return validationResult;
            }

            DbContext.WishlistItems.Add(item);
            await DbContext.SaveChangesAsync();

            item = await DbContext.WishlistItems
                .AsNoTracking()
                .Include(value => value.UserProfile)
                .Include(value => value.ConvertedPurchase)
                .SingleAsync(value => value.Id == item.Id);

            return CreatedAtAction(nameof(GetById), new { id = item.Id }, item.ToDto());
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<WishlistItemDto>> Update(int id, [FromBody] WishlistItemUpsertDto model)
        {
            var item = await DbContext.WishlistItems.SingleOrDefaultAsync(value => value.Id == id);
            if (item is null)
            {
                return NotFound();
            }

            var userExists = await DbContext.UserProfiles.AnyAsync(user => user.Id == model.UserProfileId);
            if (!userExists)
            {
                return RelatedEntityNotFound(nameof(model.UserProfileId), "Selected user profile does not exist.");
            }

            item.UserProfileId = model.UserProfileId;
            item.Name = model.Name;
            item.DesiredPrice = model.DesiredPrice;
            item.CurrentPrice = model.CurrentPrice;
            item.Priority = model.Priority;
            item.AddedAt = model.AddedAt ?? item.AddedAt;
            item.TargetPurchaseDate = model.TargetPurchaseDate;
            item.Reason = model.Reason;
            item.IsPurchased = model.IsPurchased;
            item.LinkUrl = model.LinkUrl;

            var validationResult = ValidateEntity(item);
            if (validationResult is not OkResult)
            {
                return validationResult;
            }

            await DbContext.SaveChangesAsync();

            item = await DbContext.WishlistItems
                .AsNoTracking()
                .Include(value => value.UserProfile)
                .Include(value => value.ConvertedPurchase)
                .SingleAsync(value => value.Id == item.Id);

            return Ok(item.ToDto());
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await DbContext.WishlistItems.SingleOrDefaultAsync(value => value.Id == id);
            if (item is null)
            {
                return NotFound();
            }

            item.IsDeleted = true;
            await DbContext.SaveChangesAsync();

            return NoContent();
        }
    }
}