using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Common.Interfaces;
public interface IAppDbContext
{
    DbSet<StoredObject> StoredObjects { get; }
    //DbSet<FileRecord> Files { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
