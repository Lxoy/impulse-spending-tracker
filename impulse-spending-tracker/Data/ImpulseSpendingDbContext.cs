using impulse_spending_tracker.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace impulse_spending_tracker.Data
{
    public class ImpulseSpendingDbContext : IdentityDbContext<AppUser>
    {
        public ImpulseSpendingDbContext(DbContextOptions<ImpulseSpendingDbContext> options)
            : base(options)
        {
        }

        public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
        public DbSet<Purchase> Purchases => Set<Purchase>();
        public DbSet<Merchant> Merchants => Set<Merchant>();
        public DbSet<SpendingSession> SpendingSessions => Set<SpendingSession>();
        public DbSet<BudgetPlan> BudgetPlans => Set<BudgetPlan>();
        public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();
        public DbSet<PurchaseAttachment> PurchaseAttachments => Set<PurchaseAttachment>();
        public DbSet<TriggerType> TriggerTypes => Set<TriggerType>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Purchase>()
                .HasOne(p => p.UserProfile)
                .WithMany(u => u.Purchases)
                .HasForeignKey(p => p.UserProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Purchase>()
                .HasOne(p => p.Merchant)
                .WithMany(m => m.Purchases)
                .HasForeignKey(p => p.MerchantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Purchase>()
                .HasOne(p => p.SpendingSession)
                .WithMany(s => s.Purchases)
                .HasForeignKey(p => p.SpendingSessionId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Purchase>()
                .HasOne(p => p.BudgetPlan)
                .WithMany(b => b.CoveredPurchases)
                .HasForeignKey(p => p.BudgetPlanId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Purchase>()
                .HasOne(p => p.WishlistItem)
                .WithOne(w => w.ConvertedPurchase)
                .HasForeignKey<Purchase>(p => p.WishlistItemId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Purchase>()
                .HasOne(p => p.TriggerTypeTag)
                .WithMany()
                .HasForeignKey(p => p.TriggerTypeId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<SpendingSession>()
                .HasOne(s => s.UserProfile)
                .WithMany(u => u.Sessions)
                .HasForeignKey(s => s.UserProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BudgetPlan>()
                .HasOne(b => b.UserProfile)
                .WithMany(u => u.BudgetPlans)
                .HasForeignKey(b => b.UserProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WishlistItem>()
                .HasOne(w => w.UserProfile)
                .WithMany(u => u.WishlistItems)
                .HasForeignKey(w => w.UserProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Purchase>()
                .HasMany(p => p.TriggerTypes)
                .WithMany(t => t.Purchases)
                .UsingEntity("PurchaseTags");

            modelBuilder.Entity<PurchaseAttachment>()
                .HasOne(a => a.Purchase)
                .WithMany(p => p.Attachments)
                .HasForeignKey(a => a.PurchaseId)
                .OnDelete(DeleteBehavior.Cascade);

            // Link UserProfile to Identity AppUser (1:1)
            modelBuilder.Entity<UserProfile>()
                .HasOne(up => up.AppUser)
                .WithOne(u => u.UserProfile)
                .HasForeignKey<UserProfile>(up => up.AppUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Global query filters to hide soft-deleted records
            modelBuilder.Entity<UserProfile>().HasQueryFilter(u => !u.IsDeleted);
            modelBuilder.Entity<Purchase>().HasQueryFilter(p => !p.IsDeleted);
            modelBuilder.Entity<Merchant>().HasQueryFilter(m => !m.IsDeleted);
            modelBuilder.Entity<SpendingSession>().HasQueryFilter(s => !s.IsDeleted);
            modelBuilder.Entity<BudgetPlan>().HasQueryFilter(b => !b.IsDeleted);
            modelBuilder.Entity<WishlistItem>().HasQueryFilter(w => !w.IsDeleted);
            modelBuilder.Entity<TriggerType>().HasQueryFilter(t => !t.IsDeleted);
        }
    }
}