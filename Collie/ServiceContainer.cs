using Collie.Abstractions;
using Collie.Compatibility;
using Collie.ServiceLookup;
using Collie.ServiceLookup.Expressions;
using ServiceLifetime = Collie.Abstractions.ServiceLifetime;
using MSFTDI = Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collie
{
    class ServiceContainer : IServiceContainerExtended, IServiceProvider, IDisposable
    {
        private static readonly Type IEnumerableType = typeof(IEnumerable<>);
        private static readonly Type IServiceFactoryGeneratorType = typeof(IServiceFactoryGenerator);
        private static readonly Type IServiceContainerType = typeof(IServiceContainer);
        private static readonly Type IServiceProviderType = typeof(IServiceProvider);
        private static readonly Type IScopeBuilderType = typeof(IScopeBuilder);
        private static readonly Type ServiceCreatorCacheType = typeof(ServiceCreatorCache);
        private static readonly Type ITenantManagerType = typeof(ITenantManager);
        private static readonly Type IServiceScopeFactoryType = typeof(MSFTDI.IServiceScopeFactory);

        private bool disposedValue;

        protected IServiceCatalog services;

        protected IDictionary<Type, ServiceDefinition> serviceDefinitionsByType = new Dictionary<Type, ServiceDefinition>();

        protected ConcurrentDictionary<Type, object> resolvedServices = new ConcurrentDictionary<Type, object>();

        protected ConcurrentDictionary<ServiceIdentifier, object> resolvedImplementations = new ConcurrentDictionary<ServiceIdentifier, object>();

        protected IList<object> disposables = new List<object>();

        protected IServiceFactoryGenerator serviceFactoryGenerator;

        protected ServiceCreatorCache serviceCreatorCache;

        protected ITenantManager tenantManager;

        protected IScopeBuilder scopeBuilder;

        protected ServiceContainer rootContainer;

        protected ServiceContainer tenantContainer;

        protected Func<IServiceContainer, object> tenantKeySelector;

        protected Type tenantKeyType;

        protected object tenantKey;

        protected MSFTDI.IServiceScopeFactory serviceScopeFactory;

        protected ServiceLifetime containerType;

        protected internal bool IsRootContainer { get { return containerType == ServiceLifetime.Singleton; } }

        protected internal bool IsTenantContainer { get { return containerType == ServiceLifetime.TenantSingleton; } }

        protected internal bool IsScopeContainer { get { return containerType == ServiceLifetime.Scoped; } }

        private static readonly object SingleTenantKey = new object();

        public int TenantCacheSize { get; init; }

        public uint MaxTenantSize { get; init; }

        public bool IgnoreUnresolvableEnumerables { get; init; } = true;

        public bool ContextualOverrides { get; private set; }

        public ServiceContainer(IServiceCatalog services, bool contextualOverrides = false) : this(services, (container) => SingleTenantKey, typeof(object), contextualOverrides) { }

        //Root container initialization
        public ServiceContainer(IServiceCatalog services, Func<IServiceContainer, object> keySelector, Type keyType, bool contextualOverrides = false)
            : this(ServiceLifetime.Singleton, services, null, keySelector, keyType, null, contextualOverrides)
        { }

        //Tenant singleton container initialization
        internal ServiceContainer(IServiceCatalog services, ServiceContainer rootContainer, Func<IServiceContainer, object> keySelector, Type keyType, object key, bool contextualOverrides = false)
            : this(ServiceLifetime.TenantSingleton, services, rootContainer, keySelector, keyType, key, contextualOverrides)
        { }

        //Scoped container initialization
        internal ServiceContainer(IServiceCatalog services, ServiceContainer rootContainer, Func<IServiceContainer, object> keySelector, Type keyType, bool contextualOverrides = false)
            : this(ServiceLifetime.Scoped, services, rootContainer, keySelector, keyType, null, contextualOverrides)
        { }

        private ServiceContainer(ServiceLifetime containerType, IServiceCatalog services, ServiceContainer rootContainer, Func<IServiceContainer, object> keySelector, Type keyType, object key, bool contextualOverrides = false)
        {
            this.services = services;
            this.tenantKeySelector = keySelector;
            this.tenantKeyType = keyType;
            this.tenantKey = key;

            this.rootContainer = rootContainer;
            this.containerType = containerType;
            this.ContextualOverrides = contextualOverrides;

            this.Initialize();
        }


        protected internal void Initialize()
        {
            serviceFactoryGenerator = IsRootContainer ? new ExpressionServiceFactory() : (IServiceFactoryGenerator)rootContainer.GetService(IServiceFactoryGeneratorType);
            serviceCreatorCache = IsRootContainer ? new ServiceCreatorCache() : (ServiceCreatorCache)rootContainer.GetService(ServiceCreatorCacheType);
            scopeBuilder = IsRootContainer ? new DefaultScopeBuilder(services, this, tenantKeySelector, tenantKeyType) : (IScopeBuilder)rootContainer.GetService(IScopeBuilderType);
            tenantManager = IsRootContainer ? new DefaultTenantManager(services, this, tenantKeySelector, tenantKeyType, TenantCacheSize) : (ITenantManager)rootContainer.GetService(ITenantManagerType);
            serviceScopeFactory = IsRootContainer ? new ServiceScopeFactory(scopeBuilder) : (MSFTDI.IServiceScopeFactory)rootContainer.GetService(IServiceScopeFactoryType);

            //Handles the case of multiple registrations, where the last one takes precedence, but for IEnumerable<T> need to keep all registrations.
            if (ContextualOverrides)
            {
                foreach (var svc in services)
                {
                    if (!serviceDefinitionsByType.ContainsKey(svc.ServiceType) || GetLiftetimeResolution(svc.Lifetime) != ServiceLifetimeResolution.Unresolvable)
                    {
                        serviceDefinitionsByType[svc.ServiceType] = svc;
                    }
                }
            }
            else
            {
                foreach (var svc in services)
                {
                    serviceDefinitionsByType[svc.ServiceType] = svc;
                }
            }

            if (IsScopeContainer)
            {
                tenantKey = tenantKeySelector(this);
                tenantContainer = tenantManager.CaptureTenant(tenantKey);
            }
        }

        public object GetService(Type serviceType)
        {
            return GetServiceInternal(serviceType, new Type[0]);
        }

        public virtual object GetServiceInternal(Type serviceType, Type[] callChain)
        {
            if (serviceType == null) { throw new ArgumentNullException(nameof(serviceType)); }
            else if (serviceType == IServiceContainerType) { return this; }
            else if (serviceType == IServiceProviderType) { return this; }
            else if (serviceType == IScopeBuilderType) { return scopeBuilder; }
            else if (serviceType == IServiceScopeFactoryType) { return serviceScopeFactory; }
            else if (serviceType == IServiceFactoryGeneratorType) { return serviceFactoryGenerator; }
            else if (serviceType == ServiceCreatorCacheType) { return serviceCreatorCache; }
            else if (serviceType == ITenantManagerType) { return tenantManager; }
            else if (serviceType.IsInterface && serviceType.IsGenericTypeDefinition) { throw new ArgumentException("When requeing a generic typed service, request must refer to a fully specified type."); }

            if (callChain.Contains(serviceType))
            {
                throw new CircularDependencyException(callChain);
            }

            if (resolvedServices.ContainsKey(serviceType)) { return resolvedServices[serviceType]; }

            Type genericType = null;
            ServiceDefinition definition = null;
            if (serviceDefinitionsByType.ContainsKey(serviceType))
            {
                definition = serviceDefinitionsByType[serviceType];
            }
            else if (serviceType.IsInterface && serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() == IEnumerableType)
            {
                definition = new ServiceDefinition(serviceType, this.containerType, serviceType);
            }
            else if (serviceType.IsGenericType && (genericType = serviceType.GetGenericTypeDefinition()) != null && serviceDefinitionsByType.ContainsKey(genericType))
            {
                definition = serviceDefinitionsByType[genericType];
            }
            else
            {
                throw new Exception(String.Format("Unable to find a service definition matching {0}", serviceType.FullName));
            }

            var lifetimeResolution = GetLiftetimeResolution(definition.Lifetime);
            var identifier = new ServiceIdentifier(serviceType, definition);
            switch (lifetimeResolution)
            {
                case ServiceLifetimeResolution.Unresolvable:
                    return null;
                case ServiceLifetimeResolution.Delegated:
                    return GetDelegatedImplementation(identifier, callChain);
                case ServiceLifetimeResolution.Direct:
                default:
                    //Transients do not get re-used.
                    if (definition.Lifetime == ServiceLifetime.Transient)
                    {
                        return CreateLocalService(identifier, callChain);
                    }
                    else
                    {
                        resolvedServices.GetOrAdd(serviceType, key => this.GetLocalImplementation(identifier, callChain));
                        return resolvedServices[serviceType];
                    }
            }
        }

        public IEnumerable<ServiceIdentifier> GetImplementationTypes(Type serviceType)
        {
            var genericType = serviceType.IsGenericType ? serviceType.GetGenericTypeDefinition() : null;
            return services.Where(sd => {
                var lifetimeResolution = GetLiftetimeResolution(sd.Lifetime);
                if(lifetimeResolution == ServiceLifetimeResolution.Unresolvable && !IgnoreUnresolvableEnumerables)
                {
                    throw new UnresolvableDependencyException(typeof(IEnumerable<>).MakeGenericType(serviceType), sd.ServiceType);
                }
                return (lifetimeResolution != ServiceLifetimeResolution.Unresolvable)
                     && (sd.ServiceType == serviceType || (genericType != null && sd.ServiceType == genericType));
            }).Select(sd => new ServiceIdentifier(serviceType, sd));
        }

        public object GetImplementation(ServiceIdentifier identifier, Type[] callChain)
        {
            var lifetimeResolution = GetLiftetimeResolution(identifier.Lifetime);
            switch (lifetimeResolution)
            {
                case ServiceLifetimeResolution.Unresolvable:
                    return null;
                case ServiceLifetimeResolution.Delegated:
                    return GetDelegatedImplementation(identifier, callChain);
                case ServiceLifetimeResolution.Direct:
                default:
                    return GetLocalImplementation(identifier, callChain);
            }
        }

        protected internal object GetLocalImplementation(ServiceIdentifier identifier, Type[] callChain)
        {
            resolvedImplementations.GetOrAdd(identifier, key => CreateLocalService(key, callChain));
            return resolvedImplementations[identifier];
        }

        protected object CreateLocalService(ServiceIdentifier identifier, Type[] callChain)
        {
            object result = null;
            if (identifier.Kind == ServiceCreatorKind.Constant)
            {
                result = identifier.Distinguisher;
            } else {
                var factory = serviceCreatorCache.GetOrAdd(identifier, key =>
                {
                    Func<IServiceContainerExtended, Type[], object> innerFactory = null;
                    switch(identifier.Kind)
                    {
                        case ServiceCreatorKind.Factory:
                            innerFactory = serviceFactoryGenerator.CreateFactory(identifier.ServiceType, (Func<IServiceContainer, object>)identifier.Distinguisher);
                            break;
                        case ServiceCreatorKind.Generic:
                            var genericTypeDef = ((Type)identifier.Distinguisher);
                            if (identifier.ServiceType.GetGenericArguments().Length != genericTypeDef.GetGenericArguments().Length)
                            {
                                throw new Exception("Service type and implementation type generic type parameter count mismatch, unable to infer implementation type parameters.");
                            }
                            var concreteType = ((Type)identifier.Distinguisher).MakeGenericType(identifier.ServiceType.GetGenericArguments());
                            innerFactory = serviceFactoryGenerator.CreateFactory(concreteType);
                            break;
                        default:
                            innerFactory = serviceFactoryGenerator.CreateFactory((Type)identifier.Distinguisher);
                            break;
                    }

                    return innerFactory;
                });

                result = factory(this, callChain);
                //Need to add it to the disposables here to ensure transients are also caught.
                //Transients are not disposed by the container, as the instance may be reused
                //elsewhere, hence they are not added here.
                this.disposables.Add(result);
            }

            return result;
        }

        protected internal enum ServiceLifetimeResolution { Direct, Delegated, Unresolvable };
        protected internal ServiceLifetimeResolution GetLiftetimeResolution(ServiceLifetime lifetime)
        {
            switch (lifetime)
            {
                case ServiceLifetime.Transient:
                    return ServiceLifetimeResolution.Direct;
                case ServiceLifetime.Singleton:
                    return IsRootContainer ? ServiceLifetimeResolution.Direct : ServiceLifetimeResolution.Delegated;
                case ServiceLifetime.TenantSingleton:
                    if (IsRootContainer) { return ServiceLifetimeResolution.Unresolvable; }
                    else if (IsTenantContainer) { return ServiceLifetimeResolution.Direct; }
                    else { return ServiceLifetimeResolution.Delegated; }
                default:
                    return (IsRootContainer || IsTenantContainer) ? ServiceLifetimeResolution.Unresolvable : ServiceLifetimeResolution.Direct;
            }
        }

        protected internal object GetDelegatedImplementation(ServiceIdentifier identifier, Type[] callChain)
        {
            switch (identifier.Lifetime)
            {
                case ServiceLifetime.Singleton:
                    return rootContainer.GetImplementation(identifier, callChain);
                case ServiceLifetime.TenantSingleton:
                    return tenantContainer.GetImplementation(identifier, callChain);
                default:
                    throw new ArgumentException("Service lifetime not delegated.");

            }
        }

        protected internal virtual void Disown(object instance)
        {
            this.disposables.Remove(instance);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                foreach (var disposable in this.resolvedImplementations.Values.Select(d => d as IDisposable).Where(d => d != null))
                {
                    disposable.Dispose();
                }
                if (IsTenantContainer)
                {
                    tenantManager.ReleaseTenant(this.tenantKey);
                    tenantContainer = null;
                }
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~ServiceContainer()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public bool IsResolvable(Type serviceType, Type[] callChain)
        {
            if (serviceType == null) { throw new ArgumentNullException(nameof(serviceType)); }
            if (serviceType.IsInterface && serviceType.IsGenericTypeDefinition) { return false; }
            if (callChain.Contains(serviceType))
            {
                return false;
            }

            if ( resolvedServices.ContainsKey(serviceType)
                || serviceType == IServiceContainerType
                || serviceType == IServiceProviderType
                || serviceType == IScopeBuilderType
                || serviceType == IServiceScopeFactoryType
                || serviceType == IServiceFactoryGeneratorType
                || serviceType == ServiceCreatorCacheType
                || serviceType == ITenantManagerType
                || serviceType.IsInterface && serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() == IEnumerableType) { return true; }




            Type genericType = null;
            ServiceDefinition definition = null;
            if (serviceDefinitionsByType.ContainsKey(serviceType))
            {
                definition = serviceDefinitionsByType[serviceType];
            }
            else if (serviceType.IsGenericType && (genericType = serviceType.GetGenericTypeDefinition()) != null && serviceDefinitionsByType.ContainsKey(genericType))
            {
                definition = serviceDefinitionsByType[genericType];
            }
            else
            {
                return false;
            }

            var lifetimeResolution = GetLiftetimeResolution(definition.Lifetime);
            return lifetimeResolution != ServiceLifetimeResolution.Unresolvable;
        }
    }
}
