using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collie.Compatibility.Abstractions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddScoped<TService, TImplementation>(this IServiceCollection services, Func<object, bool> tenantFilter) where TImplementation : TService
        {
            services.Add(new TenantFilteringServiceDescriptor(typeof(TService), typeof(TImplementation), ServiceLifetime.Scoped) { TenantFilter = tenantFilter});
            return services;
        }

        public static IServiceCollection AddScoped<TService>(this IServiceCollection services, Func<IServiceProvider, object> factory, Func<object, bool> tenantFilter)
        {
            services.Add(new TenantFilteringServiceDescriptor(typeof(TService), factory, ServiceLifetime.Scoped) { TenantFilter = tenantFilter });
            return services;
        }
    }
}
