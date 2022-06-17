using Collie.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collie.Compatibility
{
    public class MultitenantServiceProviderFactory : IServiceProviderFactory<ContainerBuilder>
    {
        private Func<IServiceProvider, object> keySelector;
        private Type keyType;
        private ServiceContainerOptions options;

        public MultitenantServiceProviderFactory(Func<IServiceProvider, object> keySelector, Type keyType, ServiceContainerOptions options = new ServiceContainerOptions())
        {
            this.keySelector = keySelector;
            this.keyType = keyType;
            this.options = options;
        }
        public ContainerBuilder CreateBuilder(IServiceCollection services)
        {
            return new ContainerBuilder(services);
        }

        public IServiceProvider CreateServiceProvider(ContainerBuilder containerBuilder)
        {
            return containerBuilder.Services.BuildCollieProvider(keySelector, keyType, options);
        }
    }
}
