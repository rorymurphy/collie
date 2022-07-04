using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collie.Abstractions
{
    public class ServiceDefinition
    {
        public ServiceDefinition(Type serviceType, ServiceLifetime lifetime) {
            this.ServiceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
            this.Lifetime = lifetime;
        }

        public ServiceDefinition(Type serviceType, ServiceLifetime lifetime, object instance) : this(serviceType, lifetime)
        {

            this.ServiceInstance = instance ?? throw new ArgumentNullException(nameof(instance));
            this.ImplementationType = instance.GetType();

            if (!serviceType.IsAssignableFrom(instance.GetType()))
            {
                throw new Exception("Provided instance does not implement the specified service type.");
            }
        }

        public ServiceDefinition(Type serviceType, ServiceLifetime lifetime, Func<IServiceContainer, object> factory) : this(serviceType, lifetime)
        {
            this.ServiceFactory = factory;
        }

        public ServiceDefinition(Type serviceType, ServiceLifetime lifetime, Type implementationType) : this(serviceType, lifetime)
        {
            this.ImplementationType = implementationType;
        }

        public Type ServiceType { get; init; }

        public Func<object, bool> TenantFilter { get; init; }

        public Type ImplementationType { get; init; }

        public ServiceLifetime Lifetime { get; init; }

        public object ServiceInstance { get; init; }

        public Func<IServiceContainer, object> ServiceFactory { get; init; }
    }
}
