using Domain.Entities;
using Application.Common.Interfaces;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Storage
{
    public class FileStorageService
    {
        private readonly IAppDbContext _db;
        private readonly IStoragePathStrategy _path;
        private readonly long _maxUploadBytes;
        private readonly HashSet<string> _allowedTypes;

        public FileStorageService(
            IAppDbContext db,
            IStoragePathStrategy path,
            long maxUploadBytes,
            IEnumerable<string> allowedTypes)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _path = path ?? throw new ArgumentNullException(nameof(path));
            _maxUploadBytes = maxUploadBytes;
            _allowedTypes = new HashSet<string>(allowedTypes, StringComparer.OrdinalIgnoreCase);
        }
        public async Task<StoredObject> SaveAsync(
       Stream input,
       string originalName,
       string contentType,
       string? tags,
       string createdByUserId,
       CancellationToken ct)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            // Block dangerous executable file types
            var forbiddenExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        ".exe", ".bat", ".cmd", ".sh", ".msi", ".ps1"
    };

            var ext = Path.GetExtension(originalName);
            if (!string.IsNullOrWhiteSpace(ext) && forbiddenExtensions.Contains(ext))
                throw new InvalidOperationException("Unsupported content type");

            var now = DateTime.UtcNow;
            var key = Guid.NewGuid().ToString("N");

            var safeName = Path.GetFileName(originalName);
            if (string.IsNullOrWhiteSpace(safeName))
                safeName = "unnamed";

            var finalPath = _path.GetPathForNewFile(safeName);
            var dir = Path.GetDirectoryName(finalPath)!;

            Directory.CreateDirectory(dir);

            var tmpPath = Path.Combine(dir, $"{key}.tmp");

            long size = 0;
            using var sha256 = SHA256.Create();

            try
            {
                using (var tmp = new FileStream(tmpPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true))
                {
                    var buffer = new byte[81920];
                    int read;

                    while ((read = await input.ReadAsync(buffer.AsMemory(0, buffer.Length), ct)) > 0)
                    {
                        size += read;

                        if (size > _maxUploadBytes)
                            throw new InvalidOperationException("Upload exceeds configured limit");

                        await tmp.WriteAsync(buffer.AsMemory(0, read), ct);
                        sha256.TransformBlock(buffer, 0, read, null, 0);
                    }
                }

                sha256.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
            }
            catch
            {
                // Cleanup temp file if upload fails
                if (File.Exists(tmpPath))
                    File.Delete(tmpPath);

                throw; // Let controller return proper error
            }

            var checksum = Convert.ToHexString(sha256.Hash!).ToLowerInvariant();

            File.Move(tmpPath, finalPath, overwrite: true);

            var entity = new StoredObject
            {
                Id = Guid.NewGuid(),
                Key = key,
                OriginalName = safeName,
                SizeBytes = size,
                ContentType = contentType,
                Checksum = checksum,
                Tags = tags,
                CreatedAtUtc = now,
                DeletedAtUtc = null,
                Version = 1,
                CreatedByUserId = createdByUserId,
                Path = finalPath
            };

            _db.StoredObjects.Add(entity);
            await _db.SaveChangesAsync(ct);

            return entity;
        }

        //public async Task<StoredObject> SaveAsync(
        //    Stream input,
        //    string originalName,
        //    string contentType,
        //    string? tags,
        //    string createdByUserId,
        //    CancellationToken ct)
        //{
        //    if (input == null)
        //        throw new ArgumentNullException(nameof(input));

        //    //  Validate content type
        //    if (!_allowedTypes.Contains(contentType))
        //        throw new InvalidOperationException("Unsupported content type");

        //    var now = DateTime.UtcNow;
        //    var key = Guid.NewGuid().ToString("N");

        //    var safeName = Path.GetFileName(originalName);
        //    if (string.IsNullOrWhiteSpace(safeName))
        //        safeName = "unnamed";

        //    var finalPath = _path.GetPathForNewFile(safeName);
        //    var dir = Path.GetDirectoryName(finalPath)!;

        //    Directory.CreateDirectory(dir);

        //    var tmpPath = Path.Combine(dir, $"{key}.tmp");

        //    long size = 0;
        //    using var sha256 = SHA256.Create();

        //    using (var tmp = new FileStream(tmpPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true))
        //    {
        //        var buffer = new byte[81920];
        //        int read;

        //        while ((read = await input.ReadAsync(buffer.AsMemory(0, buffer.Length), ct)) > 0)
        //        {
        //            size += read;

        //            if (size > _maxUploadBytes)
        //                throw new InvalidOperationException("Upload exceeds configured limit");

        //            await tmp.WriteAsync(buffer.AsMemory(0, read), ct);
        //            sha256.TransformBlock(buffer, 0, read, null, 0);
        //        }
        //    }

        //    sha256.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
        //    var checksum = Convert.ToHexString(sha256.Hash!).ToLowerInvariant();

        //    File.Move(tmpPath, finalPath, overwrite: true);

        //    var entity = new StoredObject
        //    {
        //        Id = Guid.NewGuid(),
        //        Key = key,
        //        OriginalName = safeName,
        //        SizeBytes = size,
        //        ContentType = contentType,
        //        Checksum = checksum,
        //        Tags = tags,
        //        CreatedAtUtc = now,
        //        DeletedAtUtc = null,
        //        Version = 1,
        //        CreatedByUserId = createdByUserId,
        //        Path = finalPath
        //    };

        //    _db.StoredObjects.Add(entity);
        //    await _db.SaveChangesAsync(ct);

        //    return entity;
        //}

        public async Task<(FileStream Stream, long Length, StoredObject Meta)> OpenForDownloadAsync(Guid id, CancellationToken ct)
        {
            var obj = await _db.StoredObjects
                .FirstOrDefaultAsync(x => x.Id == id && x.DeletedAtUtc == null, ct);

            if (obj == null)
                throw new FileNotFoundException();

            var fs = new FileStream(obj.Path, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);

            return (fs, obj.SizeBytes, obj);
        }

        public async Task SoftDeleteAsync(Guid id, CancellationToken ct)
        {
            var obj = await _db.StoredObjects
                .FirstOrDefaultAsync(x => x.Id == id && x.DeletedAtUtc == null, ct);

            if (obj == null)
                return;

            obj.DeletedAtUtc = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
        }

        public async Task HardDeleteAsync(Guid id, CancellationToken ct)
        {
            var obj = await _db.StoredObjects.FirstOrDefaultAsync(x => x.Id == id, ct);

            if (obj != null)
            {
                if (File.Exists(obj.Path))
                    File.Delete(obj.Path);

                _db.StoredObjects.Remove(obj);
            }

            await _db.SaveChangesAsync(ct);
        }
    }
}