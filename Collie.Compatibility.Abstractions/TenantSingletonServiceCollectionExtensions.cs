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

        public static IServiceCollection TenantedServiceCollection(this IServiceCollection services)
        {
            if(services is TenantedServiceCollectionProxy)
            {
                return services;
            } else
            {
                return new TenantedServiceCollectionProxy(services);
            }
        }

        public static IServiceCollection ConvertSingletonsToTenantSingletons(this IServiceCollection services, IEnumerable<Type> typesToConvert)
        {
            var types = new HashSet<Type>(typesToConvert);
            for(int i = 0; i < services.Count; i++)
            {
                var t = services[i];
                if(types.Contains(t.ServiceType))
                {
                    services[i] = TenantedServiceCollectionProxy.TranslateSingleton(services[i]);
                }
            }

            return services;
        }

        public static IServiceCollection ConvertAllSingletonsToTenantSingletons(this IServiceCollection services, IEnumerable<Type> exclusions)
        {
            var exclude = new HashSet<Type>(exclusions);
            for (int i = 0; i < services.Count; i++)
            {
                var t = services[i];
                if (!exclude.Contains(t.ServiceType))
                {
                    services[i] = TenantedServiceCollectionProxy.TranslateSingleton(services[i]);
                }
            }

            return services;
        }
    }
}
