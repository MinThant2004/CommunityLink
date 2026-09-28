using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityLink.Domain.Features.Premium;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CommunityLink.Api.BackgroundServices;

public sealed class SubscriptionExpirationWorker(
    IServiceProvider serviceProvider,
    ILogger<SubscriptionExpirationWorker> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(15);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("SubscriptionExpirationWorker started. Interval: {Interval} minutes.", Interval.TotalMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var verificationService = scope.ServiceProvider.GetRequiredService<IIdentityVerificationService>();

                var result = await verificationService.ProcessExpiredSubscriptionsAsync(stoppingToken);
                if (result.IsSuccess && result.Data > 0)
                {
                    logger.LogInformation("SubscriptionExpirationWorker processed {Count} expired subscriptions.", result.Data);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred in SubscriptionExpirationWorker.");
            }

            try
            {
                await Task.Delay(Interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        logger.LogInformation("SubscriptionExpirationWorker stopped.");
    }
}
