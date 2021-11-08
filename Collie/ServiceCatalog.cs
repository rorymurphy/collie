using Collie.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collie
{
    public class ServiceCatalog : List<ServiceDefinition>, IServiceCatalog
    {
        public ServiceCatalog() { }

        public ServiceCatalog(IEnumerable<ServiceDefinition> services) : base(services) { }
    }
}
