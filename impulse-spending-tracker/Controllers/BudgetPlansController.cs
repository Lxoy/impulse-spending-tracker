using impulse_spending_tracker.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace impulse_spending_tracker.Controllers
{
    public class BudgetPlansController : Controller
    {
        private readonly BudgetPlanMockRepository _budgetPlanRepository;

        public BudgetPlansController(BudgetPlanMockRepository budgetPlanRepository)
        {
            _budgetPlanRepository = budgetPlanRepository;
        }

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

        public IActionResult Details(Guid id)
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