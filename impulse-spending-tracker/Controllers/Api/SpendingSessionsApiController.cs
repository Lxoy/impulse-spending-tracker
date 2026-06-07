using impulse_spending_tracker.Api;
using impulse_spending_tracker.Data;
using impulse_spending_tracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace impulse_spending_tracker.Controllers.Api
{
    [Route("api/spendingsessions")]
    public sealed class SpendingSessionsApiController : ApiControllerBase
    {
        public SpendingSessionsApiController(ImpulseSpendingDbContext dbContext)
            : base(dbContext)
        {
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SpendingSessionDto>>> GetAll(
            string? query = null,
            int? userProfileId = null,
            bool? checkoutCompleted = null)
        {
            IQueryable<SpendingSession> sessions = DbContext.SpendingSessions
                .AsNoTracking()
                .Include(session => session.UserProfile)
                .Include(session => session.Purchases);

            if (!string.IsNullOrWhiteSpace(query))
            {
                var search = query.Trim();
                sessions = sessions.Where(session =>
                    EF.Functions.Like(session.Platform, $"%{search}%") ||
                    EF.Functions.Like(session.Channel, $"%{search}%") ||
                    EF.Functions.Like(session.UserProfile!.FirstName, $"%{search}%") ||
                    EF.Functions.Like(session.UserProfile!.LastName, $"%{search}%") ||
                    EF.Functions.Like(session.UserProfile!.Email, $"%{search}%"));
            }

            if (userProfileId.HasValue)
            {
                sessions = sessions.Where(session => session.UserProfileId == userProfileId.Value);
            }

            if (checkoutCompleted.HasValue)
            {
                sessions = sessions.Where(session => session.CheckoutCompleted == checkoutCompleted.Value);
            }

            var loadedSessions = await sessions
                .OrderByDescending(session => session.StartedAt)
                .ToListAsync();

            foreach (var session in loadedSessions)
            {
                session.SpentAmount = session.Purchases.Sum(purchase => purchase.Amount);
            }

            var result = loadedSessions
                .Select(session => session.ToDto())
                .ToList();

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<SpendingSessionDto>> GetById(int id)
        {
            var session = await DbContext.SpendingSessions
                .AsNoTracking()
                .Include(item => item.UserProfile)
                .Include(item => item.Purchases)
                .SingleOrDefaultAsync(item => item.Id == id);

            if (session is null)
            {
                return NotFound();
            }

            session.SpentAmount = session.Purchases.Sum(purchase => purchase.Amount);

            return Ok(session.ToDto());
        }

        [HttpPost]
        public async Task<ActionResult<SpendingSessionDto>> Create([FromBody] SpendingSessionUpsertDto model)
        {
            var userExists = await DbContext.UserProfiles.AnyAsync(user => user.Id == model.UserProfileId);
            if (!userExists)
            {
                return RelatedEntityNotFound(nameof(model.UserProfileId), "Selected user profile does not exist.");
            }

            var session = new SpendingSession
            {
                UserProfileId = model.UserProfileId,
                StartedAt = model.StartedAt,
                EndedAt = model.EndedAt,
                Platform = model.Platform,
                Channel = model.Channel,
                SessionBudget = model.SessionBudget,
                SpentAmount = model.SpentAmount,
                ItemsViewed = model.ItemsViewed,
                ItemsAddedToCart = model.ItemsAddedToCart,
                CheckoutCompleted = model.CheckoutCompleted
            };

            var validationResult = ValidateEntity(session);
            if (validationResult is not OkResult)
            {
                return validationResult;
            }

            DbContext.SpendingSessions.Add(session);
            await DbContext.SaveChangesAsync();

            session = await DbContext.SpendingSessions
                .AsNoTracking()
                .Include(item => item.UserProfile)
                .SingleAsync(item => item.Id == session.Id);

            return CreatedAtAction(nameof(GetById), new { id = session.Id }, session.ToDto());
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<SpendingSessionDto>> Update(int id, [FromBody] SpendingSessionUpsertDto model)
        {
            var session = await DbContext.SpendingSessions.SingleOrDefaultAsync(item => item.Id == id);
            if (session is null)
            {
                return NotFound();
            }

            var userExists = await DbContext.UserProfiles.AnyAsync(user => user.Id == model.UserProfileId);
            if (!userExists)
            {
                return RelatedEntityNotFound(nameof(model.UserProfileId), "Selected user profile does not exist.");
            }

            session.UserProfileId = model.UserProfileId;
            session.StartedAt = model.StartedAt;
            session.EndedAt = model.EndedAt;
            session.Platform = model.Platform;
            session.Channel = model.Channel;
            session.SessionBudget = model.SessionBudget;
            session.SpentAmount = model.SpentAmount;
            session.ItemsViewed = model.ItemsViewed;
            session.ItemsAddedToCart = model.ItemsAddedToCart;
            session.CheckoutCompleted = model.CheckoutCompleted;

            var validationResult = ValidateEntity(session);
            if (validationResult is not OkResult)
            {
                return validationResult;
            }

            await DbContext.SaveChangesAsync();

            session = await DbContext.SpendingSessions
                .AsNoTracking()
                .Include(item => item.UserProfile)
                .SingleAsync(item => item.Id == session.Id);

            return Ok(session.ToDto());
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var session = await DbContext.SpendingSessions.SingleOrDefaultAsync(item => item.Id == id);
            if (session is null)
            {
                return NotFound();
            }

            session.IsDeleted = true;
            await DbContext.SaveChangesAsync();

            return NoContent();
        }
    }
}