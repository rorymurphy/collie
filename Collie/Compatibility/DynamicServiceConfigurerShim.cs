using Collie.Abstractions;
using Collie.Compatibility.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collie.Compatibility
{
    internal class DynamicServiceConfigurerShim : IDynamicServiceRegistrar
    {
        private readonly IDynamicServiceConfigurer _dynamicServiceConfigurer;
        public DynamicServiceConfigurerShim(IDynamicServiceConfigurer dynamicServiceConfigurer) 
        {
            _dynamicServiceConfigurer = dynamicServiceConfigurer;
        }
        public void RegisterServices(IServiceCatalog serviceCatalog)
        {
            var services = new ServiceCollection();
            _dynamicServiceConfigurer.ConfigureServices(services);
            var catalog = services.ConvertToServiceCatalog();
            foreach (var descriptor in catalog)
            {
                serviceCatalog.Add(descriptor);
            }
        }
    }
}
