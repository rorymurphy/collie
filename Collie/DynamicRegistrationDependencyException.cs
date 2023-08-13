using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collie
{
    public class DynamicRegistrationDependencyException : Exception
    {
        private const string MESSAGE_FORMAT = "Unable to dynamically register {0} because it is either part of the container key, or is a dependency of the dynamic service registrar itself.";
        public DynamicRegistrationDependencyException(Type unregisterableType, Exception innerException = null) : base(string.Format(MESSAGE_FORMAT, unregisterableType.Name ?? "Unknown"), innerException)
        {
        }
    }
}
