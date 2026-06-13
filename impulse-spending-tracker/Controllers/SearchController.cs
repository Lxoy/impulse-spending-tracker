using impulse_spending_tracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace impulse_spending_tracker.Controllers
{
    [Route("search")]
    public class SearchController : Controller
    {
        private readonly IActionDescriptorCollectionProvider _actionDescriptors;
        private readonly IAuthorizationPolicyProvider _authorizationPolicyProvider;
        private readonly IAuthorizationService _authorizationService;

        public SearchController(
            IActionDescriptorCollectionProvider actionDescriptors,
            IAuthorizationPolicyProvider authorizationPolicyProvider,
            IAuthorizationService authorizationService)
        {
            _actionDescriptors = actionDescriptors;
            _authorizationPolicyProvider = authorizationPolicyProvider;
            _authorizationService = authorizationService;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(string? query)
        {
            var normalizedQuery = query?.Trim() ?? string.Empty;

            var routedResults = RouteCatalog
                .Select(WithResolvedUrl)
                .Where(item => !string.IsNullOrWhiteSpace(item.Url))
                .ToList();

            var results = string.IsNullOrWhiteSpace(normalizedQuery)
                ? routedResults
                : routedResults
                    .Where(item => Matches(item, normalizedQuery))
                    .ToList();

            var allowedResults = new List<GlobalSearchResult>();
            foreach (var result in results)
            {
                if (await CanAccessAsync(result))
                {
                    allowedResults.Add(result);
                }
            }

            var viewModel = new GlobalSearchViewModel
            {
                Query = normalizedQuery,
                Results = allowedResults
            };

            return View(viewModel);
        }

        private GlobalSearchResult WithResolvedUrl(GlobalSearchResult result)
        {
            if (FindActionDescriptor(result) is null)
            {
                return new GlobalSearchResult();
            }

            return new GlobalSearchResult
            {
                Title = result.Title,
                Description = result.Description,
                Url = Url.Action(result.Action, result.Controller) ?? string.Empty,
                Category = result.Category,
                IsMenuItem = result.IsMenuItem,
                Controller = result.Controller,
                Action = result.Action
            };
        }

        private async Task<bool> CanAccessAsync(GlobalSearchResult result)
        {
            var actionDescriptor = FindActionDescriptor(result);

            if (actionDescriptor is null)
            {
                return false;
            }

            var authorizeData = actionDescriptor.ControllerTypeInfo
                .GetCustomAttributes(inherit: true)
                .Concat(actionDescriptor.MethodInfo.GetCustomAttributes(inherit: true))
                .OfType<IAuthorizeData>()
                .ToList();

            if (!authorizeData.Any())
            {
                return true;
            }

            var policy = await AuthorizationPolicy.CombineAsync(_authorizationPolicyProvider, authorizeData);
            if (policy is null)
            {
                return true;
            }

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, resource: null, policy);
            return authorizationResult.Succeeded;
        }

        private ControllerActionDescriptor? FindActionDescriptor(GlobalSearchResult result)
        {
            return _actionDescriptors.ActionDescriptors.Items
                .OfType<ControllerActionDescriptor>()
                .FirstOrDefault(action =>
                    string.Equals(action.ControllerName, result.Controller, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(action.ActionName, result.Action, StringComparison.OrdinalIgnoreCase));
        }

        private static bool Matches(GlobalSearchResult item, string query)
        {
            return item.Title.Contains(query, StringComparison.OrdinalIgnoreCase)
                   || item.Description.Contains(query, StringComparison.OrdinalIgnoreCase)
                   || item.Category.Contains(query, StringComparison.OrdinalIgnoreCase)
                   || item.Url.Contains(query, StringComparison.OrdinalIgnoreCase);
        }

        private static readonly List<GlobalSearchResult> RouteCatalog = new()
        {
            new() { Title = "Home", Description = "Dashboard overview with impulse spending metrics and recent insights.", Category = "Page", IsMenuItem = true, Controller = "Home", Action = "Index" },
            new() { Title = "User Profiles", Description = "Manage user income, risk tolerance, and profile information.", Category = "Menu item", IsMenuItem = true, Controller = "UserProfiles", Action = "Index" },
            new() { Title = "Budget Plans", Description = "Review and maintain monthly limits, impulse caps, and category budgets.", Category = "Menu item", IsMenuItem = true, Controller = "BudgetPlans", Action = "Index" },
            new() { Title = "Merchants", Description = "Browse stores and merchant categories used by purchases.", Category = "Menu item", IsMenuItem = true, Controller = "Merchants", Action = "Index" },
            new() { Title = "Purchases", Description = "Track purchase history, amounts, moods, triggers, notes, and attachments.", Category = "Menu item", IsMenuItem = true, Controller = "Purchases", Action = "Index" },
            new() { Title = "Sessions", Description = "Analyze shopping sessions by platform, channel, budget, and completion.", Category = "Menu item", IsMenuItem = true, Controller = "SpendingSessions", Action = "Index" },
            new() { Title = "Trigger Types", Description = "Maintain impulse trigger labels, colors, and descriptions.", Category = "Menu item", IsMenuItem = true, Controller = "TriggerTypes", Action = "Index" },
            new() { Title = "Wishlist", Description = "Compare desired items, current prices, priorities, and conversion status.", Category = "Menu item", IsMenuItem = true, Controller = "WishlistItems", Action = "Index" },
            new() { Title = "Create User Profile", Description = "Add a new user profile.", Category = "Route", IsMenuItem = false, Controller = "UserProfiles", Action = "Create" },
            new() { Title = "Create Budget Plan", Description = "Add a new budget plan.", Category = "Route", IsMenuItem = false, Controller = "BudgetPlans", Action = "Create" },
            new() { Title = "Create Merchant", Description = "Add a new merchant.", Category = "Route", IsMenuItem = false, Controller = "Merchants", Action = "Create" },
            new() { Title = "Create Purchase", Description = "Record a new purchase.", Category = "Route", IsMenuItem = false, Controller = "Purchases", Action = "Create" },
            new() { Title = "Create Session", Description = "Add a new spending session.", Category = "Route", IsMenuItem = false, Controller = "SpendingSessions", Action = "Create" },
            new() { Title = "Create Trigger Type", Description = "Add a new impulse trigger type.", Category = "Route", IsMenuItem = false, Controller = "TriggerTypes", Action = "Create" },
            new() { Title = "Create Wishlist Item", Description = "Add a new wishlist item.", Category = "Route", IsMenuItem = false, Controller = "WishlistItems", Action = "Create" }
        };
    }
}
