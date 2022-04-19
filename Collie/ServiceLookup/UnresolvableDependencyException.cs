using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collie.ServiceLookup
{
    public class UnresolvableDependencyException : Exception
    {
        private const string MESSAGE_FORMAT = "Unable to construct type {0} due to unresolvable dependency of type {1}.";
        public UnresolvableDependencyException(Type dependentType, Type unresolvableType, Exception innerException = null) : base(String.Format(MESSAGE_FORMAT, dependentType.FullName, unresolvableType.FullName), innerException)
        { }
    }
}
