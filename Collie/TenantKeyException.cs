using Collie.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collie
{
    public class TenantKeyException : Exception
    {
        private const string MESSAGE_FORMAT = "Tenant keys and their dependencies must have lifetime Scoped. Key component {0} is registered with lifetime {1}.";
        public TenantKeyException(ServiceDefinition serviceDefinition, Exception innerException = null) : base(string.Format(MESSAGE_FORMAT, serviceDefinition.ServiceType.Name ?? "Unknown", serviceDefinition.Lifetime), innerException)
        {
        }
    }
}
