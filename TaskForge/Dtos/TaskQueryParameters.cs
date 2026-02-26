namespace TaskForge.Dtos;

public class TaskQueryParameters
{
    public string? Search { get; set; }
    public bool? IsComplete { get; set; }
    public int? AfterId { get; set; }
    public int Limit { get; set; } = 20;
    public string? SortBy { get; set; }
    public bool Description { get; set; }
}