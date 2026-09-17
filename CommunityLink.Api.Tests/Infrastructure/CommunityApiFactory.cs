using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using CommunityLink.Database.AppDbContextModels;

namespace CommunityLink.Api.Tests.Infrastructure;

public class CommunityApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = Guid.NewGuid().ToString("N");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            services.AddScoped(_ =>
            {
                var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseInMemoryDatabase(_dbName);
                return options.Options;
            });
        });
    }
}