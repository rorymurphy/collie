using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collie.Abstractions
{
    public static class ServiceCatalogExtensions
    {
        public static IServiceCatalog AddSingleton<TService, TImplementation>(this IServiceCatalog services) where TImplementation : TService => AddWithLifetime<TService, TImplementation>(services, ServiceLifetime.Singleton);

        public static IServiceCatalog AddSingleton<TService>(this IServiceCatalog services, TService instance) => AddWithLifetime<TService>(services, instance, ServiceLifetime.Singleton);

        public static IServiceCatalog AddSingleton<TService>(this IServiceCatalog services, Func<IServiceContainer, TService> factory) => AddWithLifetime<TService>(services, factory, ServiceLifetime.Singleton);

        public static IServiceCatalog AddTenantSingleton<TService, TImplementation>(this IServiceCatalog services) where TImplementation : TService => AddWithLifetime<TService, TImplementation>(services, ServiceLifetime.TenantSingleton);

        public static IServiceCatalog AddTenantSingleton<TService>(this IServiceCatalog services, TService instance) => AddWithLifetime<TService>(services, instance, ServiceLifetime.TenantSingleton);

        public static IServiceCatalog AddTenantSingleton<TService>(this IServiceCatalog services, Func<IServiceContainer, TService> factory) => AddWithLifetime<TService>(services, factory, ServiceLifetime.TenantSingleton);

        public static IServiceCatalog AddScoped<TService, TImplementation>(this IServiceCatalog services) where TImplementation : TService => AddWithLifetime<TService, TImplementation>(services, ServiceLifetime.Scoped);

        public static IServiceCatalog AddScoped<TService>(this IServiceCatalog services, TService instance) => AddWithLifetime<TService>(services, instance, ServiceLifetime.Scoped);

        public static IServiceCatalog AddScoped<TService>(this IServiceCatalog services, Func<IServiceContainer, TService> factory) => AddWithLifetime<TService>(services, factory, ServiceLifetime.Scoped);

        public static IServiceCatalog AddTransient<TService, TImplementation>(this IServiceCatalog services) where TImplementation : TService => AddWithLifetime<TService, TImplementation>(services, ServiceLifetime.Transient);

        public static IServiceCatalog AddTransient<TService>(this IServiceCatalog services, TService instance) => AddWithLifetime<TService>(services, instance, ServiceLifetime.Transient);

        public static IServiceCatalog AddTransient<TService>(this IServiceCatalog services, Func<IServiceContainer, TService> factory) => AddWithLifetime<TService>(services, factory, ServiceLifetime.Transient);

        private static IServiceCatalog AddWithLifetime<TService, TImplementation>(IServiceCatalog services, ServiceLifetime lifetime)
        {
            services.Add(new ServiceDefinition(typeof(TService), lifetime, typeof(TImplementation)));
            return services;
        }

        public static IServiceCatalog AddWithLifetime<TService>(IServiceCatalog services, TService instance, ServiceLifetime lifetime)
        {
            services.Add(new ServiceDefinition(typeof(TService), lifetime, instance));
            return services;
        }

        public static IServiceCatalog AddWithLifetime<TService>(IServiceCatalog services, Func<IServiceContainer, TService> factory, ServiceLifetime lifetime)
        {
            var objFactory = factory as Func<IServiceContainer, object>;
            if(objFactory == null)
            {
                throw new ArgumentException("Invalid factory function");
            }
            services.Add(new ServiceDefinition(typeof(TService), lifetime, objFactory));
            return services;
        }
    }
}
