using impulse_spending_tracker.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace impulse_spending_tracker.Controllers
{
    [Route("budget-plans")]
    public class BudgetPlansController : Controller
    {
        private readonly BudgetPlanRepository _budgetPlanRepository;

        public BudgetPlansController(BudgetPlanRepository budgetPlanRepository)
        {
            _budgetPlanRepository = budgetPlanRepository;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            var plans = _budgetPlanRepository
                .GetAll()
                .OrderByDescending(p => p.IsActive)
                .ThenByDescending(p => p.ValidFrom)
                .ThenBy(p => p.Name)
                .ToList();

            return View(plans);
        }

        [HttpGet("{id:int}")]
        public IActionResult Details(int id)
        {
            var plan = _budgetPlanRepository.GetById(id);
            if (plan is null)
            {
                return NotFound();
            }

            return View(plan);
        }
    }
}