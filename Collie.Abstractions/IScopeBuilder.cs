using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collie.Abstractions
{
    public interface IScopeBuilder
    {
        /// <summary>
        /// Creates a new scoped container, optionally injecting in additional services valid only in this scope
        /// </summary>
        /// <param name="scopedServices"></param>
        /// <returns>The new container</returns>
        IServiceContainer Create(IServiceCatalog scopedServices = null);
    }
}
