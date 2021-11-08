using Collie.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collie.ServiceLookup
{
    class ServiceIdentifier
    {
        private static readonly Type IEnumerableType = typeof(IEnumerable<>);

        public ServiceIdentifier(Type requestedType, ServiceDefinition definition)
        {
            ServiceType = requestedType ?? throw new ArgumentNullException(nameof(requestedType));
            Lifetime = definition.Lifetime;
            var genericType = ServiceType.IsGenericType ? ServiceType.GetGenericTypeDefinition() : null;

            if (definition.ServiceInstance != null) { Kind = ServiceCreatorKind.Constant; Distinguisher = definition.ServiceInstance; }
            else if(definition.ServiceFactory != null) { Kind = ServiceCreatorKind.Factory; Distinguisher = definition.ServiceFactory; }
            else if(genericType != null && definition.ServiceType.IsGenericTypeDefinition && definition.ServiceType == genericType) { Kind = ServiceCreatorKind.Generic; Distinguisher = definition.ImplementationType; }
            else if(requestedType == definition.ServiceType && requestedType.IsGenericType && requestedType.GetGenericTypeDefinition() == IEnumerableType) { Kind = ServiceCreatorKind.IEnumerable; Distinguisher = definition.ServiceType; }
            else if(definition.ImplementationType != null) { Kind = ServiceCreatorKind.Constructor; Distinguisher = definition.ImplementationType; }
        }

        public ServiceCreatorKind Kind { get; init; }

        public Type ServiceType { get; init; }

        public ServiceLifetime Lifetime { get; init; }

        public object Distinguisher { get; init; }

        //Intentionally omitting Lifetime from equality and hashcode, as it does not affect the factory used to instantiate the service
        public override bool Equals(object obj)
        {
            ServiceIdentifier other = obj as ServiceIdentifier;
            return other != null
                && other.Kind == this.Kind
                && other.ServiceType == this.ServiceType
                && other.Distinguisher == this.Distinguisher;
        }

        public override int GetHashCode()
        {
            return (Kind, ServiceType, Distinguisher).GetHashCode();
        }
    }
}
