using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collie.Compatibility
{
    public class TenantFilteringServiceDescriptor : ServiceDescriptor
    {
        public TenantFilteringServiceDescriptor(Type serviceType, object instance) : base(serviceType, instance) { }

        public TenantFilteringServiceDescriptor(Type serviceType, Type implementationType, ServiceLifetime lifetime) : base(serviceType, implementationType, lifetime) { }

        public TenantFilteringServiceDescriptor(Type serviceType, Func<IServiceProvider, object> factory, ServiceLifetime lifetime) : base(serviceType, factory, lifetime) { }

        public Func<object, bool> TenantFilter { get; init; }
    }
}
