using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collie.Test.SampleServices
{
    internal class ValueTypeDependentServiceA : IServiceA
    {
        public ValueTypeDependentServiceA((int, string) tuple)
        {

        }
    }
}
