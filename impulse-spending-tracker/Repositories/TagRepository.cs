using impulse_spending_tracker.Data;
using impulse_spending_tracker.Models;
using Microsoft.EntityFrameworkCore;

namespace impulse_spending_tracker.Repositories
{
    public class TriggerTypeRepository
    {
        private readonly ImpulseSpendingDbContext _dbContext;

        public TriggerTypeRepository(ImpulseSpendingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IReadOnlyList<TriggerType> GetAll()
        {
            var triggerTypes = _dbContext.TriggerTypes
                .AsNoTracking()
                .ToList();

            PopulatePurchasesFromPrimaryTrigger(triggerTypes);
            return triggerTypes;
        }

        public TriggerType? GetById(int id)
        {
            var triggerType = _dbContext.TriggerTypes
                .AsNoTracking()
                .SingleOrDefault(t => t.Id == id);

            if (triggerType is not null)
            {
                PopulatePurchasesFromPrimaryTrigger(new[] { triggerType });
            }

            return triggerType;
        }

        private void PopulatePurchasesFromPrimaryTrigger(IEnumerable<TriggerType> triggerTypes)
        {
            var triggerTypeList = triggerTypes.ToList();
            var triggerTypeIds = triggerTypeList.Select(t => t.Id).ToList();
            if (triggerTypeIds.Count == 0)
            {
                return;
            }

            var purchasesByTriggerType = _dbContext.Purchases
                .AsNoTracking()
                .Where(p => p.TriggerTypeId.HasValue && triggerTypeIds.Contains(p.TriggerTypeId.Value))
                .ToList()
                .GroupBy(p => p.TriggerTypeId!.Value)
                .ToDictionary(g => g.Key, g => (ICollection<Purchase>)g.ToList());

            foreach (var triggerType in triggerTypeList)
            {
                triggerType.Purchases = purchasesByTriggerType.TryGetValue(triggerType.Id, out var purchases)
                    ? purchases
                    : new List<Purchase>();
            }
        }

        public void Create(TriggerType tag)
        {
            _dbContext.TriggerTypes.Add(tag);
            _dbContext.SaveChanges();
        }

        public void Update(TriggerType tag)
        {
            _dbContext.TriggerTypes.Update(tag);
            _dbContext.SaveChanges();
        }

        public void Delete(TriggerType tag)
        {
            tag.IsDeleted = true;
            _dbContext.TriggerTypes.Update(tag);
            _dbContext.SaveChanges();
        }
    }
}
