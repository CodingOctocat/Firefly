using Firefly.Factories;
using Firefly.Views;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Firefly.ServiceProviders;

public static class AddViewsExtensions
{
    /// <summary>
    /// 注意：XAML 实例化的视图与 DI 容器实例化的视图不是同一个对象。
    /// </summary>
    /// <param name="services"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public static IServiceCollection AddViews(this IServiceCollection services, HostBuilderContext context)
    {
        services.AddSingleton<MainWindow>();
        services.AddTransient<FireTableColumnMappingWindow>();
        services.AddSingleton<IFireTableColumnMappingWindowFactory, FireTableColumnMappingWindowFactory>();
        services.AddTransient<AboutWindow>();

        if (context.Configuration.GetValue("Cccf:LocalMode", false))
        {
            services.AddTransient<CccfScraperWindow>();
            services.AddTransient<CccfDbMergeWindow>();
            services.AddTransient<ShutdownView>();
        }

        return services;
    }
}
