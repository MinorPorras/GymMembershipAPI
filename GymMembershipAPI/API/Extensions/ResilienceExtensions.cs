using Npgsql;
using Polly;
using Polly.Retry;
using Polly.Timeout;

namespace GymMembershipAPI.API.Extensions;

public static class ResilienceExtensions
{
    public static IServiceCollection AddResiliencePipeline(this IServiceCollection services)
    {
        services.AddResiliencePipeline("db-pipeline", builder =>
        {
            builder
                .AddRetry(new RetryStrategyOptions
                {
                    MaxRetryAttempts = 3,
                    BackoffType = DelayBackoffType.Exponential,
                    Delay = TimeSpan.FromSeconds(2),
                    ShouldHandle = new PredicateBuilder().Handle<Exception>()
                })
                .AddTimeout(new TimeoutStrategyOptions
                {
                    Timeout = TimeSpan.FromSeconds(10)
                });
        });

        return services;
    }
}