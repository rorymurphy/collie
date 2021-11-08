using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collie.ServiceLookup
{
    public class CircularDependencyException : Exception
    {
        private const string MESSAGE_FORMAT = "Unable to construct type {0} due to a circular dependency via type {1}.";
        public CircularDependencyException(Type[] callChain, Exception innerException = null) : base(String.Format(MESSAGE_FORMAT, callChain.First()?.FullName ?? "Unknown", callChain.Last()?.FullName ?? "Unknown"), innerException)
        {
            callChain = callChain;
        }

        public Type[] CallChain { get; set; }
    }
}
