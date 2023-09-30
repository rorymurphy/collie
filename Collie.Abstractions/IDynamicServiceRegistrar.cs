using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collie.Abstractions
{
    public interface IDynamicServiceRegistrar
    {
        public void RegisterServices(IServiceCatalog services);
    }
}
