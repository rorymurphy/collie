﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collie.ServiceLookup
{
    internal enum ServiceCreatorKind
    {
        Factory,

        Constructor,

        Generic,

        Constant,

        IEnumerable,

        ServiceProvider
    }
}
