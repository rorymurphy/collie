using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collie.ServiceLookup
{
    public class MissingDependencyException : Exception
    {
        private const string MESSAGE_FORMAT = "Unable to construct type {0} due to missing dependency of type {1}.";
        public MissingDependencyException(Type dependentType, Type missingType, Exception innerException = null) : base(String.Format(MESSAGE_FORMAT, dependentType.FullName, missingType.FullName), innerException)
        { }
    }
}
