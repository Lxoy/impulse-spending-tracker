# Semanticki DB model

## Pregled entiteta i tablica

Aplikacija koristi sljedece EF entitete (tablice):
- UserProfile
- Purchase
- Merchant
- SpendingSession
- BudgetPlan
- WishlistItem
- Tag
- PurchaseTags (join tablica za N-N vezu Purchase-Tag)

## Entiteti i glavna svojstva

### UserProfile
- Id (PK)
- FirstName
- LastName
- Email
- DateOfBirth
- MonthlyNetIncome
- RiskToleranceScore
- CreatedAt

### Purchase
- Id (PK)
- UserProfileId (FK -> UserProfile)
- MerchantId (FK -> Merchant)
- SpendingSessionId (FK, nullable -> SpendingSession)
- BudgetPlanId (FK, nullable -> BudgetPlan)
- WishlistItemId (FK, nullable, unique -> WishlistItem)
- Title
- Amount
- Currency
- PurchasedAt
- MoodBeforePurchase
- NeedLevel
- TriggerType
- Installments
- Notes (nullable)

### Merchant
- Id (PK)
- Name
- Category
- CountryCode
- IsOnlineOnly
- AverageDeliveryDays (nullable)

### SpendingSession
- Id (PK)
- UserProfileId (FK -> UserProfile)
- StartedAt
- EndedAt (nullable)
- Platform
- Channel
- SessionBudget
- SpentAmount
- ItemsViewed
- ItemsAddedToCart
- CheckoutCompleted

### BudgetPlan
- Id (PK)
- UserProfileId (FK -> UserProfile)
- Name
- ValidFrom
- ValidTo
- MonthlyLimit
- ImpulseCapPercentage
- EssentialCategoryLimit
- DiscretionaryCategoryLimit
- IsActive

### WishlistItem
- Id (PK)
- UserProfileId (FK -> UserProfile)
- Name
- DesiredPrice
- CurrentPrice
- Priority
- AddedAt
- TargetPurchaseDate (nullable)
- Reason
- IsPurchased
- LinkUrl

### Tag
- Id (PK)
- Name
- ColorHex
- Description

## Veze medu tablicama

### 1-N veze
- UserProfile (1) -> Purchase (N)
  - FK: Purchase.UserProfileId
  - OnDelete: Restrict

- Merchant (1) -> Purchase (N)
  - FK: Purchase.MerchantId
  - OnDelete: Restrict

- SpendingSession (1) -> Purchase (N)
  - FK: Purchase.SpendingSessionId (nullable)
  - OnDelete: SetNull

- BudgetPlan (1) -> Purchase (N)
  - FK: Purchase.BudgetPlanId (nullable)
  - OnDelete: SetNull

- UserProfile (1) -> SpendingSession (N)
  - FK: SpendingSession.UserProfileId
  - OnDelete: Cascade

- UserProfile (1) -> BudgetPlan (N)
  - FK: BudgetPlan.UserProfileId
  - OnDelete: Cascade

- UserProfile (1) -> WishlistItem (N)
  - FK: WishlistItem.UserProfileId
  - OnDelete: Cascade

### 1-1 veza
- WishlistItem (1) <-> Purchase (0..1)
  - FK je na Purchase.WishlistItemId (nullable, unique)
  - OnDelete: SetNull

### N-N veza
- Purchase (N) <-> Tag (N)
  - join tablica: PurchaseTags
  - kljucevi: PurchasesId, TagsId

## Napomena o modeliranju
- Routanje i UI sloj su odvojeni od semantickog DB modela.
- TriggerType je enum u domeni (pohranjuje se kao numericka vrijednost u tablici Purchase).
