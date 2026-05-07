namespace CourseWork.Models;

public class ArticleItem
{
    public int Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    public override string ToString() => $"{Number} - {Description}";
}

public class PostItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    
    public override string ToString() => Title;
}

public class StructureItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Settlement { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    public override string ToString() => $"{Name} ({Settlement})";
}