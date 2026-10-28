namespace His.Api.Dtos;

/// <summary>
/// 通用分页查询请求。Page 从 1 开始，Size 限制在 1~100。
/// </summary>
public class PageRequest
{
    private int _page = 1;
    private int _size = 10;

    public int Page
    {
        get => _page;
        set => _page = Math.Max(value, 1);
    }

    public int Size
    {
        get => _size;
        set => _size = Math.Clamp(value, 1, 100);
    }

    public string? Keyword { get; set; }

    public int Skip => (Page - 1) * Size;
}

/// <summary>
/// 通用分页查询结果。
/// </summary>
public class PageResult<T>
{
    public long Total { get; set; }
    public IReadOnlyList<T> Items { get; set; }

    public PageResult(long total, IReadOnlyList<T> items)
    {
        Total = total;
        Items = items;
    }

    public static PageResult<T> Empty => new(0, Array.Empty<T>());
}
