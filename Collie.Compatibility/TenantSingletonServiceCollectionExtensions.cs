using Collie.Compatibility;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collie
{
    public static class TenantSingletonServiceCollectionExtensions
    {
        public static IServiceCollection AddTenantSingleton<TService, TImplementation>(this IServiceCollection services)
        {
            services.Add(new TenantSingletonServiceDescriptor(typeof(TService), typeof(TImplementation)));
            return services;
        }

        public static IServiceCollection AddTenantSingleton<TService>(this IServiceCollection services, Func<IServiceProvider, TService> factory)
        {
            var serviceType = typeof(TService);
            Func<IServiceProvider, object> objFactory = provider => (object)factory(provider);

            services.Add(new TenantSingletonServiceDescriptor(typeof(TService), objFactory));
            return services;
        }
    }
}
