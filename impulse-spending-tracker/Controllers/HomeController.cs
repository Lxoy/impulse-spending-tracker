using System;
using System.Diagnostics;
using System.Linq;
using impulse_spending_tracker.Data;
using impulse_spending_tracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace impulse_spending_tracker.Controllers
{
    [Route("")]
    public class HomeController : Controller
    {
        private readonly ImpulseSpendingDbContext _db;

        public HomeController(ImpulseSpendingDbContext db)
        {
            _db = db;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            var now = DateTime.Now;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var startOfPreviousMonth = startOfMonth.AddMonths(-1);
            var startOfLastSevenDays = now.AddDays(-7);

            var purchases = _db.Purchases
                .Include(p => p.TriggerTypeTag)
                .Where(p => p.PurchasedAt >= startOfMonth && p.PurchasedAt <= now);
            var monthPurchases = purchases.ToList();

            var totalAmount = monthPurchases.Sum(p => p.Amount);
            var previousMonthAmount = _db.Purchases
                .Where(p => p.PurchasedAt >= startOfPreviousMonth && p.PurchasedAt < startOfMonth)
                .Sum(p => (decimal?)p.Amount) ?? 0m;
            var impulsePurchasesThisWeek = _db.Purchases.Count(p => p.PurchasedAt >= startOfLastSevenDays && p.PurchasedAt <= now);
            var wishlistConvertedCount = _db.WishlistItems.Count(w => w.ConvertedPurchase != null);
            var wishlistTotalCount = _db.WishlistItems.Count();
            var averageNeedLevel = monthPurchases.Any() ? monthPurchases.Average(p => (double)p.NeedLevel) : 0.0;
            var monthlySpendChangePercent = previousMonthAmount > 0m
                ? ((totalAmount - previousMonthAmount) / previousMonthAmount) * 100.0m
                : 0.0m;
            var monthlySpendTrendText = previousMonthAmount > 0m
                ? $"{(monthlySpendChangePercent >= 0 ? "+" : "-")}{Math.Abs(monthlySpendChangePercent):0}% vs previous cycle"
                : "No previous cycle data";
            var wishlistConversionRate = wishlistTotalCount > 0
                ? wishlistConvertedCount * 100.0 / wishlistTotalCount
                : 0.0;
            var averageNeedLevelText = monthPurchases.Any() && averageNeedLevel <= 6.5
                ? "Monitor low-need purchases"
                : monthPurchases.Any()
                    ? "Need levels look balanced"
                    : "No purchase data yet";

            var grouped = purchases
                .GroupBy(p => p.TriggerTypeTag != null ? p.TriggerTypeTag.Name : p.TriggerType.ToString())
                .Select(g => new { Trigger = g.Key, Amount = g.Sum(p => p.Amount), Count = g.Count() })
                .OrderByDescending(x => x.Amount)
                .ToList();

            var vm = new ImpulsePulseViewModel
            {
                TotalAmount = totalAmount,
                MonthlySpendCurrency = monthPurchases.FirstOrDefault()?.Currency ?? "EUR",
                ImpulsePurchasesThisWeek = impulsePurchasesThisWeek,
                WishlistConvertedCount = wishlistConvertedCount,
                WishlistConversionRate = wishlistConversionRate,
                AverageNeedLevel = averageNeedLevel,
                MonthlySpendTrendText = monthlySpendTrendText,
                WishlistConversionText = $"{wishlistConversionRate:0}% conversion",
                AverageNeedLevelText = averageNeedLevelText
            };

            vm.TotalUsers = _db.UserProfiles.Count();
            vm.ImpulsePurchasesCount = monthPurchases.Count;
            vm.AvgRiskScore = _db.UserProfiles.Any() ? Math.Round(_db.UserProfiles.Average(u => (double)u.RiskToleranceScore), 1) : 0.0;

            foreach (var g in grouped)
            {
                vm.Items.Add(new ImpulsePulseViewModel.PulseItem
                {
                    TriggerType = ImpulseTriggerType.Other,
                    Label = g.Trigger.ToString(),
                    Amount = g.Amount,
                    Count = g.Count,
                    Percent = totalAmount > 0 ? (double)(g.Amount / totalAmount * 100.0m) : 0.0
                });
            }

            vm.TopTriggerLabel = vm.Items.FirstOrDefault()?.Label ?? "—";

            return View(vm);
        }

        [HttpGet("trigger-heatmap-data")]
        public IActionResult TriggerHeatmapData()
        {
            var now = DateTime.Now.Date.AddDays(1).AddTicks(-1);
            var start = now.AddDays(-29).Date; // last 30 days inclusive

            var data = _db.Purchases
                .Where(p => p.PurchasedAt >= start && p.PurchasedAt <= now)
                .GroupBy(p => p.PurchasedAt.Date)
                .Select(g => new { Day = g.Key, Count = g.Count() })
                .ToList()
                .Select(x => new { day = x.Day.ToString("yyyy-MM-dd"), count = x.Count })
                .ToList();

            // ensure all 30 days present (zero for missing)
            var result = new List<object>();
            for (int i = 0; i < 30; i++)
            {
                var d = start.AddDays(i);
                var found = data.FirstOrDefault(x => x.day == d.ToString("yyyy-MM-dd"));
                result.Add(new { day = d.ToString("yyyy-MM-dd"), count = found?.count ?? 0 });
            }

            return Json(result);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [HttpGet("error")]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
