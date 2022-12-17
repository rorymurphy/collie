using Collie.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collie.ServiceLookup
{
    class DefaultScopeBuilder : IScopeBuilder
    {
        protected ServiceContainer rootContainer;

        protected IServiceCatalog services;

        protected Func<IServiceContainer, object> keySelector;

        protected Type keyType;

        protected ServiceContainerOptions serviceContainerOptions;

        public DefaultScopeBuilder(IServiceCatalog services, ServiceContainer rootContainer, Func<IServiceContainer, object> keySelector, Type keyType, ServiceContainerOptions serviceContainerOptions)
        {
            this.services = services;
            this.rootContainer = rootContainer;
            this.keySelector = keySelector;
            this.keyType = keyType;
            this.serviceContainerOptions = serviceContainerOptions;
        }

        public IServiceContainer Create(IServiceCatalog scopedServices = null)
        {
            //TODO: update to allow overrides or additions (rather than just additions)
            ServiceCatalog effectiveServices = new ServiceCatalog(services);
            if (scopedServices != null)
            {
                effectiveServices.AddRange(scopedServices);
            }
            return new ServiceContainer(effectiveServices, rootContainer, keySelector, keyType, serviceContainerOptions);
        }
    }
}
