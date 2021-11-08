using Collie.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collie.ServiceLookup
{
    class ServiceCreatorCache : ConcurrentDictionary<ServiceIdentifier, Func<ServiceContainer, Type[], object>>
    {
    }
}
