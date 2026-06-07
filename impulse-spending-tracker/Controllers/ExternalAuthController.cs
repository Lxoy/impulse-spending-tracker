using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using impulse_spending_tracker.Data;
using impulse_spending_tracker.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace impulse_spending_tracker.Controllers
{
    [AllowAnonymous]
    [Route("auth")]
    public class ExternalAuthController : Controller
    {
        private readonly SignInManager<Models.AppUser> _signInManager;
        private readonly UserManager<Models.AppUser> _userManager;
        private readonly ImpulseSpendingDbContext _db;

        public ExternalAuthController(
            SignInManager<Models.AppUser> signInManager,
            UserManager<Models.AppUser> userManager,
            ImpulseSpendingDbContext db)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _db = db;
        }

        // Start Google challenge. Visit /auth/google to test.
        [HttpGet("google")]
        public IActionResult Google(string returnUrl = "/")
        {
            var props = new AuthenticationProperties { RedirectUri = Url.Action(nameof(GoogleCallback), new { returnUrl }) };
            return Challenge(props, "Google");
        }

        // Callback URL that Google redirects to after consent.
        [HttpGet("google-callback")]
        public async Task<IActionResult> GoogleCallback(string returnUrl = "/")
        {
            var auth = await HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);
            var principal = auth.Principal;

            if (principal == null)
            {
                return LocalRedirect("/Identity/Account/Login");
            }

            var email = principal.FindFirstValue(ClaimTypes.Email)
                ?? principal.FindFirstValue("email");

            var externalId = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? principal.FindFirstValue("sub");

            if (string.IsNullOrWhiteSpace(externalId))
            {
                await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
                return LocalRedirect("/Identity/Account/Login");
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                email = $"google-{externalId}@local.invalid";
            }

            const string loginProvider = "Google";
            var user = await _userManager.FindByLoginAsync(loginProvider, externalId);
            user ??= await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new Models.AppUser
                {
                    UserName = email,
                    Email = email,
                    OIB = BuildDigits(externalId, 11),
                    JMBG = BuildDigits(externalId + ":jmbg", 13)
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
                    return LocalRedirect("/Identity/Account/Login");
                }
            }

            var userLogins = await _userManager.GetLoginsAsync(user);
            if (!userLogins.Any(login =>
                    string.Equals(login.LoginProvider, loginProvider, StringComparison.OrdinalIgnoreCase) &&
                    login.ProviderKey == externalId))
            {
                var addLoginResult = await _userManager.AddLoginAsync(
                    user,
                    new UserLoginInfo(loginProvider, externalId, loginProvider));

                if (!addLoginResult.Succeeded)
                {
                    await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
                    return LocalRedirect("/Identity/Account/Login");
                }
            }

            var firstName = principal.FindFirstValue(ClaimTypes.GivenName)
                ?? principal.FindFirstValue(ClaimTypes.Name)
                ?? email.Split('@')[0];

            var lastName = principal.FindFirstValue(ClaimTypes.Surname) ?? string.Empty;

            var profile = await _db.UserProfiles
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.AppUserId == user.Id);

            if (profile == null)
            {
                profile = await _db.UserProfiles
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(p => p.Email == email);
            }

            if (profile == null)
            {
                profile = new UserProfile
                {
                    AppUserId = user.Id,
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    CreatedAt = DateTime.UtcNow
                };

                _db.UserProfiles.Add(profile);
                await _db.SaveChangesAsync();
            }
            else
            {
                profile.IsDeleted = false;
                profile.AppUserId = user.Id;

                if (string.IsNullOrWhiteSpace(profile.FirstName))
                {
                    profile.FirstName = firstName;
                }

                if (string.IsNullOrWhiteSpace(profile.LastName))
                {
                    profile.LastName = lastName;
                }

                profile.Email = email;
                _db.UserProfiles.Update(profile);
                await _db.SaveChangesAsync();
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            return LocalRedirect(returnUrl);
        }

        private static string BuildDigits(string seed, int length)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(seed));
            var value = BitConverter.ToUInt64(bytes, 0);

            ulong max = 1;
            for (var i = 0; i < length; i++)
            {
                max *= 10;
            }

            return (value % max).ToString().PadLeft(length, '0');
        }
    }
}

