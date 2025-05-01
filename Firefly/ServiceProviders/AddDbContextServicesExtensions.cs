using Firefly.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Firefly.ServiceProviders;

public static class AddDbContextServicesExtensions
{
    public static IServiceCollection AddDbContextServices(this IServiceCollection services, HostBuilderContext context)
    {
        // 注册为暂时性服务，避免相同实例出现并发访问问题
        services.AddDbContext<CccfDbContext>(o => o.UseSqlite(context.Configuration.GetConnectionString("DefaultConnection")),
            ServiceLifetime.Transient, ServiceLifetime.Singleton);

        return services;
    }
}
