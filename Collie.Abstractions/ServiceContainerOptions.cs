using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collie.Abstractions
{
    public struct ServiceContainerOptions
    {
        public int TenantCacheSize { get; init; }

        public bool IgnoreUnresolvableEnumerables { get; init; }
    }
}
