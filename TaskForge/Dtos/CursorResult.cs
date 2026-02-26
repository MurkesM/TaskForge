namespace TaskForge.Dtos;

public class CursorResult<T>
{
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
    public int? NextCursor { get; set; }
    public bool HasMore { get; set; }

}