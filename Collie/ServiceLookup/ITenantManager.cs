﻿using Collie.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collie.ServiceLookup
{
    interface ITenantManager
    {
        ServiceContainer CaptureTenant(object key, ServiceContainerOptions serviceContainerOptions);

        void ReleaseTenant(object key);
    }
}
