﻿using Collie.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collie
{
    public static class ServiceContainerFactory
    {
        public static IServiceContainer Create(IServiceCatalog services, Func<IServiceContainer, object> keySelector, Type keyType, int tenantCacheSize = 0)
        {
            return new ServiceContainer(services, keySelector, keyType) { TenantCacheSize = tenantCacheSize };
        }
    }
}