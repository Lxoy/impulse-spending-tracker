using impulse_spending_tracker.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace impulse_spending_tracker.Controllers
{
    [Route("budget-plans")]
    public class BudgetPlansController : Controller
    {
        private readonly BudgetPlanRepository _budgetPlanRepository;
        private readonly UserProfileRepository _userProfileRepository;

        public BudgetPlansController(
            BudgetPlanRepository budgetPlanRepository,
            UserProfileRepository userProfileRepository)
        {
            _budgetPlanRepository = budgetPlanRepository;
            _userProfileRepository = userProfileRepository;
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

        [HttpGet("filter")]
        public IActionResult Filter(string query)
        {
            var plans = _budgetPlanRepository
                .GetAll()
                .Where(b => string.IsNullOrEmpty(query) || 
                            b.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(p => p.IsActive)
                .ThenByDescending(p => p.ValidFrom)
                .ThenBy(p => p.Name)
                .ToList();

            return PartialView("_BudgetPlanTableRows", plans);
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

        private void LoadDropdownData()
        {
            var users = _userProfileRepository.GetAll()
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = $"{u.FirstName} {u.LastName} (ID: {u.Id})"
                })
                .ToList();

            ViewBag.UserProfileId = users;
        }

        private void PopulateSelectedUser(Models.BudgetPlan plan)
        {
            if (plan.UserProfileId <= 0)
            {
                return;
            }

            plan.UserProfile = _userProfileRepository.GetById(plan.UserProfileId);
        }

        private bool ValidateDateRange(Models.BudgetPlan plan)
        {
            if (plan.ValidFrom.HasValue && plan.ValidTo.HasValue && plan.ValidFrom.Value.Date > plan.ValidTo.Value.Date)
            {
                ModelState.AddModelError(nameof(Models.BudgetPlan.ValidTo), "Valid From ne smije biti veći od Valid To.");
                return false;
            }

            return true;
        }

        [HttpGet("create")]
        public IActionResult Create()
        {
            LoadDropdownData();
            return View(new Models.BudgetPlan());
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Models.BudgetPlan plan)
        {
            if (!ValidateDateRange(plan) || !ModelState.IsValid)
            {
                LoadDropdownData();
                PopulateSelectedUser(plan);
                return View(plan);
            }

            _budgetPlanRepository.Create(plan);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("edit")]
        public IActionResult Edit(int id)
        {
            var plan = _budgetPlanRepository.GetById(id);
            if (plan is null)
            {
                return NotFound();
            }

            LoadDropdownData();
            return View(plan);
        }

        [HttpPost("edit")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Models.BudgetPlan plan)
        {
            if (!ValidateDateRange(plan) || !ModelState.IsValid)
            {
                LoadDropdownData();
                PopulateSelectedUser(plan);
                return View(plan);
            }

            _budgetPlanRepository.Update(plan);
            return RedirectToAction(nameof(Details), new { id = plan.Id });
        }

        [HttpGet("delete")]
        public IActionResult Delete(int id)
        {
            var plan = _budgetPlanRepository.GetById(id);
            if (plan is null)
            {
                return NotFound();
            }

            return View(plan);
        }

        [HttpPost("delete")]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(Models.BudgetPlan model)
        {
            var plan = _budgetPlanRepository.GetById(model.Id);
            if (plan is null)
            {
                return NotFound();
            }

            try
            {
                _budgetPlanRepository.Delete(plan);
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "Unable to delete this budget plan because related data exists.");
                return View(plan);
            }
        }
    }
}