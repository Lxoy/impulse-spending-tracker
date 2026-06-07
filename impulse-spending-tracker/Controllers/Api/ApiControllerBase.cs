using System.ComponentModel.DataAnnotations;
using impulse_spending_tracker.Data;
using Microsoft.AspNetCore.Mvc;

namespace impulse_spending_tracker.Controllers.Api
{
    [ApiController]
    public abstract class ApiControllerBase : ControllerBase
    {
        protected readonly ImpulseSpendingDbContext DbContext;

        protected ApiControllerBase(ImpulseSpendingDbContext dbContext)
        {
            DbContext = dbContext;
        }

        protected ActionResult ValidateEntity(object entity)
        {
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(entity);

            if (Validator.TryValidateObject(entity, validationContext, validationResults, true))
            {
                return Ok();
            }

            foreach (var validationResult in validationResults)
            {
                var memberNames = validationResult.MemberNames.Any()
                    ? validationResult.MemberNames
                    : new[] { string.Empty };

                foreach (var memberName in memberNames)
                {
                    ModelState.AddModelError(memberName, validationResult.ErrorMessage ?? "Validation failed.");
                }
            }

            return ValidationProblem(ModelState);
        }

        protected ActionResult RelatedEntityNotFound(string fieldName, string message)
        {
            ModelState.AddModelError(fieldName, message);
            return ValidationProblem(ModelState);
        }
    }
}