using Collie.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Collie.ServiceLookup.Expressions
{
    class ExpressionServiceFactory : IServiceFactoryGenerator
    {
        private static readonly MethodInfo IServiceContainerGetServiceMethod = typeof(IServiceContainer).GetMethod(nameof(IServiceContainer.GetService));
        private static readonly MethodInfo ServiceContainerGetServiceInternalMethod = typeof(ServiceContainer).GetMethod(nameof(ServiceContainer.GetServiceInternal));
        private static readonly ConstructorInfo MissingDependencyExceptionConstructor = typeof(MissingDependencyException).GetConstructor(new Type[] { typeof(Type), typeof(Type), typeof(Exception) });
        private static readonly Type IEnumerableType = typeof(IEnumerable<>);
        private static readonly MethodInfo IEnumerableGeneratorMethod = typeof(ExpressionServiceFactory).GetMethod(nameof(CreateEnumerableInstanceInternal), BindingFlags.Static | BindingFlags.NonPublic);

        private static readonly MethodInfo AppendMethodInfo = typeof(Enumerable).GetMethod(nameof(Enumerable.Append));
        private static readonly MethodInfo ToArrayMethodInfo = typeof(Enumerable).GetMethod(nameof(Enumerable.ToArray));

        public Func<ServiceContainer, Type[], object> CreateFactory(Type implementationType)
        {
            Func<ServiceContainer, Type[], object> result = null;
            if (implementationType.IsGenericType && implementationType.GetGenericTypeDefinition() == IEnumerableType)
            {
                result = CreateEnumerableInstance(implementationType);
            } else
            {
                result = CreateInstanceFromConstructor(implementationType);
            }

            return result;
        }

        Func<ServiceContainer, Type[], object> CreateInstanceFromConstructor(Type implementationType)
        {
            var candidateConstructors = implementationType.GetConstructors().Where(c => c.IsPublic).ToArray();
            if(candidateConstructors.Length == 0)
            {
                throw new ArgumentException("Could not find a public constructor for dependency injection.");
            }
            if(candidateConstructors.Length > 1)
            {
                throw new ArgumentException("Multiple public constructors are not currently supported for dependency injection.");
            }

            var paramTypes = candidateConstructors[0].GetParameters();

            var expressionList = new List<Expression>();
            var nullExpr = Expression.Constant(null);

            var containerExpr = Expression.Parameter(typeof(IServiceContainer));
            var callChainExpr = Expression.Parameter(typeof(Type[]));

            Expression updateCallChainExpr = Expression.Call(AppendMethodInfo.MakeGenericMethod(typeof(Type)), callChainExpr, Expression.Constant(implementationType));
            updateCallChainExpr = Expression.Call(ToArrayMethodInfo.MakeGenericMethod(typeof(Type)), updateCallChainExpr);
            updateCallChainExpr = Expression.Assign(callChainExpr, updateCallChainExpr);

            foreach (var p in paramTypes)
            {          
                expressionList.Add(Expression.OrElse(
                    Expression.Call(containerExpr, ServiceContainerGetServiceInternalMethod, Expression.Constant(p), callChainExpr),
                    //Throw an exception if any of the arguments are not resolved
                    Expression.Throw(Expression.New(MissingDependencyExceptionConstructor, Expression.Constant(implementationType), Expression.Constant(p.ParameterType), nullExpr)))
                );
                

            }

            var newExpr = Expression.New(candidateConstructors[0], expressionList.ToArray());
            var blockExpr = Expression.Block(updateCallChainExpr, newExpr);
            var resultExpr = Expression.Lambda(blockExpr, containerExpr, callChainExpr);
            return (Func<ServiceContainer, Type[], object>)resultExpr.Compile();
        }

        Func<ServiceContainer, Type[], object> CreateEnumerableInstance(Type implementationType)
        {
            var serviceType = implementationType.GetGenericArguments()[0];

            var serviceContainerParam = Expression.Parameter(typeof(ServiceContainer));
            var callChainParam = Expression.Parameter(typeof(Type[]));

            var method = IEnumerableGeneratorMethod.MakeGenericMethod(serviceType);
            var body = Expression.Call(method, serviceContainerParam, callChainParam);
            return (Func<ServiceContainer, Type[], object>)Expression.Lambda(body, serviceContainerParam, callChainParam).Compile();
        }

        private static IEnumerable<T> CreateEnumerableInstanceInternal<T>(ServiceContainer container, Type[] callChain)
        {
            callChain = callChain.Append(typeof(T)).ToArray();
            return container.GetImplementationTypes(typeof(T)).Select(c => (T)container.GetImplementation(c, callChain));
        }

        public Func<ServiceContainer, Type[], object> CreateFactory(Type serviceType, Func<IServiceContainer, object> factory)
        {
            return (ServiceContainer inner, Type[] callChain) =>
            {
                callChain = callChain.Append(serviceType).ToArray();
                var container = new WrapperServiceContainer(inner, callChain);
                return factory(container);
            };
        }


        protected static readonly ConstructorInfo WrapperServiceContainerConstructor = typeof(WrapperServiceContainer).GetConstructor(new Type[] { typeof(ServiceContainer), typeof(Type[]) });
        struct WrapperServiceContainer : IServiceContainer
        {
            private ServiceContainer inner;
            private Type[] callChain;
            public WrapperServiceContainer(ServiceContainer inner, Type[] callChain)
            {
                this.inner = inner;
                this.callChain = callChain;
            }
            public object GetService(Type serviceType)
            {
                return inner.GetServiceInternal(serviceType, callChain);
            }
        }
    }
}
