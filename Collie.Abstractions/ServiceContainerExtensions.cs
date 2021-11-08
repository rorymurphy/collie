using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collie.Abstractions
{
    public static class ServiceContainerExtensions
    {
        public static T GetService<T>(this IServiceContainer container)
        {
            return (T)container.GetService(typeof(T));
        }
    }
}
