using Application.Common.Interfaces;
using Infrastructure.Storage;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Infrastructure.Persistence;
 

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Database
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

            // Storage root folder
            var storageRoot = Path.Combine(Directory.GetCurrentDirectory(), "uploads");

            // Path strategy
            services.AddSingleton<IStoragePathStrategy>(
                new DatePartitionedPathStrategy(storageRoot));

            // File storage service
            services.AddScoped<FileStorageService>();

            return services;
        }
    }
}
