using impulse_spending_tracker.Api;
using impulse_spending_tracker.Data;
using impulse_spending_tracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace impulse_spending_tracker.Controllers.Api
{
    [Route("api/userprofiles")]
    public sealed class UserProfilesApiController : ApiControllerBase
    {
        public UserProfilesApiController(ImpulseSpendingDbContext dbContext)
            : base(dbContext)
        {
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserProfileDto>>> GetAll(
            string? query = null,
            string? email = null,
            decimal? minIncome = null,
            decimal? maxIncome = null)
        {
            var users = DbContext.UserProfiles.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(query))
            {
                var search = query.Trim();
                users = users.Where(user =>
                    EF.Functions.Like(user.FirstName, $"%{search}%") ||
                    EF.Functions.Like(user.LastName, $"%{search}%") ||
                    EF.Functions.Like(user.Email, $"%{search}%"));
            }

            if (!string.IsNullOrWhiteSpace(email))
            {
                var emailFilter = email.Trim();
                users = users.Where(user => EF.Functions.Like(user.Email, $"%{emailFilter}%"));
            }

            if (minIncome.HasValue)
            {
                users = users.Where(user => user.MonthlyNetIncome >= minIncome.Value);
            }

            if (maxIncome.HasValue)
            {
                users = users.Where(user => user.MonthlyNetIncome <= maxIncome.Value);
            }

            var result = (await users
                    .OrderBy(user => user.FirstName)
                    .ThenBy(user => user.LastName)
                    .ToListAsync())
                .Select(user => user.ToDto())
                .ToList();

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<UserProfileDto>> GetById(int id)
        {
            var user = await DbContext.UserProfiles.AsNoTracking().SingleOrDefaultAsync(item => item.Id == id);
            if (user is null)
            {
                return NotFound();
            }

            return Ok(user.ToDto());
        }

        [HttpPost]
        public async Task<ActionResult<UserProfileDto>> Create([FromBody] UserProfileUpsertDto model)
        {
            var user = new UserProfile
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                DateOfBirth = model.DateOfBirth,
                MonthlyNetIncome = model.MonthlyNetIncome,
                RiskToleranceScore = model.RiskToleranceScore,
                CreatedAt = DateTime.UtcNow
            };

            var validationResult = ValidateEntity(user);
            if (validationResult is not OkResult)
            {
                return validationResult;
            }

            DbContext.UserProfiles.Add(user);
            await DbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user.ToDto());
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<UserProfileDto>> Update(int id, [FromBody] UserProfileUpsertDto model)
        {
            var user = await DbContext.UserProfiles.SingleOrDefaultAsync(item => item.Id == id);
            if (user is null)
            {
                return NotFound();
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.DateOfBirth = model.DateOfBirth;
            user.MonthlyNetIncome = model.MonthlyNetIncome;
            user.RiskToleranceScore = model.RiskToleranceScore;

            var validationResult = ValidateEntity(user);
            if (validationResult is not OkResult)
            {
                return validationResult;
            }

            await DbContext.SaveChangesAsync();
            return Ok(user.ToDto());
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await DbContext.UserProfiles.SingleOrDefaultAsync(item => item.Id == id);
            if (user is null)
            {
                return NotFound();
            }

            user.IsDeleted = true;
            await DbContext.SaveChangesAsync();

            return NoContent();
        }
    }
}