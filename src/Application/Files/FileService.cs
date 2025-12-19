using Application.Common.Interfaces;
using Domain.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Application.Files;

public class FileService
{
    private readonly IAppDbContext _db;

    public FileService(IAppDbContext db)
    {
        _db = db;
    }

    public (IReadOnlyList<StoredObject> Items, int Total) List(ListFilesQuery q)
    {
        var query = _db.StoredObjects.Where(x => x.DeletedAtUtc == null);

        if (!string.IsNullOrWhiteSpace(q.Name))
            query = query.Where(x => x.OriginalName.Contains(q.Name));

        if (!string.IsNullOrWhiteSpace(q.Tag))
            query = query.Where(x => x.Tags != null && x.Tags.Contains(q.Tag));

        if (!string.IsNullOrWhiteSpace(q.ContentType))
            query = query.Where(x => x.ContentType == q.ContentType);

        if (q.From.HasValue)
            query = query.Where(x => x.CreatedAtUtc >= q.From.Value);

        if (q.To.HasValue)
            query = query.Where(x => x.CreatedAtUtc <= q.To.Value);

        var total = query.Count();

        var items = query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .ToList();

        return (items, total);
    }
}
