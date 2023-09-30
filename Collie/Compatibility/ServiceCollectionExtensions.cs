using Collie.Abstractions;
using Collie.Compatibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IServiceCollection = Microsoft.Extensions.DependencyInjection.IServiceCollection;
using MsftServiceLifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime;
using ServiceDescriptor = Microsoft.Extensions.DependencyInjection.ServiceDescriptor;

namespace Collie
{
    public static class ServiceCollectionExtensions
    {
        public static readonly Type IDynamicServiceRegistrarType = typeof(IDynamicServiceRegistrar);
        public static readonly Type IDynamicServiceConfigurerType = typeof(IDynamicServiceConfigurer);
        public static readonly Type DynamicServiceConfigurerAdapterType = typeof(DynamicServiceConfigurerAdapter);
        public static IServiceProvider BuildCollieProvider(this IServiceCollection services, Func<IServiceProvider, object> keySelctor, Type keyType, ServiceContainerOptions options = new ServiceContainerOptions())
        {
            var catalog = ConvertToServiceCatalog(services);

            Func<IServiceContainer, object> collieKeySelector = container => keySelctor((IServiceProvider)container);

            return (IServiceProvider)ServiceContainerFactory.Create(catalog, collieKeySelector, keyType, options);
        }

        public static IServiceCatalog ConvertToServiceCatalog(this IServiceCollection services)
        {
            ServiceCatalog catalog = new ServiceCatalog(services.Count);
            foreach (var svc in services)
            {
                var tfSvc = svc as TenantFilteringServiceDescriptor;
                catalog.Add(new ServiceDefinition(svc.ServiceType, GetServiceLifetime(svc))
                {
                    ImplementationType = svc.ImplementationType,
                    ServiceFactory = (svc.ImplementationFactory != null) ? sc => svc.ImplementationFactory((IServiceProvider)sc) : null,
                    ServiceInstance = svc.ImplementationInstance,
                    TenantFilter = tfSvc?.TenantFilter
                });

                if(svc.ServiceType == IDynamicServiceConfigurerType)
                {
                    catalog.Add(new ServiceDefinition(IDynamicServiceRegistrarType, GetServiceLifetime(svc))
                    {
                        ImplementationType = DynamicServiceConfigurerAdapterType,
                        TenantFilter = tfSvc?.TenantFilter
                    });
                }
            }

            return catalog;
        }

        private static ServiceLifetime GetServiceLifetime(ServiceDescriptor descriptor)
        {
            switch(descriptor.Lifetime)
            {
                case MsftServiceLifetime.Transient:
                    return ServiceLifetime.Transient;
                case MsftServiceLifetime.Singleton:
                    return ServiceLifetime.Singleton;
                case MsftServiceLifetime.Scoped:
                default:
                    if(descriptor is TenantSingletonServiceDescriptor)
                    {
                        return ServiceLifetime.TenantSingleton;
                    } else
                    {
                        return ServiceLifetime.Scoped;
                    }
            }
        }
    }
}
