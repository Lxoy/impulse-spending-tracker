namespace impulse_spending_tracker.Models
{
    public class GlobalSearchResult
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsMenuItem { get; set; }
        public string Controller { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
    }
}
