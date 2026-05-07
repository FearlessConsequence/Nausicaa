namespace CourseWork.Models;

public class SearchResultItem
{
    public string Category { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    public override string ToString()
    {
        return string.IsNullOrEmpty(Description) ? $"[{Category}] {Title}" : $"[{Category}] {Title} - {Description}";
    }
}