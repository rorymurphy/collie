# Collie
### Inversion of Control Container

## Purpose

Collie is a multi-tenant inversion of control container for .NET 5 & above. Multi-tenancy is supported by introducing the concept of a tenant key, that defines the
shared context which a given scope should use to resolve the TenantSingleton service lifetime (one shared instance per tenant).

## Usage

Collie offers interoperability with the standard .NET dependency injection API. However, to support the enhanced tenancy features, a native API is also provided.

For the most basic use case, a ServiceCollection can be populated, and an IServiceProvider built as shown below.

    int tenantId = 0;
    var services = new ServiceCollection();
    services.AddSingleton<IServiceA, DefaultServiceA>();
    services.AddTenantSingleton<IServiceB, DefaultServiceB>();
    services.AddScoped<Tuple<int>>(container => new Tuple<int>(tenantId++));

    IServiceContainer rootContainer = services.BuildCollieProvider(catalog, container => container.GetService<Tuple<int>>(), typeof(Tuple<int>));

    var scopedProvider = rootContainer.CreateScope().ServiceProvider;

For more advanced usage via the Collie API, see the unit test suite.

## Scalability

One of the key challenges that arises when dealing with (potentially massive) multi-tenancy is determining exactly how long the TenantSingleton lifetime should
persist. In the nominal case, where there are relatively few clients, TenantSingletons should live effectively as long as a Singleton, which is to say until the
root IServiceProvider is disposed. However, as the number of tenants rises, so too does the memory overhead required to retain all TenantSingletons. Consequently,
at some point, it becomes desireable to dispose of tenant state not currently in active use. Using Collie's native API, this can be achieved by specifying a
TenantCacheSize. This controls the desired number of tenant contexts to retain in memory when no requests are active. When requests are active, the actual number
retained may be higher if needed to service all active requests.

## Background

While Collie derives its name from the breed known for their ability to fetch, it also pays tribute to a childhood friend who later tragically disappeared under
suspicious circumstances. Still hoping you're out there and find your way home.