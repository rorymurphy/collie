using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collie.Compatibility
{
    class TenantedServiceCollectionProxy : IServiceCollection
    {
        private IServiceCollection services;

        public TenantedServiceCollectionProxy(IServiceCollection services)
        {
            this.services = services;
        }

        public ServiceDescriptor this[int index] {
            get => services[index]; 
            set => services[index] = TranslateSingleton(value);
        }

        public int Count => services.Count;

        public bool IsReadOnly => services.IsReadOnly;

        public void Add(ServiceDescriptor item)
        {
            services.Add(TranslateSingleton(item));
        }

        public void Clear()
        {
            services.Clear();
        }

        public bool Contains(ServiceDescriptor item)
        {
            return services.Contains(item);
        }

        public void CopyTo(ServiceDescriptor[] array, int arrayIndex)
        {
            services.CopyTo(array, arrayIndex);
        }

        public IEnumerator<ServiceDescriptor> GetEnumerator()
        {
            return services.GetEnumerator();
        }

        public int IndexOf(ServiceDescriptor item)
        {
            return services.IndexOf(item);
        }

        public void Insert(int index, ServiceDescriptor item)
        {
            services.Insert(index, TranslateSingleton(item));
        }

        public bool Remove(ServiceDescriptor item)
        {
            return services.Remove(item);
        }

        public void RemoveAt(int index)
        {
            services.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return services.GetEnumerator();
        }

        internal static ServiceDescriptor TranslateSingleton(ServiceDescriptor descriptor)
        {
            if (descriptor.Lifetime == ServiceLifetime.Singleton)
            {
                if (descriptor.ImplementationFactory != null)
                {
                    return new TenantSingletonServiceDescriptor(descriptor.ServiceType, descriptor.ImplementationFactory);
                }
                else if (descriptor.ImplementationType != null)
                {
                    return new TenantSingletonServiceDescriptor(descriptor.ServiceType, descriptor.ImplementationType);
                }
                else
                {
                    //If it truly is simply a singleton instance, go ahead and leave the scope alone.
                    return descriptor;
                }
            } else
            {
                return descriptor;
            }
        }
    }
}
