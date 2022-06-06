using Collie.Abstractions;
using Collie.ServiceLookup;
using System;
using System.Collections.Generic;

namespace Collie
{
    interface IServiceContainerExtended : IServiceContainer
    {
        object GetImplementation(ServiceIdentifier identifier, Type[] callChain);
        IEnumerable<ServiceIdentifier> GetImplementationTypes(Type serviceType);
        object GetServiceInternal(Type serviceType, Type[] callChain);

        bool IsResolvable(Type serviceType, Type[] callChain);
    }
}