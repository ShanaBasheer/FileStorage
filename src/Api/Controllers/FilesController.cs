using Application.Common.Interfaces;
using Infrastructure.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace Api.Controllers;

[ApiController]
[Route("api/files")]
public class FilesController : ControllerBase
{
    private readonly FileStorageService _storage;
    private readonly IAppDbContext _db;

    public FilesController(FileStorageService storage, IAppDbContext db)
    {
        _storage = storage;
        _db = db;
    }

    // 1) UPLOAD

    //[HttpPost("upload")]
    //public async Task<IActionResult> Upload(
    // [FromForm] IFormFile file,
    // [FromQuery] string? tags,
    // CancellationToken ct)
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(
    [FromForm] IFormFile file,
    [FromForm] string? tags,
    CancellationToken ct)

    {
        if (file == null || file.Length == 0)
        {
            return Problem(
                title: "Invalid file",
                detail: "No file was provided or file is empty.",
                statusCode: StatusCodes.Status400BadRequest,
                type: "https://httpstatuses.io/400");
        }

        try
        {
            // Read user ID from JWT (or fallback)
            var userId = User?.Identity?.Name ?? "system";

            // Call your existing storage service
            var result = await _storage.SaveAsync(
                input: file.OpenReadStream(),
                originalName: file.FileName,
                contentType: file.ContentType,
                tags: tags,
                createdByUserId: userId,
                ct: ct
            );

            return Ok(new
            {
                id = result.Id,
                originalName = result.OriginalName,
                sizeBytes = result.SizeBytes,
                contentType = result.ContentType,
                checksum = result.Checksum,
                tags = result.Tags,
                createdAtUtc = result.CreatedAtUtc
            });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("limit"))
        {
            return Problem(
                title: "File too large",
                detail: "The uploaded file exceeds the maximum allowed size.",
                statusCode: StatusCodes.Status413PayloadTooLarge,
                type: "https://httpstatuses.io/413");
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Unsupported"))
        {
            return Problem(
                title: "Unsupported file type",
                detail: "This file type is not allowed.",
                statusCode: StatusCodes.Status415UnsupportedMediaType,
                type: "https://httpstatuses.io/415");
        }
        catch (Exception ex)
        {
            return Problem(
                title: "Upload failed",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                type: "https://httpstatuses.io/500");
        }

    }
     
    // ---------------------------------------------------------
    // 2) PREVIEW (inline view)
    // ---------------------------------------------------------
    //[HttpGet("{id:guid}/preview")]
    //[Authorize(Roles = "user,admin")]
    //public async Task<IActionResult> Preview(Guid id, CancellationToken ct)
    //{
    //    var (stream, length, meta) = await _storage.OpenForDownloadAsync(id, ct);

    //    return File(stream, meta.ContentType);
    //}
    [HttpGet("{id:guid}/preview")]
    [Authorize(Roles = "user,admin")]
    public async Task<IActionResult> Preview(Guid id, CancellationToken ct)
    {
        var (stream, length, meta) = await _storage.OpenForDownloadAsync(id, ct);

        // Force inline preview
        Response.Headers["Content-Disposition"] = "inline";

        var rangeHeader = Request.Headers["Range"].ToString();

        if (string.IsNullOrEmpty(rangeHeader))
        {
            // NO FILENAME → browser previews update first upload 12-25
            return File(stream, meta.ContentType);
        }

        if (!rangeHeader.StartsWith("bytes="))
            return BadRequest("Invalid Range header");

        var range = rangeHeader.Replace("bytes=", "").Split('-');

        if (!long.TryParse(range[0], out long start))
            start = 0;

        long end = length - 1;

        if (range.Length > 1 && long.TryParse(range[1], out long parsedEnd))
            end = parsedEnd;

        if (start >= length || end >= length)
            return StatusCode(416);

        var bytesToRead = end - start + 1;

        stream.Seek(start, SeekOrigin.Begin);

        Response.StatusCode = 206;
        Response.Headers["Content-Range"] = $"bytes {start}-{end}/{length}";
        Response.Headers["Accept-Ranges"] = "bytes";
        Response.Headers["Content-Length"] = bytesToRead.ToString();

         
        return File(stream, meta.ContentType);
    }
 
    [Authorize(Policy = "UserOrAdmin")]
    [HttpGet("{id}/download")]
    public async Task<IActionResult> Download(Guid id, CancellationToken ct)
    {
        var obj = await _db.StoredObjects
            .FirstOrDefaultAsync(x => x.Id == id && x.DeletedAtUtc == null, ct);

        if (obj == null)
            return NotFound();

        var filePath = obj.Path;

        if (!System.IO.File.Exists(filePath))
            return NotFound();

        var fileLength = new FileInfo(filePath).Length;
        var rangeHeader = Request.Headers["Range"].ToString();
         
        if (string.IsNullOrEmpty(rangeHeader))
        {
            var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return File(fs, obj.ContentType, enableRangeProcessing: true);
        }

        if (!rangeHeader.StartsWith("bytes="))
            return BadRequest("Invalid Range header");

        var range = rangeHeader.Replace("bytes=", "").Split('-');

        if (!long.TryParse(range[0], out long start))
            start = 0;

        long end = fileLength - 1;

        if (range.Length > 1 && long.TryParse(range[1], out long parsedEnd))
            end = parsedEnd;

        if (start >= fileLength || end >= fileLength)
            return StatusCode(416); // Range Not Satisfiable

        var length = end - start + 1;

        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        stream.Seek(start, SeekOrigin.Begin);

        Response.StatusCode = 206; // Partial Content
        Response.Headers["Content-Range"] = $"bytes {start}-{end}/{fileLength}";
        Response.Headers["Accept-Ranges"] = "bytes";
        Response.Headers["Content-Length"] = length.ToString();

        return File(stream, obj.ContentType, enableRangeProcessing: false);
    }
  
     
    [HttpGet("all")]
    [Authorize(Roles = "user,admin")]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var items = await _db.StoredObjects
            .Where(f => f.DeletedAtUtc == null)
            .OrderByDescending(f => f.CreatedAtUtc)
            .Select(f => new
            {
                f.Id,
                Name = f.OriginalName,
                f.ContentType,
                Size = f.SizeBytes,
                CreatedAt = f.CreatedAtUtc
            })
            .ToListAsync(ct);

        return Ok(items);
    }

   
    [HttpGet("paged")]
    [Authorize(Roles = "user,admin")]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;

        var skip = (page - 1) * pageSize;

        var query = _db.StoredObjects
            .Where(f => f.DeletedAtUtc == null);

        var totalCount = await query.CountAsync(ct);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var items = await query
            .OrderByDescending(f => f.CreatedAtUtc)
            .Skip(skip)
            .Take(pageSize)
            .Select(f => new
            {
                f.Id,
                Name = f.OriginalName,
                f.ContentType,
                Size = f.SizeBytes,
                CreatedAt = f.CreatedAtUtc
            })
            .ToListAsync(ct);

        return Ok(new
        {
            page,
            pageSize,
            totalCount,
            totalPages,
            items
        });
    }

    
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> SoftDelete(Guid id, CancellationToken ct)
    {
        await _storage.SoftDeleteAsync(id, ct);
        return Ok(new { Message = "Soft deleted" });
    }

    
    [HttpDelete("{id:guid}/hard")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> HardDelete(Guid id, CancellationToken ct)
    {
        await _storage.HardDeleteAsync(id, ct);
        return Ok(new { Message = "Hard deleted" });
    }

    //new list night addeed
    [HttpGet]
    [Authorize(Roles = "user,admin")]
    public async Task<IActionResult> List(
    int page = 1,
    int pageSize = 20,
    string? search = null,
    string? tag = null,
    string? type = null,
    string? nameFilter = null,
    string? contentTypeFilter = null,
    string? fromDate = null,
    string? toDate = null,
    CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;

        var query = _db.StoredObjects
            .Where(x => x.DeletedAtUtc == null);
 
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x =>
                x.OriginalName.Contains(search) ||
                x.Key.Contains(search) ||
                (x.Tags != null && x.Tags.Contains(search)) ||
                x.ContentType.Contains(search)
            );
        }
 
        if (!string.IsNullOrWhiteSpace(nameFilter))
        {
            query = query.Where(x => x.OriginalName.Contains(nameFilter));
        }
 
        if (!string.IsNullOrWhiteSpace(contentTypeFilter))
        {
            query = query.Where(x => x.ContentType.Contains(contentTypeFilter));
        }
 
        if (!string.IsNullOrWhiteSpace(tag))
        {
            query = query.Where(x => x.Tags != null && x.Tags.Contains(tag));
        }
 
        if (!string.IsNullOrWhiteSpace(type))
        {
            query = query.Where(x => x.ContentType.Contains(type));
        }
 
        if (!string.IsNullOrWhiteSpace(fromDate) &&
            DateTime.TryParse(fromDate, out var from))
        {
            query = query.Where(x => x.CreatedAtUtc >= from);
        }

        if (!string.IsNullOrWhiteSpace(toDate) &&
            DateTime.TryParse(toDate, out var to))
        {
            query = query.Where(x => x.CreatedAtUtc <= to);
        }
 
        query = query.OrderByDescending(x => x.CreatedAtUtc);
         
        var total = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new
            {
                x.Id,
                x.OriginalName,
                x.Key,
                x.SizeBytes,
                x.ContentType,
                x.Checksum,
                x.Tags,
                x.CreatedAtUtc,
                x.DeletedAtUtc,
                x.Version,
                x.CreatedByUserId
            })
            .ToListAsync(ct);

        return Ok(new
        {
            page,
            pageSize,
            total,
            items
        });
    }


    //[HttpPost]
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
    


}






