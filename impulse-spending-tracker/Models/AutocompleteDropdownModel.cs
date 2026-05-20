namespace impulse_spending_tracker.Models
{
    public class AutocompleteDropdownModel
    {
        public string InputName { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Placeholder { get; set; } = "Type to search...";
        public string SearchUrl { get; set; } = string.Empty;
        public string EmptyMessage { get; set; } = "No matching results.";
        public string? ValidationMessageFor { get; set; }
        public int? SelectedId { get; set; }
        public string? SelectedText { get; set; }
    }
}
