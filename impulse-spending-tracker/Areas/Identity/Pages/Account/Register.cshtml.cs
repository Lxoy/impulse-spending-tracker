using System.ComponentModel.DataAnnotations;
using impulse_spending_tracker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace impulse_spending_tracker.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly Data.ImpulseSpendingDbContext _db;

        public RegisterModel(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            Data.ImpulseSpendingDbContext db)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _db = db;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ReturnUrl { get; set; }

        public class InputModel
        {
            [Required]
            [StringLength(60, MinimumLength = 2)]
            public string FirstName { get; set; } = string.Empty;

            [Required]
            [StringLength(60, MinimumLength = 2)]
            public string LastName { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.Date)]
            public DateTime DateOfBirth { get; set; }

            [Required]
            [EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required]
            [StringLength(11, MinimumLength = 11)]
            [RegularExpression("^[0-9]*$")]
            public string OIB { get; set; } = string.Empty;

            [Required]
            [StringLength(13, MinimumLength = 13)]
            [RegularExpression("^[0-9]*$")]
            public string JMBG { get; set; } = string.Empty;

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            [DataType(DataType.Password)]
            [Compare(nameof(Password), ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        public void OnGet(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = new AppUser
            {
                UserName = Input.Email,
                Email = Input.Email,
                OIB = Input.OIB,
                JMBG = Input.JMBG
            };

            var result = await _userManager.CreateAsync(user, Input.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return Page();
            }

            // Link or create UserProfile
            var existing = await _db.UserProfiles.FirstOrDefaultAsync(p => p.Email == user.Email);
            if (existing == null)
            {
                var profile = new UserProfile
                {
                    AppUserId = user.Id,
                    FirstName = Input.FirstName,
                    LastName = Input.LastName,
                    DateOfBirth = Input.DateOfBirth,
                    Email = user.Email ?? string.Empty,
                    CreatedAt = DateTime.UtcNow
                };

                _db.UserProfiles.Add(profile);
                await _db.SaveChangesAsync();
            }
            else
            {
                // If profile exists but is not linked, attach it to the new user.
                if (string.IsNullOrEmpty(existing.AppUserId))
                {
                    existing.AppUserId = user.Id;
                    existing.FirstName = Input.FirstName;
                    existing.LastName = Input.LastName;
                        existing.DateOfBirth = Input.DateOfBirth;
                    existing.Email = user.Email ?? existing.Email;
                    _db.UserProfiles.Update(existing);
                    await _db.SaveChangesAsync();
                }
                // If existing.AppUserId is not null and not equal to this user, do not overwrite.
            }

            const string defaultRoleName = "User";
            if (!await _roleManager.RoleExistsAsync(defaultRoleName))
            {
                await _roleManager.CreateAsync(new IdentityRole(defaultRoleName));
            }

            await _userManager.AddToRoleAsync(user, defaultRoleName);

            // After registration, redirect user to set their MonthlyNetIncome before signing them in.
            return RedirectToPage("/Account/SetIncome", new { area = "Identity", userId = user.Id, returnUrl });
        }
    }
}
