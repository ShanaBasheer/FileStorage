using System;

namespace Application.Files;

public class ListFilesQuery
{
    public string? Name { get; set; }
    public string? Tag { get; set; }
    public string? ContentType { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
