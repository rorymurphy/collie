using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collie.Test.SampleServices
{
    class CompositeServiceE
    {
        public CompositeServiceE(IServiceA a, IServiceB b, IServiceC c)
        {
            ServiceA = a;
            ServiceB = b;
            ServiceC = c;
        }

        public CompositeServiceE(IServiceA a, IServiceB b)
        {
            ServiceA = a;
            ServiceB = b;
        }

        public IServiceA ServiceA { get; private set; }

        public IServiceB ServiceB { get; private set; }

        public IServiceC ServiceC { get; private set; }
    }
}
