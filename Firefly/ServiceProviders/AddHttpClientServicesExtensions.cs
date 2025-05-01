using System;
using System.Net.Http;
using System.Threading.RateLimiting;

using Firefly.Factories;
using Firefly.Services;
using Firefly.Services.Abstractions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http.Resilience;

using Polly;

namespace Firefly.ServiceProviders;

public static class AddHttpClientServicesExtensions
{
    public static IServiceCollection AddHttpClientServices(this IServiceCollection services, HostBuilderContext context)
    {
        AddHttpClientWithResilience(services, context, CccfServiceFactory.AutoPipeline, 3, GetFixedWindowRateLimiter(10, 1), TimeSpan.FromSeconds(10));
        AddHttpClientWithResilience(services, context, CccfServiceFactory.ManualPipeline, 3, GetSlidingWindowRateLimiter(15, 2, 15), TimeSpan.FromSeconds(10));
        AddHttpClientWithResilience(services, context, CccfServiceFactory.ScraperPipeline, 5, GetFixedWindowRateLimiter(2, 1), TimeSpan.FromSeconds(30));

        services.AddSingleton<ICccfServiceFactory, CccfServiceFactory>();

        return services;
    }

    public static HttpRetryStrategyOptions GetRetryTimesStrategyOptions(int retry)
    {
        return new() {
            BackoffType = DelayBackoffType.Exponential,  // 指数退避
            UseJitter = true,  // 添加随机抖动延迟以获得更好的分布
            MaxRetryAttempts = retry
        };
    }

    private static void AddHttpClientWithResilience(IServiceCollection services, HostBuilderContext context, string pipelineName, int retry, RateLimiter rateLimiter, TimeSpan timeout)
    {
        // Hanlder 需要每次创建新的实例，否则可能会被释放掉
        var builder = services.AddHttpClient<ICccfService, CccfService>(pipelineName, c => c.BaseAddress = new Uri(context.Configuration["CccfApi:BaseUrl"]!))
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler() {
                ClientCertificateOptions = ClientCertificateOption.Manual,
                ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, certChain, policyErrors) => true
            });

        builder.AddResilienceHandler(pipelineName, builder => builder
                .AddRetry(GetRetryTimesStrategyOptions(retry))
                .AddRateLimiter(rateLimiter)
                .AddTimeout(timeout));
    }

    private static FixedWindowRateLimiter GetFixedWindowRateLimiter(int limit, int window)
    {
        var options = new FixedWindowRateLimiterOptions {
            PermitLimit = limit,
            Window = TimeSpan.FromSeconds(window)
        };

        return new FixedWindowRateLimiter(options);
    }

    private static SlidingWindowRateLimiter GetSlidingWindowRateLimiter(int limit, int seg, int window)
    {
        var options = new SlidingWindowRateLimiterOptions {
            PermitLimit = limit,
            SegmentsPerWindow = seg,
            Window = TimeSpan.FromSeconds(window)
        };

        return new SlidingWindowRateLimiter(options);
    }
}
