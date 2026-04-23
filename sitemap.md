# Sitemap (semanticki model usmjeravanja)

## Napomena
Aplikacija koristi kombinaciju:
- attribute routing (`app.MapControllers()`)
- konvencionalni default routing (`{controller=Home}/{action=Index}/{id?}`)

Zbog toga su neke akcije dostupne preko vise URL-ova.

## URL -> Controller -> Akcija -> View

| URL | Controller | Akcija | View | Tip rute |
| --- | --- | --- | --- | --- |
| `/` | HomeController | Index | Views/Home/Index.cshtml | Attribute |
| `/error` | HomeController | Error | Views/Shared/Error.cshtml | Attribute |
| `/users` | UserProfilesController | Index | Views/UserProfiles/Index.cshtml | Attribute |
| `/users/{id:int}` | UserProfilesController | Details | Views/UserProfiles/Details.cshtml | Attribute |
| `/purchases-log` | PurchasesController | Index | Views/Purchases/Index.cshtml | Attribute |
| `/purchases-log/{id:int}` | PurchasesController | Details | Views/Purchases/Details.cshtml | Attribute |
| `/budget-plans` | BudgetPlansController | Index | Views/BudgetPlans/Index.cshtml | Attribute |
| `/budget-plans/{id:int}` | BudgetPlansController | Details | Views/BudgetPlans/Details.cshtml | Attribute |
| `/merchants` | MerchantsController | Index | Views/Merchants/Index.cshtml | Attribute |
| `/merchants/{id:int}` | MerchantsController | Details | Views/Merchants/Details.cshtml | Attribute |
| `/sessions` | SpendingSessionsController | Index | Views/SpendingSessions/Index.cshtml | Attribute |
| `/sessions/{id:int}` | SpendingSessionsController | Details | Views/SpendingSessions/Details.cshtml | Attribute |
| `/tags` | TagsController | Index | Views/Tags/Index.cshtml | Attribute |
| `/tags/{id:int}` | TagsController | Details | Views/Tags/Details.cshtml | Attribute |
| `/wishlist` | WishlistItemsController | Index | Views/WishlistItems/Index.cshtml | Attribute |
| `/wishlist/{id:int}` | WishlistItemsController | Details | Views/WishlistItems/Details.cshtml | Attribute |

## Dodatno dostupni URL-ovi preko default rute

| URL | Controller | Akcija | View | Tip rute |
| --- | --- | --- | --- | --- |
| `/Home/Index` | HomeController | Index | Views/Home/Index.cshtml | Default |
| `/Home/Error` | HomeController | Error | Views/Shared/Error.cshtml | Default |
| `/UserProfiles/Index` | UserProfilesController | Index | Views/UserProfiles/Index.cshtml | Default |
| `/UserProfiles/Details/{id}` | UserProfilesController | Details | Views/UserProfiles/Details.cshtml | Default |
| `/Purchases/Index` | PurchasesController | Index | Views/Purchases/Index.cshtml | Default |
| `/Purchases/Details/{id}` | PurchasesController | Details | Views/Purchases/Details.cshtml | Default |
| `/BudgetPlans/Index` | BudgetPlansController | Index | Views/BudgetPlans/Index.cshtml | Default |
| `/BudgetPlans/Details/{id}` | BudgetPlansController | Details | Views/BudgetPlans/Details.cshtml | Default |
| `/Merchants/Index` | MerchantsController | Index | Views/Merchants/Index.cshtml | Default |
| `/Merchants/Details/{id}` | MerchantsController | Details | Views/Merchants/Details.cshtml | Default |
| `/SpendingSessions/Index` | SpendingSessionsController | Index | Views/SpendingSessions/Index.cshtml | Default |
| `/SpendingSessions/Details/{id}` | SpendingSessionsController | Details | Views/SpendingSessions/Details.cshtml | Default |
| `/Tags/Index` | TagsController | Index | Views/Tags/Index.cshtml | Default |
| `/Tags/Details/{id}` | TagsController | Details | Views/Tags/Details.cshtml | Default |
| `/WishlistItems/Index` | WishlistItemsController | Index | Views/WishlistItems/Index.cshtml | Default |
| `/WishlistItems/Details/{id}` | WishlistItemsController | Details | Views/WishlistItems/Details.cshtml | Default |
