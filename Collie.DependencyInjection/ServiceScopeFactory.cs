using Collie.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collie.DependencyInjection
{
    class ServiceScopeFactory : IServiceScopeFactory
    {
        protected IScopeBuilder scopeBuilder;
        public ServiceScopeFactory(IScopeBuilder scopeBuilder)
        {
            this.scopeBuilder = scopeBuilder;
        }

        public IServiceScope CreateScope()
        {
            var container = scopeBuilder.Create();
            var provider = new ServiceProvider(container);
            return new ServiceScope(provider);
        }
    }
}
