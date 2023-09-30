using Collie.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collie.Compatibility
{
    internal class DynamicServiceConfigurerAdapter : IDynamicServiceRegistrar
    {
        private IDynamicServiceConfigurer configurer;

        public DynamicServiceConfigurerAdapter(IDynamicServiceConfigurer configurer)
        {
            this.configurer = configurer;
        }

        public void RegisterServices(IServiceCatalog services)
        {
            var serviceCollection = new ServiceCollection();
            configurer.ConfigureServices(serviceCollection);
            var catalog = serviceCollection.ConvertToServiceCatalog();
            foreach(var svc in  catalog) { services.Add(svc); }
        }
    }
}
