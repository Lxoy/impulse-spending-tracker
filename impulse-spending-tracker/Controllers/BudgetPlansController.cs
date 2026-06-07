using impulse_spending_tracker.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace impulse_spending_tracker.Controllers
{
    [Authorize]
    [Route("budget-plans")]
    public class BudgetPlansController : Controller
    {
        private readonly BudgetPlanRepository _budgetPlanRepository;
        private readonly UserProfileRepository _userProfileRepository;
        private readonly Microsoft.AspNetCore.Identity.UserManager<impulse_spending_tracker.Models.AppUser> _userManager;

        public BudgetPlansController(
            BudgetPlanRepository budgetPlanRepository,
            UserProfileRepository userProfileRepository,
            Microsoft.AspNetCore.Identity.UserManager<impulse_spending_tracker.Models.AppUser> userManager)
        {
            _budgetPlanRepository = budgetPlanRepository;
            _userProfileRepository = userProfileRepository;
            _userManager = userManager;
        }

        [Authorize]
        [HttpGet("")]
        public IActionResult Index()
        {
            if (User.IsInRole("Admin"))
            {
                var plans = _budgetPlanRepository
                    .GetAll()
                    .OrderByDescending(p => p.IsActive)
                    .ThenByDescending(p => p.ValidFrom)
                    .ThenBy(p => p.Name)
                    .ToList();

                return View(plans);
            }

            var profileId = GetCurrentUserProfileId();
            if (!profileId.HasValue) return Forbid();

            var plansForUser = _budgetPlanRepository
                .GetAll()
                .Where(b => b.UserProfileId == profileId.Value)
                .OrderByDescending(p => p.IsActive)
                .ThenByDescending(p => p.ValidFrom)
                .ThenBy(p => p.Name)
                .ToList();

            return View(plansForUser);
        }

        [Authorize]
        [HttpGet("filter")]
        public IActionResult Filter(string query)
        {
            if (User.IsInRole("Admin"))
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

            var profileId = GetCurrentUserProfileId();
            if (!profileId.HasValue) return Forbid();

            var filtered = _budgetPlanRepository
                .GetAll()
                .Where(b => b.UserProfileId == profileId.Value &&
                            (string.IsNullOrEmpty(query) || b.Name.Contains(query, StringComparison.OrdinalIgnoreCase)))
                .OrderByDescending(p => p.IsActive)
                .ThenByDescending(p => p.ValidFrom)
                .ThenBy(p => p.Name)
                .ToList();

            return PartialView("_BudgetPlanTableRows", filtered);
        }

        [Authorize]
        [HttpGet("{id:int}")]
        public IActionResult Details(int id)
        {
            var plan = _budgetPlanRepository.GetById(id);
            if (plan is null) return NotFound();
            if (!CanManagePlan(plan)) return Forbid();
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

        private int? GetCurrentUserProfileId()
        {
            var currentUserId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(currentUserId)) return null;
            var profile = _userProfileRepository.GetAll().FirstOrDefault(p => p.AppUserId == currentUserId);
            return profile?.Id;
        }

        private bool CanManagePlan(Models.BudgetPlan plan)
        {
            if (User.IsInRole("Admin")) return true;
            var profileId = GetCurrentUserProfileId();
            return profileId.HasValue && plan.UserProfileId == profileId.Value;
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

        [Authorize]
        [HttpGet("create")]
        public IActionResult Create()
        {
            if (User.IsInRole("Admin"))
            {
                LoadDropdownData();
                ViewBag.ShowUserSelector = true;
                return View(new Models.BudgetPlan());
            }

            var profileId = GetCurrentUserProfileId();
            var plan = new Models.BudgetPlan { UserProfileId = profileId ?? 0 };
            ViewBag.ShowUserSelector = false;
            if (profileId.HasValue)
            {
                ViewBag.CurrentUserProfile = _userProfileRepository.GetById(profileId.Value);
            }
            return View(plan);
        }

        [Authorize]
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Models.BudgetPlan plan)
        {
            // Ensure non-admins can only create for themselves
            if (!User.IsInRole("Admin"))
            {
                var profileId = GetCurrentUserProfileId();
                plan.UserProfileId = profileId ?? 0;
            }

            if (!ValidateDateRange(plan) || !ModelState.IsValid)
            {
                if (User.IsInRole("Admin")) LoadDropdownData();
                if (!User.IsInRole("Admin"))
                {
                    var pid = GetCurrentUserProfileId();
                    if (pid.HasValue) ViewBag.CurrentUserProfile = _userProfileRepository.GetById(pid.Value);
                }
                PopulateSelectedUser(plan);
                return View(plan);
            }

            _budgetPlanRepository.Create(plan);
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        [HttpGet("edit")]
        public IActionResult Edit(int id)
        {
            var plan = _budgetPlanRepository.GetById(id);
            if (plan is null)
            {
                return NotFound();
            }

            if (!CanManagePlan(plan)) return Forbid();
            if (User.IsInRole("Admin")) LoadDropdownData();
            else
            {
                ViewBag.ShowUserSelector = false;
                ViewBag.CurrentUserProfile = _userProfileRepository.GetById(plan.UserProfileId);
            }
            return View(plan);
        }

        [Authorize]
        [HttpPost("edit")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Models.BudgetPlan plan)
        {
            var existing = _budgetPlanRepository.GetById(plan.Id);
            if (existing is null) return NotFound();
            if (!CanManagePlan(existing)) return Forbid();

            // Prevent non-admins from changing ownership
            if (!User.IsInRole("Admin")) plan.UserProfileId = existing.UserProfileId;

            if (!ValidateDateRange(plan) || !ModelState.IsValid)
            {
                if (User.IsInRole("Admin")) LoadDropdownData();
                if (!User.IsInRole("Admin")) ViewBag.CurrentUserProfile = _userProfileRepository.GetById(plan.UserProfileId);
                PopulateSelectedUser(plan);
                return View(plan);
            }

            _budgetPlanRepository.Update(plan);
            return RedirectToAction(nameof(Details), new { id = plan.Id });
        }

        [Authorize]
        [HttpGet("delete")]
        public IActionResult Delete(int id)
        {
            var plan = _budgetPlanRepository.GetById(id);
            if (plan is null) return NotFound();
            if (!CanManagePlan(plan)) return Forbid();
            return View(plan);
        }

        [Authorize]
        [HttpPost("delete")]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(Models.BudgetPlan model)
        {
            var plan = _budgetPlanRepository.GetById(model.Id);
            if (plan is null) return NotFound();
            if (!CanManagePlan(plan)) return Forbid();

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