namespace impulse_spending_tracker.Models
{
    public class GlobalSearchViewModel
    {
        public string Query { get; set; } = string.Empty;
        public List<GlobalSearchResult> Results { get; set; } = new();
    }
}
