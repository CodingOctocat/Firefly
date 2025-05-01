using Firefly.Factories;
using Firefly.Models;
using Firefly.Models.Responses;
using Firefly.Services;
using Firefly.Services.Abstractions;
using Firefly.Services.Navigation;
using Firefly.Services.Parsers;

using Microsoft.Extensions.DependencyInjection;

namespace Firefly.ServiceProviders;

public static class AddServicesExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddTransient<ProgressTimer>();

        services.AddSingleton<IDocxProvider, DocxProvider>();
        services.AddSingleton<IFireTableReader, FireTableReader>();
        services.AddSingleton<IFireTableWriter, FireTableWriter>();

        services.AddSingleton<IFireCheckSettings, FireCheckSettings>();
        services.AddTransient<IFireChecker<Cccf>, CccfChecker>();
        services.AddTransient<IFireCheckContextFactory, FireCheckContextFactory>();

        services.AddSingleton<FireTableService>();

        services.AddSingleton<IResponseParser<QueryResponse<Cccf>>, CccfParser>();

        services.AddTransient<INavigationService<CccfQuerySession>, NavigationService<CccfQuerySession>>();
        services.AddSingleton<IViewSwitcher, ViewSwitcher>();

        return services;
    }
}
