using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collie.Compatibility
{
    public class ContainerBuilder
    {
        public ContainerBuilder(IServiceCollection services)
        {
            this.Services = services;
        }

        public IServiceCollection Services { get; private set; }
    }
}
