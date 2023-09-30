using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collie.Abstractions
{
    public record struct ServiceContainerOptions(int TenantCacheSize = 0, uint MaxTenantSize = 0, bool IgnoreUnresolvableEnumerables = true, bool AllowContextualOverrides = true, bool AlwaysRequireResolution = false)
    { }
}
