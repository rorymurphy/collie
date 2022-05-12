using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collie.DependencyInjection
{
    class ServiceScope : IServiceScope
    {
        protected ServiceProvider serviceProvider;
        public ServiceScope(ServiceProvider serviceProvider)
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
