using impulse_spending_tracker.Models;
using impulse_spending_tracker.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var labUsers = BuildLab1Users();
builder.Services.AddSingleton(labUsers);
builder.Services.AddSingleton<UserProfileMockRepository>();

var app = builder.Build();

RunLab1LinqQueries(labUsers, app.Logger);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();

void RunLab1LinqQueries(List<UserProfile> users, ILogger logger)
{
    var allPurchases = users
        .SelectMany(u => u.Purchases)
        .ToList();

    var expensivePurchases = allPurchases
        .Where(p => p.Amount >= 180m)
        .OrderByDescending(p => p.Amount)
        .ToList();

    var stressTriggeredPurchases = allPurchases
        .Where(p => p.TriggerType == ImpulseTriggerType.Stress || p.Tags.Any(t => t.Name == "Stress Buy"))
        .ToList();

    var topPurchase = allPurchases
        .OrderByDescending(p => p.Amount)
        .FirstOrDefault();

    var singlePetra = users
        .SingleOrDefault(u => u.Email == "petra.maric@example.com");

    var purchaseCountByUser = users
        .Select(u => new
        {
            User = $"{u.FirstName} {u.LastName}",
            PurchaseCount = u.Purchases.Count(),
            TotalSpent = u.Purchases.Sum(p => p.Amount)
        })
        .OrderByDescending(x => x.TotalSpent)
        .ToList();

    var usersWithPurchaseAboveIncomeShare = users
        .Where(u => u.Purchases.Any(p => p.Amount > (u.MonthlyNetIncome * 0.10m)))
        .Select(u => $"{u.FirstName} {u.LastName}")
        .ToList();

    var sessionToPurchaseProjection = users
        .SelectMany(u => u.Sessions.Select(s => new
        {
            User = $"{u.FirstName} {u.LastName}",
            SessionStart = s.StartedAt,
            PurchaseTitles = s.Purchases.Select(p => p.Title).ToList(),
            SessionSpent = s.SpentAmount
        }))
        .OrderByDescending(x => x.SessionSpent)
        .ToList();

    var purchasesByTrigger = allPurchases
        .GroupBy(p => p.TriggerType)
        .Select(g => new
        {
            Trigger = g.Key,
            Count = g.Count(),
            Sum = g.Sum(x => x.Amount)
        })
        .OrderByDescending(x => x.Count)
        .ThenByDescending(x => x.Sum)
        .ToList();

    logger.LogInformation("=== LAB 1 LINQ QUERIES ===");
    logger.LogInformation("Ukupno kupnji: {Count}", allPurchases.Count());
    logger.LogInformation("Kupnje skuplje ili jednake 180 EUR: {Count}", expensivePurchases.Count());
    logger.LogInformation("Kupnje s triggerom/tagom stress: {Count}", stressTriggeredPurchases.Count());

    if (topPurchase is not null)
    {
        logger.LogInformation(
            "Najskuplja kupnja: {Title} ({Amount} {Currency})",
            topPurchase.Title,
            topPurchase.Amount,
            topPurchase.Currency);
    }

    logger.LogInformation(
        "SingleOrDefault provjera za Petru: {Exists}",
        singlePetra is not null ? "pronadjena" : "nije pronadjena");

    foreach (var item in purchaseCountByUser)
    {
        logger.LogInformation(
            "Korisnik {User} -> broj kupnji: {Count}, ukupno potroseno: {Spent}",
            item.User,
            item.PurchaseCount,
            item.TotalSpent);
    }

    logger.LogInformation(
        "Korisnici s barem jednom kupnjom > 10% mjesecnog prihoda: {Users}",
        usersWithPurchaseAboveIncomeShare.Count > 0 ? string.Join(", ", usersWithPurchaseAboveIncomeShare) : "nema");

    foreach (var session in sessionToPurchaseProjection)
    {
        logger.LogInformation(
            "Session {User} ({Start}) -> potroseno {Spent}, kupljeno: {Titles}",
            session.User,
            session.SessionStart,
            session.SessionSpent,
            session.PurchaseTitles.Count > 0 ? string.Join(", ", session.PurchaseTitles) : "nista");
    }

    foreach (var trigger in purchasesByTrigger)
    {
        logger.LogInformation(
            "Trigger {Trigger} -> broj: {Count}, iznos: {Sum}",
            trigger.Trigger,
            trigger.Count,
            trigger.Sum);
    }
}

List<UserProfile> BuildLab1Users()
{
    var techStore = new Merchant
    {
        Name = "Tech Basket",
        Category = "Electronics",
        CountryCode = "HR",
        IsOnlineOnly = true,
        AverageDeliveryDays = 2
    };

    var fashionStore = new Merchant
    {
        Name = "StreetFit",
        Category = "Fashion",
        CountryCode = "HR",
        IsOnlineOnly = false,
        AverageDeliveryDays = 4
    };

    var homeStore = new Merchant
    {
        Name = "HomeMood",
        Category = "Home",
        CountryCode = "SI",
        IsOnlineOnly = true,
        AverageDeliveryDays = 5
    };

    var stressTag = new Tag { Name = "Stress Buy", ColorHex = "#D9534F", Description = "Kupnja pod stresom" };
    var saleTag = new Tag { Name = "Flash Sale", ColorHex = "#F0AD4E", Description = "Kupnja zbog hitne akcije" };
    var socialTag = new Tag { Name = "Social Influence", ColorHex = "#5BC0DE", Description = "Kupnja nakon sadržaja s društvenih mreža" };

    var user1 = new UserProfile
    {
        FirstName = "Luka",
        LastName = "Kovač",
        Email = "luka.kovac@example.com",
        DateOfBirth = new DateTime(1999, 4, 11),
        MonthlyNetIncome = 1650m,
        RiskToleranceScore = 7,
        CreatedAt = DateTime.UtcNow.AddMonths(-10)
    };

    var user1Plan = new BudgetPlan
    {
        UserProfileId = user1.Id,
        UserProfile = user1,
        Name = "Q1 Plan",
        ValidFrom = new DateTime(2026, 1, 1),
        ValidTo = new DateTime(2026, 3, 31),
        MonthlyLimit = 650m,
        ImpulseCapPercentage = 25,
        EssentialCategoryLimit = 350m,
        DiscretionaryCategoryLimit = 300m,
        IsActive = true
    };

    var user1Session = new SpendingSession
    {
        UserProfileId = user1.Id,
        UserProfile = user1,
        StartedAt = DateTime.UtcNow.AddDays(-9).AddHours(-1),
        EndedAt = DateTime.UtcNow.AddDays(-9),
        Platform = "Web",
        Channel = "Instagram Ad",
        SessionBudget = 220m,
        SpentAmount = 189.99m,
        ItemsViewed = 22,
        ItemsAddedToCart = 4,
        CheckoutCompleted = true
    };

    var user1Wishlist = new WishlistItem
    {
        UserProfileId = user1.Id,
        UserProfile = user1,
        Name = "Noise Cancelling Headphones",
        DesiredPrice = 120m,
        CurrentPrice = 189.99m,
        Priority = 2,
        AddedAt = DateTime.UtcNow.AddDays(-30),
        TargetPurchaseDate = DateTime.UtcNow.AddDays(20),
        Reason = "Za fokus tijekom učenja",
        IsPurchased = true,
        LinkUrl = "https://example.com/headphones"
    };

    var user1Purchase = new Purchase
    {
        UserProfileId = user1.Id,
        UserProfile = user1,
        MerchantId = techStore.Id,
        Merchant = techStore,
        SpendingSessionId = user1Session.Id,
        SpendingSession = user1Session,
        BudgetPlanId = user1Plan.Id,
        BudgetPlan = user1Plan,
        WishlistItemId = user1Wishlist.Id,
        WishlistItem = user1Wishlist,
        Title = "ANC Headphones X200",
        Amount = 189.99m,
        Currency = "EUR",
        PurchasedAt = DateTime.UtcNow.AddDays(-9),
        MoodBeforePurchase = "Nervous before exam",
        NeedLevel = 6,
        TriggerType = ImpulseTriggerType.SocialMedia,
        Installments = 1,
        Notes = "Kupnja odmah nakon oglasa i recenzije influencera",
        Tags = new List<Tag> { stressTag, socialTag }
    };

    var user2 = new UserProfile
    {
        FirstName = "Petra",
        LastName = "Marić",
        Email = "petra.maric@example.com",
        DateOfBirth = new DateTime(2000, 8, 23),
        MonthlyNetIncome = 1400m,
        RiskToleranceScore = 5,
        CreatedAt = DateTime.UtcNow.AddMonths(-7)
    };

    var user2Plan = new BudgetPlan
    {
        UserProfileId = user2.Id,
        UserProfile = user2,
        Name = "Spring Plan",
        ValidFrom = new DateTime(2026, 3, 1),
        ValidTo = new DateTime(2026, 5, 31),
        MonthlyLimit = 520m,
        ImpulseCapPercentage = 20,
        EssentialCategoryLimit = 320m,
        DiscretionaryCategoryLimit = 200m,
        IsActive = true
    };

    var user2Session = new SpendingSession
    {
        UserProfileId = user2.Id,
        UserProfile = user2,
        StartedAt = DateTime.UtcNow.AddDays(-5).AddMinutes(-40),
        EndedAt = DateTime.UtcNow.AddDays(-5),
        Platform = "Mobile App",
        Channel = "Push Notification",
        SessionBudget = 150m,
        SpentAmount = 136.50m,
        ItemsViewed = 18,
        ItemsAddedToCart = 3,
        CheckoutCompleted = true
    };

    var user2Wishlist = new WishlistItem
    {
        UserProfileId = user2.Id,
        UserProfile = user2,
        Name = "Sneakers",
        DesiredPrice = 90m,
        CurrentPrice = 136.50m,
        Priority = 1,
        AddedAt = DateTime.UtcNow.AddDays(-14),
        TargetPurchaseDate = DateTime.UtcNow.AddDays(15),
        Reason = "Zamjena starih tenisica",
        IsPurchased = true,
        LinkUrl = "https://example.com/sneakers"
    };

    var user2Purchase = new Purchase
    {
        UserProfileId = user2.Id,
        UserProfile = user2,
        MerchantId = fashionStore.Id,
        Merchant = fashionStore,
        SpendingSessionId = user2Session.Id,
        SpendingSession = user2Session,
        BudgetPlanId = user2Plan.Id,
        BudgetPlan = user2Plan,
        WishlistItemId = user2Wishlist.Id,
        WishlistItem = user2Wishlist,
        Title = "Street Sneakers V3",
        Amount = 136.50m,
        Currency = "EUR",
        PurchasedAt = DateTime.UtcNow.AddDays(-5),
        MoodBeforePurchase = "Excited",
        NeedLevel = 5,
        TriggerType = ImpulseTriggerType.FlashSale,
        Installments = 2,
        Notes = "30% popusta i odbrojavanje akcije",
        Tags = new List<Tag> { saleTag }
    };

    var user3 = new UserProfile
    {
        FirstName = "Ana",
        LastName = "Horvat",
        Email = "ana.horvat@example.com",
        DateOfBirth = new DateTime(1997, 1, 6),
        MonthlyNetIncome = 2100m,
        RiskToleranceScore = 4,
        CreatedAt = DateTime.UtcNow.AddMonths(-12)
    };

    var user3Plan = new BudgetPlan
    {
        UserProfileId = user3.Id,
        UserProfile = user3,
        Name = "Home Balance",
        ValidFrom = new DateTime(2026, 2, 1),
        ValidTo = new DateTime(2026, 4, 30),
        MonthlyLimit = 800m,
        ImpulseCapPercentage = 18,
        EssentialCategoryLimit = 500m,
        DiscretionaryCategoryLimit = 300m,
        IsActive = true
    };

    var user3Session = new SpendingSession
    {
        UserProfileId = user3.Id,
        UserProfile = user3,
        StartedAt = DateTime.UtcNow.AddDays(-2).AddMinutes(-55),
        EndedAt = DateTime.UtcNow.AddDays(-2),
        Platform = "Web",
        Channel = "Email Recommendation",
        SessionBudget = 250m,
        SpentAmount = 229.00m,
        ItemsViewed = 27,
        ItemsAddedToCart = 5,
        CheckoutCompleted = true
    };

    var user3Wishlist = new WishlistItem
    {
        UserProfileId = user3.Id,
        UserProfile = user3,
        Name = "Air Fryer",
        DesiredPrice = 170m,
        CurrentPrice = 229.00m,
        Priority = 3,
        AddedAt = DateTime.UtcNow.AddDays(-45),
        TargetPurchaseDate = DateTime.UtcNow.AddDays(40),
        Reason = "Brža priprema obroka",
        IsPurchased = true,
        LinkUrl = "https://example.com/air-fryer"
    };

    var user3Purchase = new Purchase
    {
        UserProfileId = user3.Id,
        UserProfile = user3,
        MerchantId = homeStore.Id,
        Merchant = homeStore,
        SpendingSessionId = user3Session.Id,
        SpendingSession = user3Session,
        BudgetPlanId = user3Plan.Id,
        BudgetPlan = user3Plan,
        WishlistItemId = user3Wishlist.Id,
        WishlistItem = user3Wishlist,
        Title = "Air Fryer Pro 5L",
        Amount = 229.00m,
        Currency = "EUR",
        PurchasedAt = DateTime.UtcNow.AddDays(-2),
        MoodBeforePurchase = "Tired after work",
        NeedLevel = 7,
        TriggerType = ImpulseTriggerType.Recommendation,
        Installments = 3,
        Notes = "Kupnja nakon email preporuke i ograničene zalihe",
        Tags = new List<Tag> { stressTag, saleTag }
    };

    user1.BudgetPlans.Add(user1Plan);
    user1.Sessions.Add(user1Session);
    user1.WishlistItems.Add(user1Wishlist);
    user1.Purchases.Add(user1Purchase);

    user2.BudgetPlans.Add(user2Plan);
    user2.Sessions.Add(user2Session);
    user2.WishlistItems.Add(user2Wishlist);
    user2.Purchases.Add(user2Purchase);

    user3.BudgetPlans.Add(user3Plan);
    user3.Sessions.Add(user3Session);
    user3.WishlistItems.Add(user3Wishlist);
    user3.Purchases.Add(user3Purchase);

    user1Session.Purchases.Add(user1Purchase);
    user2Session.Purchases.Add(user2Purchase);
    user3Session.Purchases.Add(user3Purchase);

    user1Plan.CoveredPurchases.Add(user1Purchase);
    user2Plan.CoveredPurchases.Add(user2Purchase);
    user3Plan.CoveredPurchases.Add(user3Purchase);

    user1Wishlist.ConvertedPurchase = user1Purchase;
    user2Wishlist.ConvertedPurchase = user2Purchase;
    user3Wishlist.ConvertedPurchase = user3Purchase;

    techStore.Purchases.Add(user1Purchase);
    fashionStore.Purchases.Add(user2Purchase);
    homeStore.Purchases.Add(user3Purchase);

    stressTag.Purchases.AddRange(new[] { user1Purchase, user3Purchase });
    saleTag.Purchases.AddRange(new[] { user2Purchase, user3Purchase });
    socialTag.Purchases.Add(user1Purchase);

    return new List<UserProfile> { user1, user2, user3 };
}
