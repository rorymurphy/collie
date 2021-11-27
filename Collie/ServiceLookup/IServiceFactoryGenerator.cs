using Collie.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collie.ServiceLookup
{
    interface IServiceFactoryGenerator
    {
        Func<IServiceContainerExtended, Type[], object> CreateFactory(Type implementationType);

        Func<IServiceContainerExtended, Type[], object> CreateFactory(Type serviceType, Func<IServiceContainer, object> factory);
    }
}
