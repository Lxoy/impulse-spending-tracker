using impulse_spending_tracker.Api;
using impulse_spending_tracker.Data;
using impulse_spending_tracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace impulse_spending_tracker.Controllers.Api
{
    [Route("api/budgetplans")]
    public sealed class BudgetPlansApiController : ApiControllerBase
    {
        public BudgetPlansApiController(ImpulseSpendingDbContext dbContext)
            : base(dbContext)
        {
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BudgetPlanDto>>> GetAll(
            string? query = null,
            int? userProfileId = null,
            bool? isActive = null)
        {
            IQueryable<BudgetPlan> plans = DbContext.BudgetPlans
                .AsNoTracking()
                .Include(plan => plan.UserProfile);

            if (!string.IsNullOrWhiteSpace(query))
            {
                var search = query.Trim();
                plans = plans.Where(plan =>
                    EF.Functions.Like(plan.Name, $"%{search}%") ||
                    EF.Functions.Like(plan.UserProfile!.FirstName, $"%{search}%") ||
                    EF.Functions.Like(plan.UserProfile!.LastName, $"%{search}%") ||
                    EF.Functions.Like(plan.UserProfile!.Email, $"%{search}%"));
            }

            if (userProfileId.HasValue)
            {
                plans = plans.Where(plan => plan.UserProfileId == userProfileId.Value);
            }

            if (isActive.HasValue)
            {
                plans = plans.Where(plan => plan.IsActive == isActive.Value);
            }

            var result = (await plans
                    .OrderBy(plan => plan.Name)
                    .ToListAsync())
                .Select(plan => plan.ToDto())
                .ToList();

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<BudgetPlanDto>> GetById(int id)
        {
            var plan = await DbContext.BudgetPlans
                .AsNoTracking()
                .Include(item => item.UserProfile)
                .SingleOrDefaultAsync(item => item.Id == id);

            if (plan is null)
            {
                return NotFound();
            }

            return Ok(plan.ToDto());
        }

        [HttpPost]
        public async Task<ActionResult<BudgetPlanDto>> Create([FromBody] BudgetPlanUpsertDto model)
        {
            var userExists = await DbContext.UserProfiles.AnyAsync(user => user.Id == model.UserProfileId);
            if (!userExists)
            {
                return RelatedEntityNotFound(nameof(model.UserProfileId), "Selected user profile does not exist.");
            }

            var plan = new BudgetPlan
            {
                UserProfileId = model.UserProfileId,
                Name = model.Name,
                ValidFrom = model.ValidFrom,
                ValidTo = model.ValidTo,
                MonthlyLimit = model.MonthlyLimit,
                ImpulseCapPercentage = model.ImpulseCapPercentage,
                EssentialCategoryLimit = model.EssentialCategoryLimit,
                DiscretionaryCategoryLimit = model.DiscretionaryCategoryLimit,
                IsActive = model.IsActive
            };

            var validationResult = ValidateEntity(plan);
            if (validationResult is not OkResult)
            {
                return validationResult;
            }

            DbContext.BudgetPlans.Add(plan);
            await DbContext.SaveChangesAsync();

            plan = await DbContext.BudgetPlans
                .AsNoTracking()
                .Include(item => item.UserProfile)
                .SingleAsync(item => item.Id == plan.Id);

            return CreatedAtAction(nameof(GetById), new { id = plan.Id }, plan.ToDto());
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<BudgetPlanDto>> Update(int id, [FromBody] BudgetPlanUpsertDto model)
        {
            var plan = await DbContext.BudgetPlans.SingleOrDefaultAsync(item => item.Id == id);
            if (plan is null)
            {
                return NotFound();
            }

            var userExists = await DbContext.UserProfiles.AnyAsync(user => user.Id == model.UserProfileId);
            if (!userExists)
            {
                return RelatedEntityNotFound(nameof(model.UserProfileId), "Selected user profile does not exist.");
            }

            plan.UserProfileId = model.UserProfileId;
            plan.Name = model.Name;
            plan.ValidFrom = model.ValidFrom;
            plan.ValidTo = model.ValidTo;
            plan.MonthlyLimit = model.MonthlyLimit;
            plan.ImpulseCapPercentage = model.ImpulseCapPercentage;
            plan.EssentialCategoryLimit = model.EssentialCategoryLimit;
            plan.DiscretionaryCategoryLimit = model.DiscretionaryCategoryLimit;
            plan.IsActive = model.IsActive;

            var validationResult = ValidateEntity(plan);
            if (validationResult is not OkResult)
            {
                return validationResult;
            }

            await DbContext.SaveChangesAsync();

            plan = await DbContext.BudgetPlans
                .AsNoTracking()
                .Include(item => item.UserProfile)
                .SingleAsync(item => item.Id == plan.Id);

            return Ok(plan.ToDto());
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var plan = await DbContext.BudgetPlans.SingleOrDefaultAsync(item => item.Id == id);
            if (plan is null)
            {
                return NotFound();
            }

            plan.IsDeleted = true;
            await DbContext.SaveChangesAsync();

            return NoContent();
        }
    }
}