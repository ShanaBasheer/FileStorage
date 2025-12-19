using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure;

public class DesignTimeFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        //var options = new DbContextOptionsBuilder<AppDbContext>()
        //    .UseSqlServer("Server=localhost,1433;Database=FileStore;User Id=sa;Password=Your_password123;TrustServerCertificate=true;")
        //    .Options;
        //return new AppDbContext(options);
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=FileStorageDb;Trusted_Connection=True;TrustServerCertificate=True;")
            .Options;
        return new AppDbContext(options);

    }
}
