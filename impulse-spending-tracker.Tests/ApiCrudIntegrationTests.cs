using System.Net;
using System.Net.Http.Json;
using impulse_spending_tracker.Api;
using impulse_spending_tracker.Data;
using impulse_spending_tracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace impulse_spending_tracker.Tests;

public abstract class ApiCrudIntegrationTestsBase
{
    protected static async Task SeedAsync(ApiTestFactory factory, Action<ImpulseSpendingDbContext> seed)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ImpulseSpendingDbContext>();
        seed(dbContext);
        await dbContext.SaveChangesAsync();
    }

    protected static async Task<T?> ReadJsonAsync<T>(HttpResponseMessage response)
    {
        return await response.Content.ReadFromJsonAsync<T>();
    }

    protected static async Task AssertValidationProblemAsync(HttpResponseMessage response, params string[] expectedKeys)
    {
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);

        foreach (var expectedKey in expectedKeys)
        {
            Assert.Contains(expectedKey, problem!.Errors.Keys);
        }
    }
}

public sealed class UserProfilesApiControllerTests : ApiCrudIntegrationTestsBase
{
    [Fact]
    public async Task CrudFlow_Works()
    {
        using var factory = new ApiTestFactory();
        await SeedAsync(factory, dbContext =>
        {
            dbContext.UserProfiles.Add(new UserProfile
            {
                FirstName = "Ana",
                LastName = "Horvat",
                Email = "ana.horvat@example.com",
                DateOfBirth = new DateTime(1998, 3, 14),
                MonthlyNetIncome = 1500m,
                RiskToleranceScore = 6,
                CreatedAt = DateTime.UtcNow
            });
        });

        var client = factory.CreateClient();

        var allResponse = await client.GetAsync("/api/userprofiles");
        Assert.Equal(HttpStatusCode.OK, allResponse.StatusCode);

        var allItems = await ReadJsonAsync<List<UserProfileDto>>(allResponse);
        Assert.NotNull(allItems);
        Assert.Single(allItems!);

        var existingId = allItems![0].Id;
        var getByIdResponse = await client.GetAsync($"/api/userprofiles/{existingId}");
        Assert.Equal(HttpStatusCode.OK, getByIdResponse.StatusCode);

        var getByIdItem = await ReadJsonAsync<UserProfileDto>(getByIdResponse);
        Assert.NotNull(getByIdItem);
        Assert.Equal("Ana", getByIdItem!.FirstName);

        var createResponse = await client.PostAsJsonAsync("/api/userprofiles", new UserProfileUpsertDto
        {
            FirstName = "Iva",
            LastName = "Peric",
            Email = "iva.peric@example.com",
            DateOfBirth = new DateTime(1997, 10, 2),
            MonthlyNetIncome = 1800m,
            RiskToleranceScore = 7
        });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await ReadJsonAsync<UserProfileDto>(createResponse);
        Assert.NotNull(created);
        Assert.Equal("Iva", created!.FirstName);

        var updateResponse = await client.PutAsJsonAsync($"/api/userprofiles/{created.Id}", new UserProfileUpsertDto
        {
            FirstName = "Ivana",
            LastName = "Peric",
            Email = "ivana.peric@example.com",
            DateOfBirth = new DateTime(1997, 10, 2),
            MonthlyNetIncome = 1900m,
            RiskToleranceScore = 8
        });

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await ReadJsonAsync<UserProfileDto>(updateResponse);
        Assert.NotNull(updated);
        Assert.Equal("Ivana", updated!.FirstName);

        var deleteResponse = await client.DeleteAsync($"/api/userprofiles/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var deletedLookup = await client.GetAsync($"/api/userprofiles/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, deletedLookup.StatusCode);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ImpulseSpendingDbContext>();
        var softDeleted = await dbContext.UserProfiles.IgnoreQueryFilters().SingleAsync(item => item.Id == created.Id);
        Assert.True(softDeleted.IsDeleted);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenMissing()
    {
        using var factory = new ApiTestFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/userprofiles/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsValidationProblem_ForInvalidPayload()
    {
        using var factory = new ApiTestFactory();
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/userprofiles", new UserProfileUpsertDto
        {
            FirstName = "A",
            LastName = "",
            Email = "not-an-email",
            DateOfBirth = DateTime.UtcNow,
            MonthlyNetIncome = 0m,
            RiskToleranceScore = 0
        });

        await AssertValidationProblemAsync(response, nameof(UserProfileUpsertDto.FirstName), nameof(UserProfileUpsertDto.LastName), nameof(UserProfileUpsertDto.Email), nameof(UserProfileUpsertDto.MonthlyNetIncome), nameof(UserProfileUpsertDto.RiskToleranceScore));
    }
}

public sealed class MerchantsApiControllerTests : ApiCrudIntegrationTestsBase
{
    [Fact]
    public async Task CrudFlow_Works()
    {
        using var factory = new ApiTestFactory();
        await SeedAsync(factory, dbContext =>
        {
            dbContext.Merchants.Add(new Merchant
            {
                Name = "Nova Trgovina",
                Category = "Electronics",
                CountryCode = "HR",
                IsOnlineOnly = true,
                AverageDeliveryDays = 4
            });
        });

        var client = factory.CreateClient();
        var allResponse = await client.GetAsync("/api/merchants");
        Assert.Equal(HttpStatusCode.OK, allResponse.StatusCode);

        var allItems = await ReadJsonAsync<List<MerchantDto>>(allResponse);
        Assert.NotNull(allItems);
        Assert.Single(allItems!);

        var getByIdResponse = await client.GetAsync($"/api/merchants/{allItems![0].Id}");
        Assert.Equal(HttpStatusCode.OK, getByIdResponse.StatusCode);

        var createResponse = await client.PostAsJsonAsync("/api/merchants", new MerchantUpsertDto
        {
            Name = "Market Lab",
            Category = "Groceries",
            CountryCode = "SI",
            IsOnlineOnly = false,
            AverageDeliveryDays = 2
        });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await ReadJsonAsync<MerchantDto>(createResponse);
        Assert.NotNull(created);
        Assert.Equal("Market Lab", created!.Name);

        var updateResponse = await client.PutAsJsonAsync($"/api/merchants/{created.Id}", new MerchantUpsertDto
        {
            Name = "Market Lab Plus",
            Category = "Groceries",
            CountryCode = "SI",
            IsOnlineOnly = true,
            AverageDeliveryDays = 3
        });

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await ReadJsonAsync<MerchantDto>(updateResponse);
        Assert.NotNull(updated);
        Assert.Equal("Market Lab Plus", updated!.Name);

        var deleteResponse = await client.DeleteAsync($"/api/merchants/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var deletedLookup = await client.GetAsync($"/api/merchants/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, deletedLookup.StatusCode);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ImpulseSpendingDbContext>();
        var softDeleted = await dbContext.Merchants.IgnoreQueryFilters().SingleAsync(item => item.Id == created.Id);
        Assert.True(softDeleted.IsDeleted);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenMissing()
    {
        using var factory = new ApiTestFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/merchants/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsValidationProblem_ForInvalidPayload()
    {
        using var factory = new ApiTestFactory();
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/merchants", new MerchantUpsertDto
        {
            Name = "A",
            Category = "",
            CountryCode = "HRV",
            IsOnlineOnly = false,
            AverageDeliveryDays = 0
        });

        await AssertValidationProblemAsync(response, nameof(MerchantUpsertDto.Name), nameof(MerchantUpsertDto.Category), nameof(MerchantUpsertDto.CountryCode), nameof(MerchantUpsertDto.AverageDeliveryDays));
    }
}

public sealed class TagsApiControllerTests : ApiCrudIntegrationTestsBase
{
    [Fact]
    public async Task CrudFlow_Works()
    {
        using var factory = new ApiTestFactory();
        await SeedAsync(factory, dbContext =>
        {
            dbContext.TriggerTypes.Add(new TriggerType
            {
                Name = "Stress Buy",
                ColorHex = "#D9534F",
                Description = "Kupnja pod stresom"
            });
        });

        var client = factory.CreateClient();
        var allResponse = await client.GetAsync("/api/tags");
        Assert.Equal(HttpStatusCode.OK, allResponse.StatusCode);

        var allItems = await ReadJsonAsync<List<TriggerTypeDto>>(allResponse);
        Assert.NotNull(allItems);
        Assert.Single(allItems!);

        var getByIdResponse = await client.GetAsync($"/api/tags/{allItems![0].Id}");
        Assert.Equal(HttpStatusCode.OK, getByIdResponse.StatusCode);

        var createResponse = await client.PostAsJsonAsync("/api/tags", new TriggerTypeUpsertDto
        {
            Name = "Flash Sale",
            ColorHex = "#F0AD4E",
            Description = "Kupnja zbog akcije"
        });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await ReadJsonAsync<TriggerTypeDto>(createResponse);
        Assert.NotNull(created);
        Assert.Equal("Flash Sale", created!.Name);

        var updateResponse = await client.PutAsJsonAsync($"/api/tags/{created.Id}", new TriggerTypeUpsertDto
        {
            Name = "Flash Sale Updated",
            ColorHex = "#5BC0DE",
            Description = "Updated description"
        });

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await ReadJsonAsync<TriggerTypeDto>(updateResponse);
        Assert.NotNull(updated);
        Assert.Equal("Flash Sale Updated", updated!.Name);

        var deleteResponse = await client.DeleteAsync($"/api/tags/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var deletedLookup = await client.GetAsync($"/api/tags/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, deletedLookup.StatusCode);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ImpulseSpendingDbContext>();
        var softDeleted = await dbContext.TriggerTypes.IgnoreQueryFilters().SingleAsync(item => item.Id == created.Id);
        Assert.True(softDeleted.IsDeleted);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenMissing()
    {
        using var factory = new ApiTestFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/tags/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsValidationProblem_ForInvalidPayload()
    {
        using var factory = new ApiTestFactory();
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/tags", new TriggerTypeUpsertDto
        {
            Name = "A",
            ColorHex = "red",
            Description = new string('x', 251)
        });

        await AssertValidationProblemAsync(response, nameof(TriggerTypeUpsertDto.Name), nameof(TriggerTypeUpsertDto.ColorHex), nameof(TriggerTypeUpsertDto.Description));
    }
}

public sealed class BudgetPlansApiControllerTests : ApiCrudIntegrationTestsBase
{
    [Fact]
    public async Task CrudFlow_Works()
    {
        using var factory = new ApiTestFactory();

        UserProfile userProfile = null!;
        await SeedAsync(factory, dbContext =>
        {
            userProfile = new UserProfile
            {
                FirstName = "Petra",
                LastName = "Kovac",
                Email = "petra.kovac@example.com",
                DateOfBirth = new DateTime(1996, 12, 11),
                MonthlyNetIncome = 2100m,
                RiskToleranceScore = 5,
                CreatedAt = DateTime.UtcNow
            };

            dbContext.UserProfiles.Add(userProfile);
        });

        var client = factory.CreateClient();
        var allResponse = await client.GetAsync("/api/budgetplans");
        Assert.Equal(HttpStatusCode.OK, allResponse.StatusCode);

        var allItems = await ReadJsonAsync<List<BudgetPlanDto>>(allResponse);
        Assert.NotNull(allItems);
        Assert.Empty(allItems!);

        var createResponse = await client.PostAsJsonAsync("/api/budgetplans", new BudgetPlanUpsertDto
        {
            UserProfileId = userProfile.Id,
            Name = "Q1 Plan",
            ValidFrom = new DateTime(2026, 1, 1),
            ValidTo = new DateTime(2026, 3, 31),
            MonthlyLimit = 500m,
            ImpulseCapPercentage = 20,
            EssentialCategoryLimit = 200m,
            DiscretionaryCategoryLimit = 100m,
            IsActive = true
        });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await ReadJsonAsync<BudgetPlanDto>(createResponse);
        Assert.NotNull(created);
        Assert.Equal("Q1 Plan", created!.Name);

        var getByIdResponse = await client.GetAsync($"/api/budgetplans/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getByIdResponse.StatusCode);

        var updateResponse = await client.PutAsJsonAsync($"/api/budgetplans/{created.Id}", new BudgetPlanUpsertDto
        {
            UserProfileId = userProfile.Id,
            Name = "Q1 Plan Updated",
            ValidFrom = new DateTime(2026, 1, 1),
            ValidTo = new DateTime(2026, 4, 30),
            MonthlyLimit = 600m,
            ImpulseCapPercentage = 25,
            EssentialCategoryLimit = 250m,
            DiscretionaryCategoryLimit = 120m,
            IsActive = false
        });

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await ReadJsonAsync<BudgetPlanDto>(updateResponse);
        Assert.NotNull(updated);
        Assert.Equal("Q1 Plan Updated", updated!.Name);

        var deleteResponse = await client.DeleteAsync($"/api/budgetplans/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var deletedLookup = await client.GetAsync($"/api/budgetplans/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, deletedLookup.StatusCode);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ImpulseSpendingDbContext>();
        var softDeleted = await dbContext.BudgetPlans.IgnoreQueryFilters().SingleAsync(item => item.Id == created.Id);
        Assert.True(softDeleted.IsDeleted);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenMissing()
    {
        using var factory = new ApiTestFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/budgetplans/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsValidationProblem_ForInvalidPayload()
    {
        using var factory = new ApiTestFactory();
        UserProfile userProfile = null!;
        await SeedAsync(factory, dbContext =>
        {
            userProfile = new UserProfile
            {
                FirstName = "Maja",
                LastName = "Ivic",
                Email = "maja.ivic@example.com",
                DateOfBirth = new DateTime(1995, 5, 5),
                MonthlyNetIncome = 1800m,
                RiskToleranceScore = 6,
                CreatedAt = DateTime.UtcNow
            };

            dbContext.UserProfiles.Add(userProfile);
        });

        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/budgetplans", new BudgetPlanUpsertDto
        {
            UserProfileId = userProfile.Id,
            Name = "Budget",
            MonthlyLimit = 100m,
            EssentialCategoryLimit = 80m,
            DiscretionaryCategoryLimit = 40m,
            IsActive = true
        });

        await AssertValidationProblemAsync(response, nameof(BudgetPlanUpsertDto.EssentialCategoryLimit), nameof(BudgetPlanUpsertDto.DiscretionaryCategoryLimit), nameof(BudgetPlanUpsertDto.MonthlyLimit));
    }
}

public sealed class SpendingSessionsApiControllerTests : ApiCrudIntegrationTestsBase
{
    [Fact]
    public async Task CrudFlow_Works()
    {
        using var factory = new ApiTestFactory();
        UserProfile userProfile = null!;
        await SeedAsync(factory, dbContext =>
        {
            userProfile = new UserProfile
            {
                FirstName = "Ivan",
                LastName = "Maric",
                Email = "ivan.maric@example.com",
                DateOfBirth = new DateTime(1994, 8, 19),
                MonthlyNetIncome = 2200m,
                RiskToleranceScore = 4,
                CreatedAt = DateTime.UtcNow
            };

            dbContext.UserProfiles.Add(userProfile);
        });

        var client = factory.CreateClient();
        var createResponse = await client.PostAsJsonAsync("/api/spendingsessions", new SpendingSessionUpsertDto
        {
            UserProfileId = userProfile.Id,
            StartedAt = new DateTime(2026, 5, 1, 10, 0, 0, DateTimeKind.Utc),
            EndedAt = new DateTime(2026, 5, 1, 10, 30, 0, DateTimeKind.Utc),
            Platform = "Web",
            Channel = "Chrome",
            SessionBudget = 150m,
            SpentAmount = 60m,
            ItemsViewed = 8,
            ItemsAddedToCart = 2,
            CheckoutCompleted = true
        });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await ReadJsonAsync<SpendingSessionDto>(createResponse);
        Assert.NotNull(created);
        Assert.Equal("Web", created!.Platform);

        var allResponse = await client.GetAsync("/api/spendingsessions");
        Assert.Equal(HttpStatusCode.OK, allResponse.StatusCode);
        var allItems = await ReadJsonAsync<List<SpendingSessionDto>>(allResponse);
        Assert.NotNull(allItems);
        Assert.Single(allItems!);

        var updateResponse = await client.PutAsJsonAsync($"/api/spendingsessions/{created.Id}", new SpendingSessionUpsertDto
        {
            UserProfileId = userProfile.Id,
            StartedAt = new DateTime(2026, 5, 1, 10, 0, 0, DateTimeKind.Utc),
            EndedAt = new DateTime(2026, 5, 1, 10, 45, 0, DateTimeKind.Utc),
            Platform = "Web",
            Channel = "Firefox",
            SessionBudget = 160m,
            SpentAmount = 70m,
            ItemsViewed = 10,
            ItemsAddedToCart = 3,
            CheckoutCompleted = false
        });

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await ReadJsonAsync<SpendingSessionDto>(updateResponse);
        Assert.NotNull(updated);
        Assert.Equal("Firefox", updated!.Channel);

        var deleteResponse = await client.DeleteAsync($"/api/spendingsessions/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var deletedLookup = await client.GetAsync($"/api/spendingsessions/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, deletedLookup.StatusCode);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ImpulseSpendingDbContext>();
        var softDeleted = await dbContext.SpendingSessions.IgnoreQueryFilters().SingleAsync(item => item.Id == created.Id);
        Assert.True(softDeleted.IsDeleted);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenMissing()
    {
        using var factory = new ApiTestFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/spendingsessions/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsValidationProblem_ForInvalidPayload()
    {
        using var factory = new ApiTestFactory();
        UserProfile userProfile = null!;
        await SeedAsync(factory, dbContext =>
        {
            userProfile = new UserProfile
            {
                FirstName = "Luka",
                LastName = "Knezevic",
                Email = "luka.knezevic@example.com",
                DateOfBirth = new DateTime(1993, 1, 8),
                MonthlyNetIncome = 2500m,
                RiskToleranceScore = 5,
                CreatedAt = DateTime.UtcNow
            };

            dbContext.UserProfiles.Add(userProfile);
        });

        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/spendingsessions", new SpendingSessionUpsertDto
        {
            UserProfileId = userProfile.Id,
            StartedAt = new DateTime(2026, 5, 1, 10, 0, 0, DateTimeKind.Utc),
            EndedAt = new DateTime(2026, 5, 1, 9, 45, 0, DateTimeKind.Utc),
            Platform = "Web",
            Channel = "Mobile",
            SessionBudget = 10m,
            SpentAmount = 5m,
            ItemsViewed = 1,
            ItemsAddedToCart = 2,
            CheckoutCompleted = false
        });

        await AssertValidationProblemAsync(response, nameof(SpendingSessionUpsertDto.EndedAt), nameof(SpendingSessionUpsertDto.StartedAt), nameof(SpendingSessionUpsertDto.ItemsAddedToCart), nameof(SpendingSessionUpsertDto.ItemsViewed));
    }
}

public sealed class WishlistItemsApiControllerTests : ApiCrudIntegrationTestsBase
{
    [Fact]
    public async Task CrudFlow_Works()
    {
        using var factory = new ApiTestFactory();
        UserProfile userProfile = null!;
        await SeedAsync(factory, dbContext =>
        {
            userProfile = new UserProfile
            {
                FirstName = "Sara",
                LastName = "Jukic",
                Email = "sara.jukic@example.com",
                DateOfBirth = new DateTime(1999, 2, 20),
                MonthlyNetIncome = 1750m,
                RiskToleranceScore = 6,
                CreatedAt = DateTime.UtcNow
            };

            dbContext.UserProfiles.Add(userProfile);
        });

        var client = factory.CreateClient();
        var createResponse = await client.PostAsJsonAsync("/api/wishlistitems", new WishlistItemUpsertDto
        {
            UserProfileId = userProfile.Id,
            Name = "Laptop",
            DesiredPrice = 1200m,
            CurrentPrice = 1100m,
            Priority = 4,
            AddedAt = DateTime.UtcNow,
            TargetPurchaseDate = DateTime.UtcNow.AddDays(30),
            Reason = "Need for work",
            IsPurchased = false,
            LinkUrl = "https://example.com/laptop"
        });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await ReadJsonAsync<WishlistItemDto>(createResponse);
        Assert.NotNull(created);
        Assert.Equal("Laptop", created!.Name);

        var allResponse = await client.GetAsync("/api/wishlistitems");
        Assert.Equal(HttpStatusCode.OK, allResponse.StatusCode);
        var allItems = await ReadJsonAsync<List<WishlistItemDto>>(allResponse);
        Assert.NotNull(allItems);
        Assert.Single(allItems!);

        var updateResponse = await client.PutAsJsonAsync($"/api/wishlistitems/{created.Id}", new WishlistItemUpsertDto
        {
            UserProfileId = userProfile.Id,
            Name = "Laptop Pro",
            DesiredPrice = 1500m,
            CurrentPrice = 1400m,
            Priority = 5,
            AddedAt = DateTime.UtcNow,
            TargetPurchaseDate = DateTime.UtcNow.AddDays(45),
            Reason = "Better model",
            IsPurchased = true,
            LinkUrl = "https://example.com/laptop-pro"
        });

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await ReadJsonAsync<WishlistItemDto>(updateResponse);
        Assert.NotNull(updated);
        Assert.Equal("Laptop Pro", updated!.Name);

        var deleteResponse = await client.DeleteAsync($"/api/wishlistitems/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var deletedLookup = await client.GetAsync($"/api/wishlistitems/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, deletedLookup.StatusCode);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ImpulseSpendingDbContext>();
        var softDeleted = await dbContext.WishlistItems.IgnoreQueryFilters().SingleAsync(item => item.Id == created.Id);
        Assert.True(softDeleted.IsDeleted);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenMissing()
    {
        using var factory = new ApiTestFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/wishlistitems/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsValidationProblem_ForInvalidPayload()
    {
        using var factory = new ApiTestFactory();
        UserProfile userProfile = null!;
        await SeedAsync(factory, dbContext =>
        {
            userProfile = new UserProfile
            {
                FirstName = "Marta",
                LastName = "Vuk",
                Email = "marta.vuk@example.com",
                DateOfBirth = new DateTime(1992, 11, 29),
                MonthlyNetIncome = 2300m,
                RiskToleranceScore = 7,
                CreatedAt = DateTime.UtcNow
            };

            dbContext.UserProfiles.Add(userProfile);
        });

        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/wishlistitems", new WishlistItemUpsertDto
        {
            UserProfileId = userProfile.Id,
            Name = "Laptop",
            DesiredPrice = 1200m,
            CurrentPrice = 1100m,
            Priority = 4,
            AddedAt = DateTime.UtcNow,
            TargetPurchaseDate = DateTime.UtcNow.AddDays(-1),
            Reason = "Need for work",
            IsPurchased = false,
            LinkUrl = "https://example.com/laptop"
        });

        await AssertValidationProblemAsync(response, nameof(WishlistItemUpsertDto.TargetPurchaseDate), nameof(WishlistItemUpsertDto.AddedAt));
    }
}

public sealed class PurchasesApiControllerTests : ApiCrudIntegrationTestsBase
{
    [Fact]
    public async Task CrudFlow_Works()
    {
        using var factory = new ApiTestFactory();

        UserProfile userProfile = null!;
        Merchant merchant = null!;
        TriggerType tag = null!;
        BudgetPlan budgetPlan = null!;
        SpendingSession session = null!;
        WishlistItem wishlistItem = null!;

        await SeedAsync(factory, dbContext =>
        {
            userProfile = new UserProfile
            {
                FirstName = "Dora",
                LastName = "Nedic",
                Email = "dora.nedic@example.com",
                DateOfBirth = new DateTime(1991, 6, 17),
                MonthlyNetIncome = 2600m,
                RiskToleranceScore = 5,
                CreatedAt = DateTime.UtcNow
            };

            merchant = new Merchant
            {
                Name = "Tech Store",
                Category = "Electronics",
                CountryCode = "HR",
                IsOnlineOnly = true,
                AverageDeliveryDays = 5
            };

            tag = new TriggerType
            {
                Name = "Stress Buy",
                ColorHex = "#D9534F",
                Description = "Kupnja pod stresom"
            };

            dbContext.UserProfiles.Add(userProfile);
            dbContext.Merchants.Add(merchant);
            dbContext.TriggerTypes.Add(tag);
            dbContext.SaveChanges();

            budgetPlan = new BudgetPlan
            {
                UserProfileId = userProfile.Id,
                Name = "Monthly Plan",
                ValidFrom = new DateTime(2026, 5, 1),
                ValidTo = new DateTime(2026, 5, 31),
                MonthlyLimit = 700m,
                ImpulseCapPercentage = 25,
                EssentialCategoryLimit = 300m,
                DiscretionaryCategoryLimit = 150m,
                IsActive = true
            };

            session = new SpendingSession
            {
                UserProfileId = userProfile.Id,
                StartedAt = new DateTime(2026, 5, 1, 9, 0, 0, DateTimeKind.Utc),
                EndedAt = new DateTime(2026, 5, 1, 9, 30, 0, DateTimeKind.Utc),
                Platform = "Web",
                Channel = "Chrome",
                SessionBudget = 150m,
                SpentAmount = 60m,
                ItemsViewed = 8,
                ItemsAddedToCart = 2,
                CheckoutCompleted = true
            };

            wishlistItem = new WishlistItem
            {
                UserProfileId = userProfile.Id,
                Name = "Headphones",
                DesiredPrice = 200m,
                CurrentPrice = 180m,
                Priority = 3,
                AddedAt = DateTime.UtcNow,
                TargetPurchaseDate = DateTime.UtcNow.AddDays(14),
                Reason = "Need for calls",
                IsPurchased = false,
                LinkUrl = "https://example.com/headphones"
            };

            dbContext.BudgetPlans.Add(budgetPlan);
            dbContext.SpendingSessions.Add(session);
            dbContext.WishlistItems.Add(wishlistItem);
            dbContext.SaveChanges();
        });

        var client = factory.CreateClient();
        var createResponse = await client.PostAsJsonAsync("/api/purchases", new PurchaseUpsertDto
        {
            UserProfileId = userProfile.Id,
            MerchantId = merchant.Id,
            SpendingSessionId = session.Id,
            BudgetPlanId = budgetPlan.Id,
            WishlistItemId = wishlistItem.Id,
            Title = "Wireless Mouse",
            Amount = 45m,
            Currency = "EUR",
            PurchasedAt = new DateTime(2026, 5, 1, 9, 10, 0, DateTimeKind.Utc),
            MoodBeforePurchase = "Stressed",
            NeedLevel = 6,
            TriggerType = ImpulseTriggerType.Stress,
            Installments = 1,
            Notes = "Needed for work",
            TriggerTypeIds = new List<int> { tag.Id }
        });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await ReadJsonAsync<PurchaseDto>(createResponse);
        Assert.NotNull(created);
        Assert.Equal("Wireless Mouse", created!.Title);
        Assert.Single(created.TriggerTypes);

        var allResponse = await client.GetAsync("/api/purchases");
        Assert.Equal(HttpStatusCode.OK, allResponse.StatusCode);
        var allItems = await ReadJsonAsync<List<PurchaseDto>>(allResponse);
        Assert.NotNull(allItems);
        Assert.Single(allItems!);

        var updateResponse = await client.PutAsJsonAsync($"/api/purchases/{created.Id}", new PurchaseUpsertDto
        {
            UserProfileId = userProfile.Id,
            MerchantId = merchant.Id,
            SpendingSessionId = session.Id,
            BudgetPlanId = budgetPlan.Id,
            WishlistItemId = wishlistItem.Id,
            Title = "Wireless Mouse Pro",
            Amount = 55m,
            Currency = "EUR",
            PurchasedAt = new DateTime(2026, 5, 1, 9, 10, 0, DateTimeKind.Utc),
            MoodBeforePurchase = "Excited",
            NeedLevel = 7,
            TriggerType = ImpulseTriggerType.Recommendation,
            Installments = 2,
            Notes = "Better version",
            TriggerTypeIds = new List<int> { tag.Id }
        });

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await ReadJsonAsync<PurchaseDto>(updateResponse);
        Assert.NotNull(updated);
        Assert.Equal("Wireless Mouse Pro", updated!.Title);

        var deleteResponse = await client.DeleteAsync($"/api/purchases/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var deletedLookup = await client.GetAsync($"/api/purchases/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, deletedLookup.StatusCode);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ImpulseSpendingDbContext>();
        var softDeleted = await dbContext.Purchases.IgnoreQueryFilters().SingleAsync(item => item.Id == created.Id);
        Assert.True(softDeleted.IsDeleted);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenMissing()
    {
        using var factory = new ApiTestFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/purchases/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsValidationProblem_ForInvalidPayload()
    {
        using var factory = new ApiTestFactory();

        UserProfile userProfile = null!;
        Merchant merchant = null!;
        await SeedAsync(factory, dbContext =>
        {
            userProfile = new UserProfile
            {
                FirstName = "Nina",
                LastName = "Kralj",
                Email = "nina.kralj@example.com",
                DateOfBirth = new DateTime(1990, 4, 9),
                MonthlyNetIncome = 2400m,
                RiskToleranceScore = 5,
                CreatedAt = DateTime.UtcNow
            };

            merchant = new Merchant
            {
                Name = "Office Shop",
                Category = "Office",
                CountryCode = "HR",
                IsOnlineOnly = true,
                AverageDeliveryDays = 3
            };

            dbContext.UserProfiles.Add(userProfile);
            dbContext.Merchants.Add(merchant);
        });

        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/purchases", new PurchaseUpsertDto
        {
            UserProfileId = userProfile.Id,
            MerchantId = merchant.Id,
            Title = "Keyboard",
            Amount = 45m,
            Currency = "EUR",
            PurchasedAt = new DateTime(2026, 5, 1, 9, 10, 0, DateTimeKind.Utc),
            MoodBeforePurchase = "Focused",
            NeedLevel = 5,
            TriggerType = ImpulseTriggerType.Other,
            Installments = 1,
            Notes = "Needed for work",
            TriggerTypeIds = new List<int> { 999 }
        });

        await AssertValidationProblemAsync(response, nameof(PurchaseUpsertDto.TriggerTypeIds));
    }

    [Fact]
    public async Task SpendingSession_ReadsSpendingFromLinkedPurchases()
    {
        using var factory = new ApiTestFactory();

        UserProfile userProfile = null!;
        Merchant merchant = null!;
        SpendingSession session = null!;

        await SeedAsync(factory, dbContext =>
        {
            userProfile = new UserProfile
            {
                FirstName = "Ana",
                LastName = "Matic",
                Email = "ana.matic@example.com",
                DateOfBirth = new DateTime(1993, 2, 11),
                MonthlyNetIncome = 2500m,
                RiskToleranceScore = 5,
                CreatedAt = DateTime.UtcNow
            };

            merchant = new Merchant
            {
                Name = "Coffee Bar",
                Category = "Food",
                CountryCode = "HR",
                IsOnlineOnly = false,
                AverageDeliveryDays = 1
            };

            dbContext.UserProfiles.Add(userProfile);
            dbContext.Merchants.Add(merchant);
            dbContext.SaveChanges();

            session = new SpendingSession
            {
                UserProfileId = userProfile.Id,
                StartedAt = DateTime.UtcNow.AddHours(-1),
                EndedAt = DateTime.UtcNow,
                Platform = "Web",
                Channel = "Chrome",
                SessionBudget = 100m,
                SpentAmount = 0m,
                ItemsViewed = 4,
                ItemsAddedToCart = 1,
                CheckoutCompleted = true
            };

            dbContext.SpendingSessions.Add(session);
            dbContext.SaveChanges();

            dbContext.Purchases.Add(new Purchase
            {
                UserProfileId = userProfile.Id,
                MerchantId = merchant.Id,
                SpendingSessionId = session.Id,
                Title = "Latte",
                Amount = 3.50m,
                Currency = "EUR",
                PurchasedAt = DateTime.UtcNow.AddMinutes(-45),
                MoodBeforePurchase = "Neutral",
                NeedLevel = 2,
                TriggerType = ImpulseTriggerType.Other,
                Installments = 1
            });

            dbContext.Purchases.Add(new Purchase
            {
                UserProfileId = userProfile.Id,
                MerchantId = merchant.Id,
                SpendingSessionId = session.Id,
                Title = "Sandwich",
                Amount = 6.50m,
                Currency = "EUR",
                PurchasedAt = DateTime.UtcNow.AddMinutes(-30),
                MoodBeforePurchase = "Hungry",
                NeedLevel = 3,
                TriggerType = ImpulseTriggerType.Other,
                Installments = 1
            });

            dbContext.SaveChanges();
        });

        var client = factory.CreateClient();
        var response = await client.GetAsync($"/api/spendingsessions/{session.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dto = await ReadJsonAsync<SpendingSessionDto>(response);
        Assert.NotNull(dto);
        Assert.Equal(10.00m, dto!.SpentAmount);
    }

    [Fact]
    public async Task Create_ReturnsValidationProblem_WhenBudgetPlanLimitWouldBeExceeded()
    {
        using var factory = new ApiTestFactory();

        UserProfile userProfile = null!;
        Merchant merchant = null!;
        BudgetPlan budgetPlan = null!;

        await SeedAsync(factory, dbContext =>
        {
            userProfile = new UserProfile
            {
                FirstName = "Ivan",
                LastName = "Peric",
                Email = "ivan.peric@example.com",
                DateOfBirth = new DateTime(1991, 9, 5),
                MonthlyNetIncome = 2400m,
                RiskToleranceScore = 6,
                CreatedAt = DateTime.UtcNow
            };

            merchant = new Merchant
            {
                Name = "Electro Shop",
                Category = "Electronics",
                CountryCode = "HR",
                IsOnlineOnly = true,
                AverageDeliveryDays = 3
            };

            dbContext.UserProfiles.Add(userProfile);
            dbContext.Merchants.Add(merchant);
            dbContext.SaveChanges();

            budgetPlan = new BudgetPlan
            {
                UserProfileId = userProfile.Id,
                Name = "Monthly Tech",
                ValidFrom = new DateTime(2026, 6, 1),
                ValidTo = new DateTime(2026, 6, 30),
                MonthlyLimit = 500m,
                ImpulseCapPercentage = 20,
                EssentialCategoryLimit = 250m,
                DiscretionaryCategoryLimit = 150m,
                IsActive = true
            };

            dbContext.BudgetPlans.Add(budgetPlan);
            dbContext.SaveChanges();

            dbContext.Purchases.Add(new Purchase
            {
                UserProfileId = userProfile.Id,
                MerchantId = merchant.Id,
                BudgetPlanId = budgetPlan.Id,
                Title = "Headphones",
                Amount = 200m,
                Currency = "EUR",
                PurchasedAt = DateTime.UtcNow.AddDays(-1),
                MoodBeforePurchase = "Focused",
                NeedLevel = 4,
                TriggerType = ImpulseTriggerType.Recommendation,
                Installments = 1
            });

            dbContext.SaveChanges();
        });

        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/purchases", new PurchaseUpsertDto
        {
            UserProfileId = userProfile.Id,
            MerchantId = merchant.Id,
            BudgetPlanId = budgetPlan.Id,
            Title = "Monitor",
            Amount = 400m,
            Currency = "EUR",
            PurchasedAt = DateTime.UtcNow,
            MoodBeforePurchase = "Tempted",
            NeedLevel = 5,
            TriggerType = ImpulseTriggerType.Recommendation,
            Installments = 1,
            TriggerTypeIds = new List<int>()
        });

        await AssertValidationProblemAsync(response, nameof(PurchaseUpsertDto.BudgetPlanId));
    }

    [Fact]
    public async Task Create_ReturnsValidationProblem_WhenWishlistItemIsAlreadyConverted()
    {
        using var factory = new ApiTestFactory();

        UserProfile userProfile = null!;
        Merchant merchant = null!;
        WishlistItem wishlistItem = null!;

        await SeedAsync(factory, dbContext =>
        {
            userProfile = new UserProfile
            {
                FirstName = "Mia",
                LastName = "Jelic",
                Email = "mia.jelic@example.com",
                DateOfBirth = new DateTime(1992, 8, 21),
                MonthlyNetIncome = 2300m,
                RiskToleranceScore = 4,
                CreatedAt = DateTime.UtcNow
            };

            merchant = new Merchant
            {
                Name = "Gadget Shop",
                Category = "Electronics",
                CountryCode = "HR",
                IsOnlineOnly = true,
                AverageDeliveryDays = 4
            };

            dbContext.UserProfiles.Add(userProfile);
            dbContext.Merchants.Add(merchant);
            dbContext.SaveChanges();

            wishlistItem = new WishlistItem
            {
                UserProfileId = userProfile.Id,
                Name = "Wireless Keyboard",
                DesiredPrice = 120m,
                CurrentPrice = 100m,
                Priority = 2,
                AddedAt = DateTime.UtcNow,
                TargetPurchaseDate = DateTime.UtcNow.AddDays(7),
                Reason = "Work setup",
                IsPurchased = true,
                LinkUrl = "https://example.com/keyboard"
            };

            dbContext.WishlistItems.Add(wishlistItem);
            dbContext.SaveChanges();

            dbContext.Purchases.Add(new Purchase
            {
                UserProfileId = userProfile.Id,
                MerchantId = merchant.Id,
                WishlistItemId = wishlistItem.Id,
                Title = "Wireless Keyboard",
                Amount = 100m,
                Currency = "EUR",
                PurchasedAt = DateTime.UtcNow.AddMinutes(-10),
                MoodBeforePurchase = "Calm",
                NeedLevel = 5,
                TriggerType = ImpulseTriggerType.Recommendation,
                Installments = 1
            });

            dbContext.SaveChanges();
        });

        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/purchases", new PurchaseUpsertDto
        {
            UserProfileId = userProfile.Id,
            MerchantId = merchant.Id,
            WishlistItemId = wishlistItem.Id,
            Title = "Wireless Keyboard Pro",
            Amount = 130m,
            Currency = "EUR",
            PurchasedAt = DateTime.UtcNow,
            MoodBeforePurchase = "Interested",
            NeedLevel = 6,
            TriggerType = ImpulseTriggerType.Recommendation,
            Installments = 1,
            TriggerTypeIds = new List<int>()
        });

        await AssertValidationProblemAsync(response, nameof(PurchaseUpsertDto.WishlistItemId));
    }
}