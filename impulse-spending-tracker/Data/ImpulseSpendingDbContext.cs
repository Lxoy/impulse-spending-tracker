using impulse_spending_tracker.Models;
using Microsoft.EntityFrameworkCore;

namespace impulse_spending_tracker.Data
{
    public class ImpulseSpendingDbContext : DbContext
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
        public DbSet<Tag> Tags => Set<Tag>();

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
                .HasMany(p => p.Tags)
                .WithMany(t => t.Purchases)
                .UsingEntity("PurchaseTags");
        }
    }
}