using Collie.Abstractions;
using Collie.Compatibility;
using Collie.Compatibility.Abstractions;
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
        private static readonly Type IDynamicServiceConfigurerType = typeof(IDynamicServiceConfigurer);
        public static IServiceProvider BuildCollieProvider(this IServiceCollection services, Func<IServiceProvider, object> keySelctor, Type keyType, ServiceContainerOptions options = new ServiceContainerOptions())
        {
            var catalog = new ServiceCatalog(services.Count);
            foreach (var svc in services)
            {
                var tfSvc = svc as TenantFilteringServiceDescriptor;
                var defintion = new ServiceDefinition(svc.ServiceType, GetServiceLifetime(svc))
                {
                    ImplementationType = svc.ImplementationType,
                    ServiceFactory = (svc.ImplementationFactory != null) ? sc => svc.ImplementationFactory((IServiceProvider)sc) : null,
                    ServiceInstance = svc.ImplementationInstance,
                    TenantFilter = (tfSvc != null) ? tfSvc.TenantFilter : null
                };

                catalog.Add(defintion);

                if(svc.ServiceType == IDynamicServiceConfigurerType)
                {
                    defintion = new ServiceDefinition(typeof(IDynamicServiceRegistrar), GetServiceLifetime(svc))
                    {
                        ServiceInstance = typeof(DynamicServiceConfigurerShim),
                        TenantFilter = (tfSvc != null) ? tfSvc.TenantFilter : null
                    };
                    catalog.Add(defintion);
                }
            }
            ConvertToServiceCatalog(services);

            Func<IServiceContainer, object> collieKeySelector = container => keySelctor((IServiceProvider)container);

            return (IServiceProvider)ServiceContainerFactory.Create(catalog, collieKeySelector, keyType, options);
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

        public static IServiceCatalog ConvertToServiceCatalog(this IServiceCollection services)
        {
            var catalog = new ServiceCatalog(services.Select(svc =>
            {
                var tfSvc = svc as TenantFilteringServiceDescriptor;
                return new ServiceDefinition(svc.ServiceType, GetServiceLifetime(svc))
                {
                    ImplementationType = svc.ImplementationType,
                    ServiceFactory = (svc.ImplementationFactory != null) ? sc => svc.ImplementationFactory((IServiceProvider)sc) : null,
                    ServiceInstance = svc.ImplementationInstance,
                    TenantFilter = (tfSvc != null) ? tfSvc.TenantFilter : null
                };
            }));

            return catalog;
        }
    }
}
