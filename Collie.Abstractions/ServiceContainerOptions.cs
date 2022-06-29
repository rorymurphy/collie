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

        // 0 indicates no limit
        public uint MaxTenantSize { get; init; }

        public bool IgnoreUnresolvableEnumerables { get; init; }

        public bool ContextualOverrides { get; init; }
    }
}
