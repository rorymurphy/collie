using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collie.ServiceLookup
{
    public class TenantLimitExceededException: Exception
    {
        private const string MESSAGE_FORMAT = "Could not create tenant with key: %1$s. Maximum number of simultaneous tenants exceeded.";
        public TenantLimitExceededException(object key, Exception innerException = null) : base(String.Format(MESSAGE_FORMAT, key), innerException)
        {
        }
    }
}
