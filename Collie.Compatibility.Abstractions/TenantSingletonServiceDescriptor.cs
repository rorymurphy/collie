using Microsoft.Extensions.DependencyInjection;
using System;

namespace Collie.Compatibility
{
    public class TenantSingletonServiceDescriptor : TenantFilteringServiceDescriptor
    {
        public TenantSingletonServiceDescriptor(Type serviceType, Func<IServiceProvider, object> factory) : base(serviceType, factory, ServiceLifetime.Scoped) { }

        public TenantSingletonServiceDescriptor(Type serviceType, Type implementationType) : base(serviceType, implementationType, ServiceLifetime.Scoped) { }

    }
}
