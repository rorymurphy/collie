using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collie.Compatibility
{
    class ServiceScope : IServiceScope
    {
        protected ServiceContainer serviceProvider;
        public ServiceScope(ServiceContainer serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
        public IServiceProvider ServiceProvider { get { return serviceProvider; } }

        public void Dispose()
        {
            serviceProvider.Dispose();
        }
    }
}
