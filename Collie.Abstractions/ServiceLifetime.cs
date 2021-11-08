using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collie.Abstractions
{
    public enum ServiceLifetime { Transient, Scoped, TenantSingleton, Singleton };
}
