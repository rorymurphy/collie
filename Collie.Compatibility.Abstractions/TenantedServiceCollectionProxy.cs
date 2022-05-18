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

        public ServiceDescriptor this[int index] { get => services[index]; set => services[index] = value; }

        public int Count => services.Count;

        public bool IsReadOnly => services.IsReadOnly;

        public void Add(ServiceDescriptor item)
        {
            if(item.Lifetime == ServiceLifetime.Singleton)
            {
                services.Add(TranslateSingleton(item));
            } else
            {
                services.Add(item);
            }
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
            if (item.Lifetime == ServiceLifetime.Singleton)
            {
                services.Insert(index, TranslateSingleton(item));
            }
            else
            {
                services.Insert(index, item);
            }
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

        private ServiceDescriptor TranslateSingleton(ServiceDescriptor descriptor)
        {
            if(descriptor.ImplementationFactory != null)
            {
                return new TenantSingletonServiceDescriptor(descriptor.ServiceType, descriptor.ImplementationFactory);
            } else if(descriptor.ImplementationType != null)
            {
                return new TenantSingletonServiceDescriptor(descriptor.ServiceType, descriptor.ImplementationType);
            } else
            {
                return descriptor;
            }
        }
    }
}
