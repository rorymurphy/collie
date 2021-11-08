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
        Func<ServiceContainer, Type[], object> CreateFactory(Type implementationType);

        Func<ServiceContainer, Type[], object> CreateFactory(Type serviceType, Func<IServiceContainer, object> factory);
    }
}
