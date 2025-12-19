using Application.Common.Interfaces;
using Infrastructure.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

 namespace Api.Controllers;

//[ApiController]
//[Route("api/files")]
 
    public class HomeController : Controller
    {
    //    private readonly FileStorageService _storage;
    //    private readonly IAppDbContext _db;

    public HomeController(FileStorageService storage, IAppDbContext db)
    {
        //        _storage = storage;
        //        _db = db;
    }
}

//    //file upload method//user,admin

//    [HttpPost]
//    [Authorize(Roles = "user,admin")]
//    public async Task<IActionResult> Upload([FromQuery] string? tags, CancellationToken ct)
//    {
//        if (!Request.HasFormContentType || Request.Form.Files.Count == 0)
//            return Problem("No file provided", statusCode: 400);

//        var file = Request.Form.Files[0];
//        var originalName = Path.GetFileName(file.FileName);
//        var contentType = string.IsNullOrWhiteSpace(file.ContentType)
//            ? "application/octet-stream"
//            : file.ContentType;

//        var userId = User?.Identity?.Name ?? "mock-user";

//        await using var uploadStream = file.OpenReadStream();

//        var entity = await _storage.SaveAsync(
//            uploadStream,
//            originalName,
//            contentType,
//            tags,
//            userId,
//            ct
//        );

//        return Ok(entity);
//    }
//    //preview method
//    [HttpGet("{id:guid}/preview")]
//    [Authorize(Roles = "user,admin")]
//    public async Task<IActionResult> Preview(Guid id, CancellationToken ct)
//    {
//        var (stream, length, meta) = await _storage.OpenForDownloadAsync(id, ct);

//        // Force inline preview
//        Response.Headers["Content-Disposition"] = "inline";

//        var rangeHeader = Request.Headers["Range"].ToString();

//        if (string.IsNullOrEmpty(rangeHeader))
//        {
//            // NO FILENAME → browser previews
//            return File(stream, meta.ContentType);
//        }

//        if (!rangeHeader.StartsWith("bytes="))
//            return BadRequest("Invalid Range header");

//        var range = rangeHeader.Replace("bytes=", "").Split('-');

//        if (!long.TryParse(range[0], out long start))
//            start = 0;

//        long end = length - 1;

//        if (range.Length > 1 && long.TryParse(range[1], out long parsedEnd))
//            end = parsedEnd;

//        if (start >= length || end >= length)
//            return StatusCode(416);

//        var bytesToRead = end - start + 1;

//        stream.Seek(start, SeekOrigin.Begin);

//        Response.StatusCode = 206;
//        Response.Headers["Content-Range"] = $"bytes {start}-{end}/{length}";
//        Response.Headers["Accept-Ranges"] = "bytes";
//        Response.Headers["Content-Length"] = bytesToRead.ToString();

//        // NO FILENAME → preview works
//        return File(stream, meta.ContentType);

//    }
//    [Authorize(Policy = "UserOrAdmin")]
//    [HttpGet("{id}/download")]
//    public async Task<IActionResult> Download(Guid id, CancellationToken ct)
//    {
//        var obj = await _db.StoredObjects
//            .FirstOrDefaultAsync(x => x.Id == id && x.DeletedAtUtc == null, ct);

//        if (obj == null)
//            return NotFound();

//        var filePath = obj.Path;

//        if (!System.IO.File.Exists(filePath))
//            return NotFound();

//        var fileLength = new FileInfo(filePath).Length;
//        var rangeHeader = Request.Headers["Range"].ToString();

//        // ------------------------------------------------------------
//        // NO RANGE → return full file
//        // ------------------------------------------------------------
//        if (string.IsNullOrEmpty(rangeHeader))
//        {
//            var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
//            return File(fs, obj.ContentType, enableRangeProcessing: true);
//        }

//        // ------------------------------------------------------------
//        // RANGE REQUEST → parse bytes
//        // ------------------------------------------------------------
//        // Example: "bytes=0-1023"
//        if (!rangeHeader.StartsWith("bytes="))
//            return BadRequest("Invalid Range header");

//        var range = rangeHeader.Replace("bytes=", "").Split('-');

//        if (!long.TryParse(range[0], out long start))
//            start = 0;

//        long end = fileLength - 1;

//        if (range.Length > 1 && long.TryParse(range[1], out long parsedEnd))
//            end = parsedEnd;

//        if (start >= fileLength || end >= fileLength)
//            return StatusCode(416); // Range Not Satisfiable

//        var length = end - start + 1;

//        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
//        stream.Seek(start, SeekOrigin.Begin);

//        Response.StatusCode = 206; // Partial Content
//        Response.Headers["Content-Range"] = $"bytes {start}-{end}/{fileLength}";
//        Response.Headers["Accept-Ranges"] = "bytes";
//        Response.Headers["Content-Length"] = length.ToString();

//        return File(stream, obj.ContentType, enableRangeProcessing: false);
//    }

//    // ---------------------------------------------------------
//    // 4) GET ALL (simple list)
//    // ---------------------------------------------------------
//    [HttpGet("all")]
//    [Authorize(Roles = "user,admin")]
//    public async Task<IActionResult> GetAll(CancellationToken ct)
//    {
//        var items = await _db.StoredObjects
//            .Where(f => f.DeletedAtUtc == null)
//            .OrderByDescending(f => f.CreatedAtUtc)
//            .Select(f => new
//            {
//                f.Id,
//                Name = f.OriginalName,
//                f.ContentType,
//                Size = f.SizeBytes,
//                CreatedAt = f.CreatedAtUtc
//            })
//            .ToListAsync(ct);

//        return Ok(items);
//    }
//    //getpaged method
//    [HttpGet("paged")]
//    [Authorize(Roles = "user,admin")]
//    public async Task<IActionResult> GetPaged(
//       [FromQuery] int page = 1,
//       [FromQuery] int pageSize = 10,
//       CancellationToken ct = default)
//    {
//        if (page < 1) page = 1;
//        if (pageSize < 1) pageSize = 10;

//        var skip = (page - 1) * pageSize;

//        var query = _db.StoredObjects
//            .Where(f => f.DeletedAtUtc == null);

//        var totalCount = await query.CountAsync(ct);
//        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

//        var items = await query
//            .OrderByDescending(f => f.CreatedAtUtc)
//            .Skip(skip)
//            .Take(pageSize)
//            .Select(f => new
//            {
//                f.Id,
//                Name = f.OriginalName,
//                f.ContentType,
//                Size = f.SizeBytes,
//                CreatedAt = f.CreatedAtUtc
//            })
//            .ToListAsync(ct);

//        return Ok(new
//        {
//            page,
//            pageSize,
//            totalCount,
//            totalPages,
//            items
//        });
//    }
//    // soft delete
//    [HttpDelete("{id:guid}")]
//    [Authorize(Roles = "admin")]
//    public async Task<IActionResult> SoftDelete(Guid id, CancellationToken ct)
//    {
//        await _storage.SoftDeleteAsync(id, ct);
//        return Ok(new { Message = "Soft deleted" });
//    }
//    //hard delete
//    [HttpDelete("{id:guid}/hard")]
//    [Authorize(Roles = "admin")]
//    public async Task<IActionResult> HardDelete(Guid id, CancellationToken ct)
//    {
//        await _storage.HardDeleteAsync(id, ct);
//        return Ok(new { Message = "Hard deleted" });
//    }
//    // list files method
//    [HttpGet]
//    [Authorize(Roles = "user,admin")]
//    public async Task<IActionResult> List(
//      int page = 1,
//      int pageSize = 20,
//      string? search = null,
//      string? tag = null,
//      string? type = null,
//      CancellationToken ct = default)
//    {
//        if (page < 1) page = 1;
//        if (pageSize < 1) pageSize = 20;

//        // Base query
//        var query = _db.StoredObjects
//            .Where(x => x.DeletedAtUtc == null);

//        // ------------------------------------------------------------
//        // SEARCH
//        // ------------------------------------------------------------
//        if (!string.IsNullOrWhiteSpace(search))
//        {
//            query = query.Where(x =>
//                x.OriginalName.Contains(search) ||
//                x.Key.Contains(search) ||
//                (x.Tags != null && x.Tags.Contains(search)) ||
//                x.ContentType.Contains(search)
//            );
//        }

//        if (!string.IsNullOrWhiteSpace(tag))
//        {
//            query = query.Where(x => x.Tags != null && x.Tags.Contains(tag));
//        }

//        if (!string.IsNullOrWhiteSpace(type))
//        {
//            query = query.Where(x => x.ContentType.Contains(type));
//        }

//        // ------------------------------------------------------------
//        // SORTING (default: latest first)
//        // ------------------------------------------------------------
//        query = query.OrderByDescending(x => x.CreatedAtUtc);

//        // ------------------------------------------------------------
//        // TOTAL COUNT AFTER SEARCH
//        // ------------------------------------------------------------
//        var total = await query.CountAsync(ct);

//        // ------------------------------------------------------------
//        // PAGINATION
//        // ------------------------------------------------------------
//        var items = await query
//            .Skip((page - 1) * pageSize)
//            .Take(pageSize)
//            .Select(x => new
//            {
//                x.Id,
//                x.OriginalName,
//                x.Key,
//                x.SizeBytes,
//                x.ContentType,
//                x.Checksum,
//                x.Tags,
//                x.CreatedAtUtc,
//                x.DeletedAtUtc,
//                x.Version,
//                x.CreatedByUserId
//            })
//            .ToListAsync(ct);

//        return Ok(new
//        {
//            page,
//            pageSize,
//            total,
//            items
//        });
//    }

//}

