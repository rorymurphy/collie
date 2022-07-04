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

        public static IServiceCatalog AddTenantSingleton<TService, TImplementation>(this IServiceCatalog services, Func<object, bool> tenantFilter = null) where TImplementation : TService => AddWithLifetime<TService, TImplementation>(services, ServiceLifetime.TenantSingleton, tenantFilter);

        public static IServiceCatalog AddTenantSingleton<TService>(this IServiceCatalog services, TService instance, Func<object, bool> tenantFilter = null) => AddWithLifetime<TService>(services, instance, ServiceLifetime.TenantSingleton, tenantFilter);

        public static IServiceCatalog AddTenantSingleton<TService>(this IServiceCatalog services, Func<IServiceContainer, TService> factory, Func<object, bool> tenantFilter = null) => AddWithLifetime<TService>(services, factory, ServiceLifetime.TenantSingleton, tenantFilter);

        public static IServiceCatalog AddScoped<TService, TImplementation>(this IServiceCatalog services, Func<object, bool> tenantFilter = null) where TImplementation : TService => AddWithLifetime<TService, TImplementation>(services, ServiceLifetime.Scoped, tenantFilter);

        public static IServiceCatalog AddScoped<TService>(this IServiceCatalog services, TService instance, Func<object, bool> tenantFilter = null) => AddWithLifetime<TService>(services, instance, ServiceLifetime.Scoped, tenantFilter);

        public static IServiceCatalog AddScoped<TService>(this IServiceCatalog services, Func<IServiceContainer, TService> factory, Func<object, bool> tenantFilter = null) => AddWithLifetime<TService>(services, factory, ServiceLifetime.Scoped, tenantFilter);

        public static IServiceCatalog AddTransient<TService, TImplementation>(this IServiceCatalog services, Func<object, bool> tenantFilter = null) where TImplementation : TService => AddWithLifetime<TService, TImplementation>(services, ServiceLifetime.Transient, tenantFilter);

        public static IServiceCatalog AddTransient<TService>(this IServiceCatalog services, TService instance, Func<object, bool> tenantFilter = null) => AddWithLifetime<TService>(services, instance, ServiceLifetime.Transient, tenantFilter);

        public static IServiceCatalog AddTransient<TService>(this IServiceCatalog services, Func<IServiceContainer, TService> factory, Func<object, bool> tenantFilter = null) => AddWithLifetime<TService>(services, factory, ServiceLifetime.Transient, tenantFilter);

        private static IServiceCatalog AddWithLifetime<TService, TImplementation>(IServiceCatalog services, ServiceLifetime lifetime, Func<object, bool> tenantFilter = null)
        {
            services.Add(new ServiceDefinition(typeof(TService), lifetime, typeof(TImplementation)) { TenantFilter = tenantFilter });
            return services;
        }

        public static IServiceCatalog AddWithLifetime<TService>(IServiceCatalog services, TService instance, ServiceLifetime lifetime, Func<object, bool> tenantFilter = null)
        {
            services.Add(new ServiceDefinition(typeof(TService), lifetime, instance) { TenantFilter = tenantFilter });
            return services;
        }

        public static IServiceCatalog AddWithLifetime<TService>(IServiceCatalog services, Func<IServiceContainer, TService> factory, ServiceLifetime lifetime, Func<object, bool> tenantFilter = null)
        {
            if(services == null) { throw new ArgumentNullException(nameof(services)); }
            if (factory == null) { throw new ArgumentNullException(nameof(factory)); }

            var objFactory = factory as Func<IServiceContainer, object>;
            objFactory = objFactory ?? ((IServiceContainer c) => (object)factory(c));

            services.Add(new ServiceDefinition(typeof(TService), lifetime, objFactory) { TenantFilter = tenantFilter });
            return services;
        }
    }
}
