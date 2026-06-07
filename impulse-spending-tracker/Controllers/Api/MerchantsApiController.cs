using impulse_spending_tracker.Api;
using impulse_spending_tracker.Data;
using impulse_spending_tracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace impulse_spending_tracker.Controllers.Api
{
    [Route("api/merchants")]
    public sealed class MerchantsApiController : ApiControllerBase
    {
        public MerchantsApiController(ImpulseSpendingDbContext dbContext)
            : base(dbContext)
        {
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MerchantDto>>> GetAll(
            string? query = null,
            string? category = null,
            string? countryCode = null,
            bool? isOnlineOnly = null)
        {
            var merchants = DbContext.Merchants.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(query))
            {
                var search = query.Trim();
                merchants = merchants.Where(merchant =>
                    EF.Functions.Like(merchant.Name, $"%{search}%") ||
                    EF.Functions.Like(merchant.Category, $"%{search}%") ||
                    EF.Functions.Like(merchant.CountryCode, $"%{search}%"));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                var categoryFilter = category.Trim();
                merchants = merchants.Where(merchant => EF.Functions.Like(merchant.Category, $"%{categoryFilter}%"));
            }

            if (!string.IsNullOrWhiteSpace(countryCode))
            {
                var countryFilter = countryCode.Trim();
                merchants = merchants.Where(merchant => EF.Functions.Like(merchant.CountryCode, $"%{countryFilter}%"));
            }

            if (isOnlineOnly.HasValue)
            {
                merchants = merchants.Where(merchant => merchant.IsOnlineOnly == isOnlineOnly.Value);
            }

            var result = (await merchants
                    .OrderBy(merchant => merchant.Name)
                    .ThenBy(merchant => merchant.Category)
                    .ToListAsync())
                .Select(merchant => merchant.ToDto())
                .ToList();

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<MerchantDto>> GetById(int id)
        {
            var merchant = await DbContext.Merchants
                .AsNoTracking()
                .SingleOrDefaultAsync(item => item.Id == id);

            if (merchant is null)
            {
                return NotFound();
            }

            return Ok(merchant.ToDto());
        }

        [HttpPost]
        public async Task<ActionResult<MerchantDto>> Create([FromBody] MerchantUpsertDto model)
        {
            var merchant = new Merchant
            {
                Name = model.Name,
                Category = model.Category,
                CountryCode = model.CountryCode,
                IsOnlineOnly = model.IsOnlineOnly,
                AverageDeliveryDays = model.AverageDeliveryDays
            };

            var validationResult = ValidateEntity(merchant);
            if (validationResult is not OkResult)
            {
                return validationResult;
            }

            DbContext.Merchants.Add(merchant);
            await DbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = merchant.Id }, merchant.ToDto());
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<MerchantDto>> Update(int id, [FromBody] MerchantUpsertDto model)
        {
            var merchant = await DbContext.Merchants.SingleOrDefaultAsync(item => item.Id == id);
            if (merchant is null)
            {
                return NotFound();
            }

            merchant.Name = model.Name;
            merchant.Category = model.Category;
            merchant.CountryCode = model.CountryCode;
            merchant.IsOnlineOnly = model.IsOnlineOnly;
            merchant.AverageDeliveryDays = model.AverageDeliveryDays;

            var validationResult = ValidateEntity(merchant);
            if (validationResult is not OkResult)
            {
                return validationResult;
            }

            await DbContext.SaveChangesAsync();
            return Ok(merchant.ToDto());
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var merchant = await DbContext.Merchants.SingleOrDefaultAsync(item => item.Id == id);
            if (merchant is null)
            {
                return NotFound();
            }

            merchant.IsDeleted = true;
            await DbContext.SaveChangesAsync();

            return NoContent();
        }
    }
}