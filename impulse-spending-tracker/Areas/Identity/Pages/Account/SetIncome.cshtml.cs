using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using impulse_spending_tracker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace impulse_spending_tracker.Areas.Identity.Pages.Account
{
    public class SetIncomeModel : PageModel
    {
        private readonly Data.ImpulseSpendingDbContext _db;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public SetIncomeModel(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, Data.ImpulseSpendingDbContext db)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _db = db;
            Input = new InputModel();
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string? UserId { get; set; }
        public string ReturnUrl { get; set; } = "/";

        public class InputModel
        {
            [Required]
            [Range(0.01, 10000000)]
            [DataType(DataType.Currency)]
            [Display(Name = "Monthly net income")]
            public decimal MonthlyNetIncome { get; set; }
        }

        public IActionResult OnGet(string userId, string? returnUrl = null)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            UserId = userId;
            ReturnUrl = string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl!;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string userId, string? returnUrl = null)
        {
            UserId = userId;
            ReturnUrl = string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl!;

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var profile = await _db.UserProfiles.FirstOrDefaultAsync(p => p.AppUserId == userId);
            if (profile == null)
            {
                ModelState.AddModelError(string.Empty, "User profile not found.");
                return Page();
            }

            profile.MonthlyNetIncome = Input.MonthlyNetIncome;
            _db.UserProfiles.Update(profile);
            await _db.SaveChangesAsync();

            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
            }

            return LocalRedirect(ReturnUrl);
        }
    }
}
