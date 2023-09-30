using Collie.Abstractions;
using System;
using System.Collections.Concurrent;
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

        private static readonly ConcurrentDictionary<ConstructorInfo, Func<IServiceContainerExtended, Type[], object>> constructorFunctions = new ConcurrentDictionary<ConstructorInfo, Func<IServiceContainerExtended, Type[], object>>();

        public Func<IServiceContainerExtended, Type[], object> CreateFactory(Type implementationType)
        {
            Func<IServiceContainerExtended, Type[], object> result = null;
            if (implementationType.IsGenericType && implementationType.GetGenericTypeDefinition() == IEnumerableType)
            {
                result = CreateEnumerableInstance(implementationType);
            } else
            {
                result = CreateInstanceFromUnknownConstructor(implementationType);
            }

            return result;
        }

        Func<IServiceContainerExtended, Type[], object> CreateInstanceFromUnknownConstructor(Type implementationType)
        {
            var candidateConstructors = implementationType.GetConstructors().Where(c => c.IsPublic).ToArray();

            if(candidateConstructors.Length == 0) {
                throw new ArgumentException(String.Format("Could not find a public constructor for dependency injection for type {0}.", implementationType.FullName));
            } else if(candidateConstructors.Length > 1)
            {
                return CreateInstanceFromBestMatchConstructor(candidateConstructors);
            } else
            {
                return CreateInstanceFromConstructor(candidateConstructors[0]);
            }
        }

        Func<IServiceContainerExtended, Type[], object> CreateInstanceFromBestMatchConstructor(ConstructorInfo[] candidateConstructors)
        {
            return (IServiceContainerExtended services, Type[] callChain) =>
            {
                var selectedConstructor = GetPreferredConstructor(services, callChain, candidateConstructors);
                if(selectedConstructor == null)
                {
                    throw new ArgumentException(String.Format("Could not find a public constructor for dependency injection for type {0}.", candidateConstructors[0].DeclaringType.FullName));
                }
                return constructorFunctions.GetOrAdd(selectedConstructor, (ci) => CreateInstanceFromConstructor(selectedConstructor))(services, callChain);
            };
        }

        ConstructorInfo GetPreferredConstructor(IServiceContainerExtended services, Type[] callChain, ConstructorInfo[] candidateConstructors)
        {
            if(candidateConstructors == null || candidateConstructors.Length == 0) { throw new ArgumentException("Candidate constructors must be non-null and non-empty"); }
            //Sort by ascending number of parameters
            Array.Sort(candidateConstructors, (a, b) => a.GetParameters().Length - b.GetParameters().Length);

            ConstructorInfo selected = null;
            var resolvableTypes = new HashSet<Type>();
            for (int i = 0; i < candidateConstructors.Length; i++)
            {
                bool resolvable = true;
                var parameters = candidateConstructors[i].GetParameters();
                
                //In the event of same length, we already know the current one is fully resolvable, and we'll
                //arbitrarily say the tie goes to the first encountered, so there's no reason to evaluate
                //others with the same number of parameters.
                if(selected != null && parameters.Length <= selected.GetParameters().Length) { continue; }

                foreach(var p in parameters)
                {
                    if(!resolvableTypes.Contains(p.ParameterType) && !services.IsResolvable(p.ParameterType, callChain)) {
                        resolvable = false;
                        break;
                    } else
                    {
                        resolvableTypes.Add(p.ParameterType);
                    }
                }

                if (resolvable && (selected == null || selected.GetParameters().Length < candidateConstructors[i].GetParameters().Length))
                {
                    selected = candidateConstructors[i];
                }
            }

            return selected;
        }

        Func<IServiceContainerExtended, Type[], object> CreateInstanceFromConstructor(ConstructorInfo constructor)
        {
            Type implementationType = constructor.DeclaringType;
            var paramTypes = constructor.GetParameters();

            var paramList = new List<ParameterExpression>(paramTypes.Length);
            var initList = new List<Expression>(paramTypes.Length * 3);
            var nullExpr = Expression.Constant(null);

            var tempObjExpr = Expression.Variable(typeof(object));

            var containerExpr = Expression.Parameter(typeof(IServiceContainerExtended));
            var callChainExpr = Expression.Parameter(typeof(Type[]));

            Expression updateCallChainExpr = Expression.Call(AppendMethodInfo.MakeGenericMethod(typeof(Type)), callChainExpr, Expression.Constant(implementationType));
            updateCallChainExpr = Expression.Call(ToArrayMethodInfo.MakeGenericMethod(typeof(Type)), updateCallChainExpr);
            updateCallChainExpr = Expression.Assign(callChainExpr, updateCallChainExpr);

            foreach (var p in paramTypes)
            {
                var paramExpr = Expression.Parameter(p.ParameterType);
                
                var tempVarAssignmentExpr = Expression.Assign(tempObjExpr, Expression.Call(containerExpr, ServiceContainerGetServiceInternalMethod, Expression.Constant(p.ParameterType), callChainExpr));
                var initExpr = Expression.Assign(paramExpr,

                Expression.Convert(tempObjExpr, p.ParameterType));

                paramList.Add(paramExpr);
                initList.Add(tempVarAssignmentExpr);
                initList.Add(initExpr);

                var nullCheckExpr = Expression.IfThen(Expression.Equal(nullExpr, tempObjExpr),
                    Expression.Throw(Expression.New(MissingDependencyExceptionConstructor, Expression.Constant(implementationType), Expression.Constant(p.ParameterType), Expression.Convert(nullExpr, typeof(Exception)))));
                initList.Add(nullCheckExpr);


            }

            Expression newExpr = null;
            if(paramTypes.Length > 0)
            { newExpr = Expression.New(constructor, paramList.ToArray()); }
            else { newExpr = Expression.New(constructor); }

            var blockList = new List<Expression>(initList.Count + 2);
            blockList.Add(updateCallChainExpr);
            blockList.AddRange(initList);
            blockList.Add(newExpr);
            var blockExpr = Expression.Block(paramList.Append(tempObjExpr), blockList);
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
        struct WrapperServiceContainer : IServiceContainer, IServiceProvider
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
