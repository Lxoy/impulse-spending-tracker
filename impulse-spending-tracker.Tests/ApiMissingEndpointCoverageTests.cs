using System.Net;
using System.Net.Http.Json;
using impulse_spending_tracker.Api;
using impulse_spending_tracker.Models;
using Xunit;

namespace impulse_spending_tracker.Tests;

public sealed class MissingApiEndpointErrorCoverageTests
{
    public static TheoryData<string, object> PutMissingEndpointCases => new()
    {
        { "/api/userprofiles/999999", new UserProfileUpsertDto { FirstName = "Ana", LastName = "Ivic", Email = "ana.ivic@example.com", DateOfBirth = new DateTime(1998, 1, 1), MonthlyNetIncome = 1500m, RiskToleranceScore = 5 } },
        { "/api/merchants/999999", new MerchantUpsertDto { Name = "Market Lab", Category = "Groceries", CountryCode = "HR", IsOnlineOnly = true, AverageDeliveryDays = 3 } },
        { "/api/trigger-types/999999", new TriggerTypeUpsertDto { Name = "Stress Buy", ColorHex = "#D9534F", Description = "Stress trigger" } },
        { "/api/budgetplans/999999", new BudgetPlanUpsertDto { UserProfileId = 1, Name = "Monthly Plan", ValidFrom = new DateTime(2026, 1, 1), ValidTo = new DateTime(2026, 1, 31), MonthlyLimit = 500m, ImpulseCapPercentage = 20, EssentialCategoryLimit = 200m, DiscretionaryCategoryLimit = 100m, IsActive = true } },
        { "/api/spendingsessions/999999", new SpendingSessionUpsertDto { UserProfileId = 1, StartedAt = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc), EndedAt = new DateTime(2026, 1, 1, 10, 30, 0, DateTimeKind.Utc), Platform = "Web", Channel = "Chrome", SessionBudget = 100m, SpentAmount = 0m, ItemsViewed = 3, ItemsAddedToCart = 1, CheckoutCompleted = false } },
        { "/api/wishlistitems/999999", new WishlistItemUpsertDto { UserProfileId = 1, Name = "Headphones", DesiredPrice = 100m, CurrentPrice = 120m, Priority = 3, AddedAt = DateTime.UtcNow, TargetPurchaseDate = DateTime.UtcNow.AddDays(7), Reason = "Work calls", IsPurchased = false, LinkUrl = "https://example.com/headphones" } },
        { "/api/purchases/999999", new PurchaseUpsertDto { UserProfileId = 1, MerchantId = 1, Title = "Keyboard", Amount = 45m, Currency = "EUR", PurchasedAt = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc), MoodBeforePurchase = "Focused", NeedLevel = 5, TriggerType = ImpulseTriggerType.Other, Installments = 1, TriggerTypeIds = new List<int>() } }
    };

    public static TheoryData<string> DeleteMissingEndpointCases => new()
    {
        "/api/userprofiles/999999",
        "/api/merchants/999999",
        "/api/trigger-types/999999",
        "/api/budgetplans/999999",
        "/api/spendingsessions/999999",
        "/api/wishlistitems/999999",
        "/api/purchases/999999"
    };

    [Theory]
    [MemberData(nameof(PutMissingEndpointCases))]
    public async Task Put_ReturnsNotFound_WhenMissing(string url, object payload)
    {
        using var factory = new ApiTestFactory();
        var client = factory.CreateClient();

        var response = await client.PutAsJsonAsync(url, payload);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(DeleteMissingEndpointCases))]
    public async Task Delete_ReturnsNotFound_WhenMissing(string url)
    {
        using var factory = new ApiTestFactory();
        var client = factory.CreateClient();

        var response = await client.DeleteAsync(url);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
