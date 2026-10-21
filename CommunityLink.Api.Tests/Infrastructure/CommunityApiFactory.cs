using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using CommunityLink.Database.AppDbContextModels;
using CommunityLink.Domain.Services;
using System.Threading;
using System.Threading.Tasks;

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

            var emailDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IEmailSender));
            if (emailDescriptor != null) services.Remove(emailDescriptor);
            services.AddScoped<IEmailSender, TestEmailSender>();
        });
    }

    private class TestEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string toEmail, string subject, string bodyHtml, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
