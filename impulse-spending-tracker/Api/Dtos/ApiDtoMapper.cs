using impulse_spending_tracker.Models;

namespace impulse_spending_tracker.Api
{
    public static class ApiDtoMapper
    {
        public static UserProfileSummaryDto ToSummary(this UserProfile userProfile)
        {
            return new UserProfileSummaryDto
            {
                Id = userProfile.Id,
                FirstName = userProfile.FirstName,
                LastName = userProfile.LastName,
                Email = userProfile.Email
            };
        }

        public static UserProfileDto ToDto(this UserProfile userProfile)
        {
            return new UserProfileDto
            {
                Id = userProfile.Id,
                FirstName = userProfile.FirstName,
                LastName = userProfile.LastName,
                Email = userProfile.Email,
                DateOfBirth = userProfile.DateOfBirth,
                MonthlyNetIncome = userProfile.MonthlyNetIncome,
                RiskToleranceScore = userProfile.RiskToleranceScore
            };
        }

        public static MerchantSummaryDto ToSummary(this Merchant merchant)
        {
            return new MerchantSummaryDto
            {
                Id = merchant.Id,
                Name = merchant.Name,
                Category = merchant.Category
            };
        }

        public static MerchantDto ToDto(this Merchant merchant)
        {
            return new MerchantDto
            {
                Id = merchant.Id,
                Name = merchant.Name,
                Category = merchant.Category,
                CountryCode = merchant.CountryCode,
                IsOnlineOnly = merchant.IsOnlineOnly,
                AverageDeliveryDays = merchant.AverageDeliveryDays
            };
        }

        public static TriggerTypeSummaryDto ToSummary(this TriggerType tag)
        {
            return new TriggerTypeSummaryDto
            {
                Id = tag.Id,
                Name = tag.Name,
                ColorHex = tag.ColorHex
            };
        }

        public static TriggerTypeDto ToDto(this TriggerType tag)
        {
            return new TriggerTypeDto
            {
                Id = tag.Id,
                Name = tag.Name,
                ColorHex = tag.ColorHex,
                Description = tag.Description
            };
        }

        public static PurchaseSummaryDto ToSummary(this Purchase purchase)
        {
            return new PurchaseSummaryDto
            {
                Id = purchase.Id,
                Title = purchase.Title,
                Amount = purchase.Amount,
                Currency = purchase.Currency,
                PurchasedAt = purchase.PurchasedAt
            };
        }

        public static PurchaseDto ToDto(this Purchase purchase)
        {
            return new PurchaseDto
            {
                Id = purchase.Id,
                Title = purchase.Title,
                Amount = purchase.Amount,
                Currency = purchase.Currency,
                PurchasedAt = purchase.PurchasedAt,
                UserProfile = purchase.UserProfile?.ToSummary(),
                Merchant = purchase.Merchant?.ToSummary(),
                SpendingSession = purchase.SpendingSession?.ToSummary(),
                BudgetPlan = purchase.BudgetPlan?.ToSummary(),
                WishlistItem = purchase.WishlistItem?.ToSummary(),
                MoodBeforePurchase = purchase.MoodBeforePurchase,
                NeedLevel = purchase.NeedLevel,
                TriggerType = purchase.TriggerType,
                Installments = purchase.Installments,
                Notes = purchase.Notes,
                TriggerTypes = purchase.TriggerTypes.Select(tag => tag.ToSummary()).ToList()
            };
        }

        public static BudgetPlanSummaryDto ToSummary(this BudgetPlan budgetPlan)
        {
            return new BudgetPlanSummaryDto
            {
                Id = budgetPlan.Id,
                Name = budgetPlan.Name,
                MonthlyLimit = budgetPlan.MonthlyLimit,
                IsActive = budgetPlan.IsActive
            };
        }

        public static BudgetPlanDto ToDto(this BudgetPlan budgetPlan)
        {
            return new BudgetPlanDto
            {
                Id = budgetPlan.Id,
                Name = budgetPlan.Name,
                MonthlyLimit = budgetPlan.MonthlyLimit,
                IsActive = budgetPlan.IsActive,
                UserProfile = budgetPlan.UserProfile?.ToSummary(),
                ValidFrom = budgetPlan.ValidFrom,
                ValidTo = budgetPlan.ValidTo,
                ImpulseCapPercentage = budgetPlan.ImpulseCapPercentage,
                EssentialCategoryLimit = budgetPlan.EssentialCategoryLimit,
                DiscretionaryCategoryLimit = budgetPlan.DiscretionaryCategoryLimit
            };
        }

        public static SpendingSessionSummaryDto ToSummary(this SpendingSession session)
        {
            return new SpendingSessionSummaryDto
            {
                Id = session.Id,
                StartedAt = session.StartedAt,
                Platform = session.Platform,
                Channel = session.Channel
            };
        }

        public static SpendingSessionDto ToDto(this SpendingSession session)
        {
            return new SpendingSessionDto
            {
                Id = session.Id,
                StartedAt = session.StartedAt,
                Platform = session.Platform,
                Channel = session.Channel,
                UserProfile = session.UserProfile?.ToSummary(),
                EndedAt = session.EndedAt,
                SessionBudget = session.SessionBudget,
                SpentAmount = session.SpentAmount,
                ItemsViewed = session.ItemsViewed,
                ItemsAddedToCart = session.ItemsAddedToCart,
                CheckoutCompleted = session.CheckoutCompleted
            };
        }

        public static WishlistItemSummaryDto ToSummary(this WishlistItem wishlistItem)
        {
            return new WishlistItemSummaryDto
            {
                Id = wishlistItem.Id,
                Name = wishlistItem.Name,
                DesiredPrice = wishlistItem.DesiredPrice
            };
        }

        public static WishlistItemDto ToDto(this WishlistItem wishlistItem)
        {
            return new WishlistItemDto
            {
                Id = wishlistItem.Id,
                Name = wishlistItem.Name,
                DesiredPrice = wishlistItem.DesiredPrice,
                UserProfile = wishlistItem.UserProfile?.ToSummary(),
                CurrentPrice = wishlistItem.CurrentPrice,
                Priority = wishlistItem.Priority,
                AddedAt = wishlistItem.AddedAt,
                TargetPurchaseDate = wishlistItem.TargetPurchaseDate,
                Reason = wishlistItem.Reason,
                IsPurchased = wishlistItem.IsPurchased,
                LinkUrl = wishlistItem.LinkUrl,
                ConvertedPurchase = wishlistItem.ConvertedPurchase?.ToSummary()
            };
        }
    }
}