using Collie.Abstractions;
using System;
using System.Collections.Generic;

namespace Collie.ServiceLookup
{
    interface IServiceContainerExtended : IServiceContainer
    {
        object GetImplementation(ServiceIdentifier identifier, Type[] callChain);
        IEnumerable<ServiceIdentifier> GetImplementationTypes(Type serviceType);
        object GetServiceInternal(Type serviceType, Type[] callChain);
    }
}