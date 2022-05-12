using Collie.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collie.DependencyInjection
{
    struct ServiceContainerWrapper : IServiceProvider
    {
        private static readonly Type SERVICE_PROVIDER_TYPE = typeof(IServiceProvider);

        private IServiceContainer container;

        public ServiceContainerWrapper(IServiceContainer container)
        {
            this.container = container;
        }
        public object GetService(Type serviceType)
        {
            throw new NotImplementedException();
        }
    }
}
