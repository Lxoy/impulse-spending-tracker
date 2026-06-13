using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using impulse_spending_tracker.Api;
using impulse_spending_tracker.Data;
using impulse_spending_tracker.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace impulse_spending_tracker.Tests;

public sealed class ApiRoleAccessTests
{
    private const string TestAuthenticationScheme = "TestRole";
    private const string TestUserId = "api-role-test-user";

    public static TheoryData<string, HttpMethod, string, object?> ApiEndpointCases => new()
    {
        { "User", HttpMethod.Get, "/api/userprofiles", null },
        { "User", HttpMethod.Get, "/api/userprofiles/999999", null },
        { "User", HttpMethod.Post, "/api/userprofiles", ValidUserProfile() },
        { "User", HttpMethod.Put, "/api/userprofiles/999999", ValidUserProfile() },
        { "User", HttpMethod.Delete, "/api/userprofiles/999999", null },

        { "Manager", HttpMethod.Get, "/api/merchants", null },
        { "Manager", HttpMethod.Get, "/api/merchants/999999", null },
        { "Manager", HttpMethod.Post, "/api/merchants", ValidMerchant() },
        { "Manager", HttpMethod.Put, "/api/merchants/999999", ValidMerchant() },
        { "Manager", HttpMethod.Delete, "/api/merchants/999999", null },

        { "Admin", HttpMethod.Get, "/api/trigger-types", null },
        { "Admin", HttpMethod.Get, "/api/trigger-types/999999", null },
        { "Admin", HttpMethod.Post, "/api/trigger-types", ValidTriggerType() },
        { "Admin", HttpMethod.Put, "/api/trigger-types/999999", ValidTriggerType() },
        { "Admin", HttpMethod.Delete, "/api/trigger-types/999999", null },

        { "User", HttpMethod.Get, "/api/budgetplans", null },
        { "User", HttpMethod.Get, "/api/budgetplans/999999", null },
        { "User", HttpMethod.Post, "/api/budgetplans", ValidBudgetPlan() },
        { "User", HttpMethod.Put, "/api/budgetplans/999999", ValidBudgetPlan() },
        { "User", HttpMethod.Delete, "/api/budgetplans/999999", null },

        { "User", HttpMethod.Get, "/api/spendingsessions", null },
        { "User", HttpMethod.Get, "/api/spendingsessions/999999", null },
        { "User", HttpMethod.Post, "/api/spendingsessions", ValidSpendingSession() },
        { "User", HttpMethod.Put, "/api/spendingsessions/999999", ValidSpendingSession() },
        { "User", HttpMethod.Delete, "/api/spendingsessions/999999", null },

        { "User", HttpMethod.Get, "/api/wishlistitems", null },
        { "User", HttpMethod.Get, "/api/wishlistitems/999999", null },
        { "User", HttpMethod.Post, "/api/wishlistitems", ValidWishlistItem() },
        { "User", HttpMethod.Put, "/api/wishlistitems/999999", ValidWishlistItem() },
        { "User", HttpMethod.Delete, "/api/wishlistitems/999999", null },

        { "User", HttpMethod.Get, "/api/purchases", null },
        { "User", HttpMethod.Get, "/api/purchases/999999", null },
        { "User", HttpMethod.Post, "/api/purchases", ValidPurchase() },
        { "User", HttpMethod.Put, "/api/purchases/999999", ValidPurchase() },
        { "User", HttpMethod.Delete, "/api/purchases/999999", null }
    };

    [Theory]
    [MemberData(nameof(ApiEndpointCases))]
    public async Task ApiEndpoint_AllowsConfiguredRole_ToReachAction(string role, HttpMethod method, string url, object? payload)
    {
        using var factory = CreateFactory();
        await SeedAuthenticatedProfileAsync(factory);
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestRoleAuthenticationHandler.RoleHeaderName, role);

        using var request = new HttpRequestMessage(method, url);
        if (payload is not null)
        {
            request.Content = JsonContent.Create(payload);
        }

        var response = await client.SendAsync(request);

        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private static WebApplicationFactory<Program> CreateFactory()
    {
        return new ApiTestFactory().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services
                    .AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = TestAuthenticationScheme;
                        options.DefaultChallengeScheme = TestAuthenticationScheme;
                    })
                    .AddScheme<AuthenticationSchemeOptions, TestRoleAuthenticationHandler>(TestAuthenticationScheme, _ => { });
            });
        });
    }

    private static async Task SeedAuthenticatedProfileAsync(WebApplicationFactory<Program> factory)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ImpulseSpendingDbContext>();

        dbContext.UserProfiles.Add(new UserProfile
        {
            AppUserId = TestUserId,
            FirstName = "Role",
            LastName = "Tester",
            Email = "role.tester@example.com",
            DateOfBirth = new DateTime(1998, 1, 1),
            MonthlyNetIncome = 1500m,
            RiskToleranceScore = 5,
            CreatedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync();
    }

    private static UserProfileUpsertDto ValidUserProfile() => new()
    {
        FirstName = "Ana",
        LastName = "Ivic",
        Email = "ana.ivic@example.com",
        DateOfBirth = new DateTime(1998, 1, 1),
        MonthlyNetIncome = 1500m,
        RiskToleranceScore = 5
    };

    private static MerchantUpsertDto ValidMerchant() => new()
    {
        Name = "Market Lab",
        Category = "Groceries",
        CountryCode = "HR",
        IsOnlineOnly = true,
        AverageDeliveryDays = 3
    };

    private static TriggerTypeUpsertDto ValidTriggerType() => new()
    {
        Name = "Stress Buy",
        ColorHex = "#D9534F",
        Description = "Stress trigger"
    };

    private static BudgetPlanUpsertDto ValidBudgetPlan() => new()
    {
        UserProfileId = 1,
        Name = "Monthly Plan",
        ValidFrom = new DateTime(2026, 1, 1),
        ValidTo = new DateTime(2026, 1, 31),
        MonthlyLimit = 500m,
        ImpulseCapPercentage = 20,
        EssentialCategoryLimit = 200m,
        DiscretionaryCategoryLimit = 100m,
        IsActive = true
    };

    private static SpendingSessionUpsertDto ValidSpendingSession() => new()
    {
        UserProfileId = 1,
        StartedAt = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc),
        EndedAt = new DateTime(2026, 1, 1, 10, 30, 0, DateTimeKind.Utc),
        Platform = "Web",
        Channel = "Chrome",
        SessionBudget = 100m,
        SpentAmount = 0m,
        ItemsViewed = 3,
        ItemsAddedToCart = 1,
        CheckoutCompleted = false
    };

    private static WishlistItemUpsertDto ValidWishlistItem() => new()
    {
        UserProfileId = 1,
        Name = "Headphones",
        DesiredPrice = 100m,
        CurrentPrice = 120m,
        Priority = 3,
        AddedAt = DateTime.UtcNow,
        TargetPurchaseDate = DateTime.UtcNow.AddDays(7),
        Reason = "Work calls",
        IsPurchased = false,
        LinkUrl = "https://example.com/headphones"
    };

    private static PurchaseUpsertDto ValidPurchase() => new()
    {
        UserProfileId = 1,
        MerchantId = 1,
        Title = "Keyboard",
        Amount = 45m,
        Currency = "EUR",
        PurchasedAt = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc),
        MoodBeforePurchase = "Focused",
        NeedLevel = 5,
        TriggerType = ImpulseTriggerType.Other,
        Installments = 1,
        TriggerTypeIds = new List<int>()
    };

    private sealed class TestRoleAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string RoleHeaderName = "X-Test-Role";

        public TestRoleAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var role = Request.Headers[RoleHeaderName].FirstOrDefault() ?? "User";
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, TestUserId),
                new Claim(ClaimTypes.Name, "role.tester@example.com"),
                new Claim(ClaimTypes.Role, role)
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
