using impulse_spending_tracker.Api;
using impulse_spending_tracker.Data;
using impulse_spending_tracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace impulse_spending_tracker.Controllers.Api
{
    [Route("api/trigger-types")]
    public sealed class TriggerTypesApiController : ApiControllerBase
    {
        public TriggerTypesApiController(ImpulseSpendingDbContext dbContext)
            : base(dbContext)
        {
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TriggerTypeDto>>> GetAll(string? query = null)
        {
            var tags = DbContext.TriggerTypes.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(query))
            {
                var search = query.Trim();
                tags = tags.Where(tag =>
                    EF.Functions.Like(tag.Name, $"%{search}%") ||
                    EF.Functions.Like(tag.Description, $"%{search}%"));
            }

            var result = (await tags
                    .OrderBy(tag => tag.Name)
                    .ToListAsync())
                .Select(tag => tag.ToDto())
                .ToList();

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TriggerTypeDto>> GetById(int id)
        {
            var tag = await DbContext.TriggerTypes.AsNoTracking().SingleOrDefaultAsync(item => item.Id == id);
            if (tag is null)
            {
                return NotFound();
            }

            return Ok(tag.ToDto());
        }

        [HttpPost]
        public async Task<ActionResult<TriggerTypeDto>> Create([FromBody] TriggerTypeUpsertDto model)
        {
            var tag = new TriggerType
            {
                Name = model.Name,
                ColorHex = model.ColorHex,
                Description = model.Description
            };

            var validationResult = ValidateEntity(tag);
            if (validationResult is not OkResult)
            {
                return validationResult;
            }

            DbContext.TriggerTypes.Add(tag);
            await DbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = tag.Id }, tag.ToDto());
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<TriggerTypeDto>> Update(int id, [FromBody] TriggerTypeUpsertDto model)
        {
            var tag = await DbContext.TriggerTypes.SingleOrDefaultAsync(item => item.Id == id);
            if (tag is null)
            {
                return NotFound();
            }

            tag.Name = model.Name;
            tag.ColorHex = model.ColorHex;
            tag.Description = model.Description;

            var validationResult = ValidateEntity(tag);
            if (validationResult is not OkResult)
            {
                return validationResult;
            }

            await DbContext.SaveChangesAsync();
            return Ok(tag.ToDto());
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var tag = await DbContext.TriggerTypes.SingleOrDefaultAsync(item => item.Id == id);
            if (tag is null)
            {
                return NotFound();
            }

            tag.IsDeleted = true;
            await DbContext.SaveChangesAsync();

            return NoContent();
        }
    }
}