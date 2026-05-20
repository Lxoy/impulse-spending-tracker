using System.Collections.Generic;

namespace impulse_spending_tracker.Models
{
    public class ImpulsePulseViewModel
    {
        public class PulseItem
        {
            public string Label { get; set; } = string.Empty;
            public ImpulseTriggerType TriggerType { get; set; }
            public decimal Amount { get; set; }
            public int Count { get; set; }
            public double Percent { get; set; }
        }

        public List<PulseItem> Items { get; set; } = new List<PulseItem>();
        public decimal TotalAmount { get; set; }
        public string MonthlySpendCurrency { get; set; } = "EUR";
        public int TotalUsers { get; set; }
        public int ImpulsePurchasesCount { get; set; }
        public int ImpulsePurchasesThisWeek { get; set; }
        public int WishlistConvertedCount { get; set; }
        public double WishlistConversionRate { get; set; }
        public double AverageNeedLevel { get; set; }
        public double AvgRiskScore { get; set; }
        public string MonthlySpendTrendText { get; set; } = string.Empty;
        public string WishlistConversionText { get; set; } = string.Empty;
        public string AverageNeedLevelText { get; set; } = string.Empty;
        public string TopTriggerLabel { get; set; } = string.Empty;
    }
}
