using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collie.Compatibility.Abstractions
{
    public interface IDynamicServiceConfigurer
    {
        void ConfigureServices(IServiceCollection services);
    }
}
