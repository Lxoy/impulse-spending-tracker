using System.Threading.Tasks;
using impulse_spending_tracker.Data;
using impulse_spending_tracker.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace impulse_spending_tracker.Middleware
{
    public class RequireIncomeMiddleware
    {
        private readonly RequestDelegate _next;

        public RequireIncomeMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, UserManager<AppUser> userManager, ImpulseSpendingDbContext db)
        {
            var user = context.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                var path = context.Request.Path.Value ?? string.Empty;

                // Allow identity pages (including SetIncome) and static assets
                if (!path.StartsWith("/Identity", System.StringComparison.OrdinalIgnoreCase)
                    && !path.StartsWith("/css", System.StringComparison.OrdinalIgnoreCase)
                    && !path.StartsWith("/js", System.StringComparison.OrdinalIgnoreCase)
                    && !path.StartsWith("/lib", System.StringComparison.OrdinalIgnoreCase)
                    && !path.StartsWith("/img", System.StringComparison.OrdinalIgnoreCase)
                    && !path.StartsWith("/_content", System.StringComparison.OrdinalIgnoreCase)
                    && !path.StartsWith("/_framework", System.StringComparison.OrdinalIgnoreCase)
                    )
                {
                    var userId = userManager.GetUserId(user);
                    if (!string.IsNullOrEmpty(userId))
                    {
                        var profile = await db.UserProfiles.FirstOrDefaultAsync(p => p.AppUserId == userId);
                        if (profile == null || profile.MonthlyNetIncome <= 0m)
                        {
                            // redirect to SetIncome page
                            var returnUrl = context.Request.Path + context.Request.QueryString;
                            var url = $"/Identity/Account/SetIncome?userId={userId}&returnUrl={System.Net.WebUtility.UrlEncode(returnUrl)}";
                            context.Response.Redirect(url);
                            return;
                        }
                    }
                }
            }

            await _next(context);
        }
    }
}
