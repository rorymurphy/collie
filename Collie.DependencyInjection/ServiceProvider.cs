using Collie.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace Collie.DependencyInjection
{
    public class ServiceProvider : IServiceProvider, IDisposable
    {
        protected static readonly Type SERVICE_SCOPE_FACTORY_TYPE = typeof(IServiceScopeFactory);
        protected static readonly Type SERVICE_PROVIDER_TYPE = typeof(IServiceProvider);
        protected IServiceContainer container;
        protected IServiceScopeFactory scopeFactory;

        public ServiceProvider(IServiceCollection services, Func<IServiceProvider, object> keySelector, Type keyType)
        {
            this.container = ServiceContainerFactory.Create(ConvertToServiceCatalog(services), keySelector, keyType);
        }

        internal ServiceProvider(IServiceContainer container)
        {
            this.container = container;
            this.scopeFactory = new ServiceScopeFactory(container.GetService<IScopeBuilder>());
        }

        public void Dispose()
        {
            var disposableContainer = container as IDisposable;
            if(disposableContainer != null)
            {
                disposableContainer.Dispose();
            }
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == SERVICE_SCOPE_FACTORY_TYPE)
            {
                return scopeFactory;
            } else if (serviceType == SERVICE_PROVIDER_TYPE)
            {
                return this;
            } else {
                return container.GetService(serviceType);
            }
        }

        protected IServiceCatalog ConvertToServiceCatalog(IServiceCollection services)
        {
            ServiceCatalog catalog = new ServiceCatalog();
            catalog.AddRange(services.Select(s =>
            {
                var lifetime = GetServiceLifetime(s);
                new ServiceDescriptor()
            });
        }

        protected ServiceLifetime GetServiceLifetime(ServiceDescriptor service)
        {

        }
    }
}
