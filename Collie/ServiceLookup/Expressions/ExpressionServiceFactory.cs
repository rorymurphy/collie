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
        private static readonly MethodInfo ServiceContainerGetServiceInternalMethod = typeof(IServiceContainerExtended).GetMethod(nameof(IServiceContainerExtended.GetServiceInternal));
        private static readonly ConstructorInfo MissingDependencyExceptionConstructor = typeof(MissingDependencyException).GetConstructor(new Type[] { typeof(Type), typeof(Type), typeof(Exception) });
        private static readonly Type IEnumerableType = typeof(IEnumerable<>);
        private static readonly MethodInfo IEnumerableGeneratorMethod = typeof(ExpressionServiceFactory).GetMethod(nameof(CreateEnumerableInstanceInternal), BindingFlags.Static | BindingFlags.NonPublic);

        private static readonly MethodInfo AppendMethodInfo = typeof(Enumerable).GetMethod(nameof(Enumerable.Append));
        private static readonly MethodInfo ToArrayMethodInfo = typeof(Enumerable).GetMethod(nameof(Enumerable.ToArray));

        public Func<IServiceContainerExtended, Type[], object> CreateFactory(Type implementationType)
        {
            Func<IServiceContainerExtended, Type[], object> result = null;
            if (implementationType.IsGenericType && implementationType.GetGenericTypeDefinition() == IEnumerableType)
            {
                result = CreateEnumerableInstance(implementationType);
            } else
            {
                result = CreateInstanceFromConstructor(implementationType);
            }

            return result;
        }

        Func<IServiceContainerExtended, Type[], object> CreateInstanceFromConstructor(Type implementationType)
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

            var paramList = new List<ParameterExpression>(paramTypes.Length);
            var initList = new List<Expression>(paramTypes.Length * 2);
            var nullExpr = Expression.Constant(null);

            var containerExpr = Expression.Parameter(typeof(IServiceContainerExtended));
            var callChainExpr = Expression.Parameter(typeof(Type[]));

            Expression updateCallChainExpr = Expression.Call(AppendMethodInfo.MakeGenericMethod(typeof(Type)), callChainExpr, Expression.Constant(implementationType));
            updateCallChainExpr = Expression.Call(ToArrayMethodInfo.MakeGenericMethod(typeof(Type)), updateCallChainExpr);
            updateCallChainExpr = Expression.Assign(callChainExpr, updateCallChainExpr);

            foreach (var p in paramTypes)
            {
                var paramExpr = Expression.Parameter(p.ParameterType);
                var initExpr = Expression.Assign(paramExpr,
                    Expression.Convert(Expression.Call(containerExpr, ServiceContainerGetServiceInternalMethod, Expression.Constant(p.ParameterType), callChainExpr), p.ParameterType));
                var nullCheckExpr = Expression.IfThen(Expression.Equal(nullExpr, paramExpr),
                    Expression.Throw(Expression.New(MissingDependencyExceptionConstructor, Expression.Constant(implementationType), Expression.Constant(p.ParameterType), Expression.Convert(nullExpr, typeof(Exception)))));


                paramList.Add(paramExpr);
                initList.Add(initExpr);
                initList.Add(nullCheckExpr);
                

            }

            Expression newExpr = null;
            if(paramTypes.Length > 0)
            { newExpr = Expression.New(candidateConstructors[0], paramList.ToArray()); }
            else { newExpr = Expression.New(candidateConstructors[0]); }

            var blockList = new List<Expression>(initList.Count + 2);
            blockList.Add(updateCallChainExpr);
            blockList.AddRange(initList);
            blockList.Add(newExpr);
            var blockExpr = Expression.Block(paramList, blockList);
            var resultExpr = Expression.Lambda(blockExpr, containerExpr, callChainExpr);
            return (Func<IServiceContainerExtended, Type[], object>)resultExpr.Compile();
        }

        Func<IServiceContainerExtended, Type[], object> CreateEnumerableInstance(Type implementationType)
        {
            var serviceType = implementationType.GetGenericArguments()[0];

            var serviceContainerParam = Expression.Parameter(typeof(IServiceContainerExtended));
            var callChainParam = Expression.Parameter(typeof(Type[]));

            var method = IEnumerableGeneratorMethod.MakeGenericMethod(serviceType);
            var body = Expression.Call(method, serviceContainerParam, callChainParam);
            return (Func<IServiceContainerExtended, Type[], object>)Expression.Lambda(body, serviceContainerParam, callChainParam).Compile();
        }

        private static IEnumerable<T> CreateEnumerableInstanceInternal<T>(IServiceContainerExtended container, Type[] callChain)
        {
            callChain = callChain.Append(typeof(T)).ToArray();
            return container.GetImplementationTypes(typeof(T)).Select(c => (T)container.GetImplementation(c, callChain));
        }

        public Func<IServiceContainerExtended, Type[], object> CreateFactory(Type serviceType, Func<IServiceContainer, object> factory)
        {
            return (IServiceContainerExtended inner, Type[] callChain) =>
            {
                callChain = callChain.Append(serviceType).ToArray();
                var container = new WrapperServiceContainer(inner, callChain);
                return factory(container);
            };
        }


        protected static readonly ConstructorInfo WrapperServiceContainerConstructor = typeof(WrapperServiceContainer).GetConstructor(new Type[] { typeof(ServiceContainer), typeof(Type[]) });
        struct WrapperServiceContainer : IServiceContainer
        {
            private IServiceContainerExtended inner;
            private Type[] callChain;
            public WrapperServiceContainer(IServiceContainerExtended inner, Type[] callChain)
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
