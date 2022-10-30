using Collie.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collie.ServiceLookup
{
    class DefaultTenantManager : ITenantManager
    {
        protected static readonly Type IServiceCatalogType = typeof(IServiceCatalog);

        protected ServiceContainer rootContainer;

        protected IServiceCatalog services;

        protected Func<IServiceContainer, object> keySelector;

        protected Type keyType;

        protected IDictionary<object, Tuple<int, ServiceContainer>> activeServiceContainers = new Dictionary<object, Tuple<int, ServiceContainer>>();

        protected HashSet<object> evictableKeys = new HashSet<object>();

        protected int targetTenantCount = 0;
        protected uint maxTenantCount = 0;

        protected ConcurrentDictionary<object, object> tenantLocks = new ConcurrentDictionary<object, object>();

        public DefaultTenantManager(IServiceCatalog services, ServiceContainer rootContainer, Func<IServiceContainer, object> keySelector, Type keyType, int targetTenantCount = 0, uint maxTenantCount = 0)
        {
            this.services = services;
            this.rootContainer = rootContainer;
            this.keySelector = keySelector;
            this.keyType = keyType;
            this.targetTenantCount = targetTenantCount;
            this.maxTenantCount = maxTenantCount;
        }

        public ServiceContainer CaptureTenant(object key, ServiceContainerOptions serviceContainerOptions)
        {
            Tuple<int, ServiceContainer> entry = null;
            var lockObj = new object();

            var tLock = tenantLocks.GetOrAdd(key, lockObj);
            while (tLock != lockObj)
            {
                //Wait for the other caller to finish, then try again
                lock (tLock) { }
                tLock = tenantLocks.GetOrAdd(key, lockObj);
            }

            lock (tLock)
            {
                var success = activeServiceContainers.TryGetValue(key, out entry);
                evictableKeys.Remove(key);
                if (!success && maxTenantCount > 0 && activeServiceContainers.Count >= maxTenantCount)
                {
                    throw new TenantLimitExceededException(key);
                } else if (!success)
                {
                    var tenantContainer = new ServiceContainer(services, rootContainer, keySelector, keyType, key, serviceContainerOptions);
                    activeServiceContainers.Add(key, new Tuple<int, ServiceContainer>(1, tenantContainer));
                } else
                {
                    var updated = new Tuple<int, ServiceContainer>(entry.Item1 + 1, entry.Item2);
                    activeServiceContainers[key] = updated;
                }
            }
            while (!tenantLocks.TryRemove(key, out lockObj)) { }

            return activeServiceContainers[key].Item2;
        }

        public void ReleaseTenant(object key)
        {
            Tuple<int, ServiceContainer> entry = null;
            var lockObj = new object();
            var success = activeServiceContainers.TryGetValue(key, out entry);
            if (success)
            {
                var tLock = tenantLocks.GetOrAdd(key, lockObj);
                while (tLock != lockObj)
                {
                    //Wait for the other caller to finish, then try again
                    lock (tLock) { }
                    tLock = tenantLocks.GetOrAdd(key, lockObj);
                }

                lock (tLock)
                {
                    success = activeServiceContainers.TryGetValue(key, out entry);
                    //Could have been removed while acquiring lock
                    if (success)
                    {
                        var updated = new Tuple<int, ServiceContainer>(Math.Max(0, entry.Item1 - 1), entry.Item2);
                        activeServiceContainers[key] = updated;
                        if (updated.Item1 == 0) { evictableKeys.Add(key); }
                    }

                    while (activeServiceContainers.Count > targetTenantCount && evictableKeys.Count > 0)
                    {
                        var toRemove = evictableKeys.First();
                        evictableKeys.Remove(toRemove);
                        activeServiceContainers.Remove(toRemove);

                    }
                }

                while (!tenantLocks.TryRemove(key, out lockObj)) { }
            }
        }
    }
}
