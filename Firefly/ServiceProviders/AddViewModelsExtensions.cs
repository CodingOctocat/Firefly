using Firefly.ViewModels;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Firefly.ServiceProviders;

public static class AddViewModelsExtensions
{
    public static IServiceCollection AddViewModels(this IServiceCollection services, HostBuilderContext context)
    {
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<MenuViewModel>();
        services.AddSingleton<FireflyViewModel>();
        services.AddTransient<FireTableColumnMappingViewModel>();

        services.AddSingleton<CccfMainQueryViewModel>();
        services.AddSingleton<CccfOnlineQueryViewModel>();

        if (context.Configuration.GetValue("Cccf:LocalMode", false))
        {
            services.AddSingleton<CccfLocalQueryViewModel>();
            services.AddTransient<CccfScraperViewModel>();
            services.AddTransient<CccfDbMergeViewModel>();
            services.AddTransient<ShutdownViewModel>();
        }

        services.AddSingleton<StatusBarViewModel>();
        services.AddSingleton<VersionInfoViewModel>();
        services.AddSingleton<ThemeSwitchButtonViewModel>();

        services.AddTransient<AboutViewModel>();

        return services;
    }
}
