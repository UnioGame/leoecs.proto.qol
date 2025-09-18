<p align="center">
    <img src="./logo.png" alt="Proto">
</p>

# LeoECS Proto QoL
A set of extensions for `LeoECS Proto` designed to improve developer "Quality of Life".

> **IMPORTANT!** Requires C#9 (or Unity >=2021.2).

> **IMPORTANT!** Depends on: [Leopotam.EcsProto](https://gitverse.ru/leopotam/ecsproto).

> **IMPORTANT!** Don't forget to use `DEBUG` builds for development and `RELEASE` builds for releases: all internal checks/exceptions will only work in `DEBUG` builds and are removed for performance in `RELEASE` builds.

> **IMPORTANT!** Tested on Unity 2021.3 (not dependent on it) and contains asmdef descriptions for compilation as separate assemblies and reducing main project recompilation time.


# Social Resources
Official blog: https://leopotam.ru


# Installation


## As Unity module
Installation as a Unity module via git link in PackageManager or direct editing of `Packages/manifest.json` is supported:
```
"ru.leopotam.ecsproto-qol": "https://gitverse.ru/leopotam/ecsproto-qol.git",
```


## As source code
The code can also be cloned or obtained as an archive from the releases page.


## Other sources
The official working version is hosted at [https://gitverse.ru/leopotam/ecsproto-qol](https://gitverse.ru/leopotam/ecsproto-qol), all other versions (including *nuget*, *npm* and other repositories) are unofficial clones or third-party code with unknown content.


# Iterators


## Initialization
Iterators have gained the ability to initialize in 3 ways (iterator for entities with components C1 and C2, but without C3):

```c#
ProtoItExc it1 = new (It.List (new C1 (), new C2 ()), It.List (new C3 ()));
ProtoItExc it2 = new (It.Inc<C1, C2> (), It.Exc<C3> ());
ProtoItExc it3 = It.Chain<C1> ().Inc<C2> ().Exc<C3> ().End ();
```

If you need an iterator without component exclusions, there is a separate type for this:

```c#
ProtoIt it1 = new (It.List (new C1 (), new C2 ()));
ProtoIt it2 = new (It.Inc<C1, C2> ());
ProtoIt it3 = It.Chain<C1> ().Inc<C2> ().End ();
```

> **IMPORTANT!** The recommended way is the first one, using `It.List()`. Generic variants are more concise, but with a large number of components and their combinations, they increase the executable file size.


## Caching Iterators
If it's known for certain that the data selection won't change and needs to be processed multiple times, you can use caching iterators:

```c#
// Create pool in the usual way, only the type differs.
ProtoItCached it1 = new (It.Inc<C1> ());
it1.Init (world);
// Cache data, pools are switched to ReadOnly.
it1.BeginCaching ();
foreach (ProtoEntity entity in it1) {
    ref C1 c1 = ref aspect1.C1Pool.Get (entity);
}
// Multiple passes through iterator it1.
// ...
// Disable caching, pools are switched to normal mode.
it1.EndCaching ();

// Similarly create pool with Exclude in the usual way, only the type differs.
ProtoItExcCached it2 = new (It.Inc<C1> (), It.Exc<C2> ());
it2.Init (world);
// Cache data, pools are switched to ReadOnly.
it2.BeginCaching ();
foreach (ProtoEntity entity in it2) {
    ref C1 c1 = ref aspect1.C1Pool.Get (entity);
}
// Multiple passes through iterator it2.
// ...
// Disable caching, pools are switched to normal mode.
it2.EndCaching ();
```

> **IMPORTANT!** If `it.BeginCaching()` is not called, an exception will be thrown in DEBUG version.

> **IMPORTANT!** If `it.BeginCaching()` was called, `it.EndCaching()` must be called after processing is complete, otherwise dependent pools will remain in ReadOnly mode. The calls don't have to be in the same method or even in the same processing loop if working with locked pools allows them to work in ReadOnly mode for an extended period.

> **IMPORTANT!** If processing in multiple passes is not required (e.g., only 1 iteration), the caching variant will work slower.

> **IMPORTANT!** If you need to check the iterator's working mode (cached or not), there is a method `it.IsCached()` for this.

If there's a need to get cached entities as a continuous array, you can use the following method:

```c#
ProtoItCached it3 = new (It.Inc<C1> ());
it3.Init (world);
it3.BeginCaching ();
(ProtoEntity[] entities, int len) = it3.CachedData();
// Entity cache is filled from 0 to len, if caching
// mode was not activated, then len will be 0.
// ...
// Process the obtained data.
// ...
it3.EndCaching ();
```

Caching iterators support sorting entities based on their component data:

```c#
struct C2 {
    public int Id;
}

ProtoItCached it4 = new (It.Inc<C2> ());
it4.Init (world);
// Cache data, pools are switched to ReadOnly.
it4.BeginCaching ();
// Sort entities in ascending order by values
// in C2.Id (any fields can be used, Id is here for example):
it4.Sort((ProtoEntity a, ProtoEntity b) => {
    return aspect1.C2Pool.Get (a).Id - aspect1.C2Pool.Get (b).Id;
});
// Work with sorted data.
foreach (ProtoEntity entity in it4) {
    ref C2 c2 = ref aspect1.C2Pool.Get (entity);
}
// Disable caching, pools are switched to normal mode.
it4.EndCaching ();
```

> **IMPORTANT!** To reduce allocations for sorting handlers, it's recommended to use local functions or system methods instead of lambdas.


# Entities


## Entity Packing
Entities are returned to user code as `ProtoEntity` identifiers and are only valid within
the current method - **cannot** store references to entities if there's no certainty that they cannot be
destroyed somewhere in the code.
If you need to save entities, they should be packed:

```c#
// Create a new entity in the world.
ProtoEntity entity = world.NewEntity ();
// Pack it for long-term storage outside the current method.
ProtoPackedEntity packed = world.PackEntity (entity);
// When the time comes - we can unpack it with simultaneous check for its existence.
if (packed.TryUnpack (world, out ProtoEntity unpackedEntity)) {
    // If the condition is true - entity is valid, we can work with it.
    world.DelEntity (unpackedEntity);
}
```

If multiple worlds are used and it's important to preserve binding to them, you can pack in another way:

```c#
// Create a new entity in the world.
ProtoEntity entity = world.NewEntity ();
// Pack it for long-term storage outside the current method.
ProtoPackedEntityWithWorld packed = world.PackEntityWithWorld (entity);
// When the time comes - we can unpack it with simultaneous check for its existence.
if (packed.TryUnpack (out ProtoWorld unpackedWorld, out ProtoEntity unpackedEntity)) {
    // If the condition is true - entity is valid, we can work with it.
    unpackedWorld.DelEntity (unpackedEntity);
}
```

For comparing two packed entities, use the `==` operator:

```c#
ProtoPackedEntity packedA = world.PackEntity (entity);
ProtoPackedEntity packedB = world.PackEntity (entity);
if (packedA == packedB) {
    // Packed entities are identical.
}
```

The same applies to packing entity with world:

```c#
ProtoPackedEntityWithWorld packedA = world.PackEntityWithWorld (entity);
ProtoPackedEntityWithWorld packedB = world.PackEntityWithWorld (entity);
if (packedA == packedB) {
    // Packed entities are identical.
}
```


## Emulation of entity API from classic version
> **IMPORTANT!** This API is significantly slower than the standard one and should not be used on a large number of entities.

Implemented through a special type `ProtoSlowEntity`:

```c#
ProtoPool<C1> pool;
ref C1 c1 = ref pool.NewSlowEntity (out ProtoSlowEntity entity);
if (entity.IsAlive ()) {
    ref C2 c2 = ref entity.Add<C2> ();
    ref C2 c2_1 = ref entity.Get<C2> ();
    ref C2 c2_2 = ref entity.GetOrAdd<C2> ();
    bool hasC2 = entity.Has<C2> ();
    entity.Del<C2> ();
    entity.DelEntity ();
}
```

It's also possible to convert any active `ProtoEntity` entity to `ProtoSlowEntity` and back:

```c#
ProtoEntity entity;
ProtoSlowEntity slowEntity = world.PackSlowEntity (entity);
if (slowEntity.IsAlive ()) {
    entity = slowEntity.Unpack ();
}
```

> **IMPORTANT!** Any operations with `ProtoSlowEntity` are only possible after checking its activity through `ProtoSlowEntity.IsAlive()`.

> **IMPORTANT!** The behavior of `ProtoSlowEntity.Add()` and `ProtoSlowEntity.Get()` differs from `EcsEntity.Get()` behavior in `LeoECS Classic` - if the requested component doesn't exist in the world, a pool for it won't be created, but an exception will be thrown. Only components registered through world aspects can be requested.

# Worlds


## Field Injection in Aspect
To reduce the amount of world aspect initialization code, you can inherit from a special type - field injection is supported for fields implementing `IProtoAspect`, `IProtoPool` and `IProtoIt`:

```c#
class Aspect1 : ProtoAspectInject {
    public Aspect2 Aspect2;
    public ProtoPool<C1> C1Pool;
    public ProtoPool<C2> C2Pool;
    public ProtoIt ItC1 = new (It.Inc<C1> ());
    public ProtoItExc ItInc1Exc2 = new (It.Inc<C1> (), It.Exc<C2> ());
}
```

This is identical to the following code:

```c#
class Aspect1 : IProtoAspect {
    public Aspect2 Aspect2;
    public ProtoPool<C1> C1Pool;
    public ProtoPool<C2> C2Pool;
    public ProtoIt ItC1 = new (It.Inc<C1> ());
    public ProtoItExc ItInc1Exc2 = new (It.Inc<C1> (), It.Exc<C2> ());
    ProtoWorld _world;

    public void Init (ProtoWorld world) {
        _world = world;
        world.AddAspect (this);
        Aspect2 ??= new ();
        Aspect2.Init (world);
        if (world.HasPool(typeof (C1))) {
            C1Pool = (ProtoPool<C1>) world.Pool (typeof (C1));
        } else {
            C1Pool = new ();
            world.AddPool (C1Pool);
        }
        if (world.HasPool(typeof (C2))) {
            C2Pool = (ProtoPool<C2>) world.Pool (typeof (C2));
        } else {
            C2Pool = new ();
            world.AddPool (C2Pool);
        }
    }
    public void PostInit () {
        ItC1.Init (_world);
        ItInc1Exc2.Init (_world);
    }
}
```

If additional custom initialization is needed, it can be performed through `Init()`/`PostInit()` method overrides:

```c#
class Aspect1 : ProtoAspectInject {
    public ProtoPool<C1> C1Pool;
    public ProtoPool<C2> C2Pool;
    // Iterator field must be initialized by the time of injection.
    public ProtoIt ItC1 = new (It.Inc<C1> ());
    public ProtoItExc ItInc1Exc2 = new (It.Inc<C1> (), It.Exc<C2> ());

    public override void Init (ProtoWorld world) {
        base.Init (world);
        // Additional initialization.
    }
    public override void PostInit () {
        base.PostInit ();
        // Additional initialization.
    }
}
```

Fields can be initialized with data instances before injection begins - in this case they will be
used for further configuration, this is one way to call custom constructors for pools and aspects.


To get a reference to the aspect's world, you can use a special method:

```c#
ProtoWorld world = new (new Aspect1 ());
Aspect1 aspect = (Aspect1) world.Aspect (typeof (Aspect1));
ProtoWorld aspectWorld = aspect.World ();
// aspectWorld and world contain a reference to the same instance.
```


## Creating Auto-Iterator from Aspect Pools
Automatic creation of an iterator from `ProtoAspectInject` pool fields is possible, for this they should be marked with special attributes:

```c#
class Aspect1 : ProtoAspectInject {
    // Auto-iterator will require component C1 to be present.
    [Include] public ProtoPool<C1> C1Pool;
    // Auto-iterator will require component C2 to be present.
    [Include] public ProtoPool<C2> C2Pool;
    // Auto-iterator will ignore all other pools without attributes.
    public ProtoPool<C3> C3Pool;
}
class Aspect2 : ProtoAspectInject {
    // Auto-iterator will require component C1 to be present.
    [Include] public ProtoPool<C1> C1Pool;
    // Auto-iterator will require component C2 to be present.
    [Include] public ProtoPool<C2> C2Pool;
    // Auto-iterator will require component C3 to be absent.
    [Exclude] public ProtoPool<C3> C3Pool;
    // Auto-iterator will ignore all other pools without attributes.
    public ProtoPool<C4> C4Pool;
}
```

The aspect's auto-iterator can be used as follows:

```c#
// If the aspect has no [Exclude] pools.
Aspect1 aspect1;
foreach (var entity in aspect1.Iter ()) {
    ref var c1 = ref aspect1.C1Pool.Get (entity);
}

// If the aspect has [Exclude] pools.
Aspect2 aspect2;
foreach (var entity in aspect2.IterExc ()) {
    ref var c1 = ref aspect2.C1Pool.Get (entity);
}
```

> **IMPORTANT!** Auto-iterator will only be created if the aspect has at least one pool marked with the `[Include]` attribute.


## List of Active Entities

```c#
Slice<ProtoEntity> items = new ();
world.AliveEntities (items);
for (int i = 0; i < items.Len (); i++) {
    ProtoEntity entity = items.Get (i);
}
```

If you only need to know the number of active entities (faster):

```c#
int count = world.GetAliveEntitiesCount ();
```


## List of Components on Entity

```c#
Slice<object> items = new ();
world.EntityComponents(entity, items);
for (int i = 0; i < items.Len (); i++) {
    object c = items.Get (i);
}
```


# Systems


## Field Injection in Systems
To inject into system fields, they just need to be marked with the `[DI]` attribute:
```c#
class TestSystem : IProtoInitSystem {
    // Field will be initialized with an instance of aspect with type Aspect1.
    [DI] Aspect1 _aspectDef;
    // Field will be initialized with an instance of aspect with type Aspect1 from world "events".
    [DI ("events")] Aspect1 _aspectEvt;
    // Field will be initialized with an instance of iterator
    // for components from the default world.
    [DI] ProtoIt _itDef = new (It.Inc<C1> ());
    // Field will be initialized with an instance of iterator
    // for components from world "events".
    [DI ("events")] ProtoIt _itEvt = new (It.Inc<C1> ());
    // Field will be initialized with an instance of service with type Service.
    [DI] Service1 _svc;

    public void Init (IProtoSystems systems) {
        // All fields are initialized, can work with them.
    }
}
```

> **IMPORTANT!** Iterator injection implies that its instance is already created through field initializer.

> **IMPORTANT!** `ProtoWorld` type injection is also supported through the `[DI]` attribute, but for optimization
> it's recommended to use `ProtoAspectInject.World()` call. Direct injection can be useful for
> initializing service fields.

For proper field injection in systems, a special module must be connected:

```c#
ProtoWorld world1 = new (new Aspect1 ());
ProtoWorld world2 = new (new Aspect2 ());
ProtoSystems systems = new (world1);
systems
    // Injection module.
    .AddModule (new AutoInjectModule ())

    .AddWorld (world2, "events")
    .AddSystem (new TestSystem ())
    .AddService (new Service1 ())
    .Init ();
```

> **IMPORTANT!** The injection module must come before registering other modules and systems that use it -
> it's easier to always put it first. The module needs to be connected only once for each group of systems.

There's a possibility to change the behavior of standard injection, for this a special service with handler specification should be connected:

```c#
// Injection handler. Returns operation success flag.
bool OnCustomInject (System.Reflection.FieldInfo fi, object target, IProtoSystems systems, string worldName) {
    // We want to override aspect injection.
    if (typeof (IProtoAspect).IsAssignableFrom (fi.FieldType)) {
        fi.SetValue (target, systems.World (worldName).Aspect (fi.FieldType));
        // All good, standard injection is not needed.
        return true;
    }
    // Pass control to standard injection.
    return false;
}
// ...
ProtoWorld world1 = new (new Aspect1 ());
ProtoSystems systems = new (world1);
systems
    // Injection module.
    .AddModule (new AutoInjectModule ())

    .AddSystem (new TestSystem ())
    .AddService (new Service1 ())
    // Connect custom handler.
    .AddService (new AutoInjectModule.Handler(OnCustomInject))
    .Init ();
```

Technically, injection into fields can be performed not only for systems, but for any objects through calling the `AutoInjectModule.Inject()` method:

```c#
class MyObject {
    [DI] readonly MyAspect _myAspect = default;
    [DI] readonly ProtoIt _myIt = new (It.Inc<C1> ());
}
// Get custom injection handler.
AutoInjectModule.Handler injectHandler = systems
    .Services ()
    .TryGetValue (typeof (AutoInjectModule.Handler), out var handlerRaw)
    ? (AutoInjectModule.Handler) handlerRaw
    : null;
MyObject obj = new ();
AutoInjectModule.Inject (obj, systems, injectHandler);
// Object instance fields will be initialized
// and ready for use here.
```

Injection into connected services can be performed automatically by specifying an optional flag in the `AutoInjectModule` constructor:

```c#
systems
    .AddModule (new AutoInjectModule (true))
    .Init ();
```


## Removing All Components of Required Type
Removal of components of a certain type can be automated at the required location:

```c#
ProtoWorld world1 = new (new Aspect1 ());
ProtoSystems systems = new (world1);
systems
    .AddSystem (new TestSystem1 ())
    // All components with type C1 will be removed here
    // from all active entities.
    .DelHere<C1> ()
    .AddSystem (new TestSystem2 ())
    .Init ();
```

`DelHere()` supports specifying the world for components to be removed and named call point through explicit specification of additional parameters.

If this extension method doesn't suit and you need to get the behavior as a system - you can use manual creation of `DelHereSystem` system instance - the `DelHere()` extension is a wrapper over it.


## Additional Service Initialization
Additional service initialization can be automated at the required location without creating additional systems:

```c#
ProtoWorld world1 = new (new Aspect1 ());
ProtoSystems systems = new (world1);
systems
    .AddSystem (new TestSystem1 ())
    // Service Service1 will be additionally
    // initialized here.
    .InitHere<Service1> ()
    .AddSystem (new TestSystem2 ())
    // The service to be initialized must be
    // registered in the services list.
    .AddService (new Service1 ())
    .Init ();
```

For a service to support additional initialization, it must implement a special interface:

```c#
class Service1 : IProtoInitService {
    public void Init (IProtoSystems systems) {
        // Additional initialization.
    }
}
```

`InitHere()` supports specifying a named call point through explicit specification of an additional parameter.

If this extension method doesn't suit and you need to get the behavior as a system - you can use manual creation of `InitHereSystem` system instance - the `InitHere()` extension is a wrapper over it.


## Additional Service Deinitialization
Additional service deinitialization can be automated at the required location without creating additional systems:

```c#
ProtoWorld world1 = new (new Aspect1 ());
ProtoSystems systems = new (world1);
systems
    .AddSystem (new TestSystem1 ())
    // Service Service1 will be additionally
    // deinitialized here.
    .DestroyHere<Service1> ()
    .AddSystem (new TestSystem2 ())
    // The service to be deinitialized must be
    // registered in the services list.
    .AddService (new Service1 ())
    .Init ();
```

For a service to support additional deinitialization, it must implement a special interface:

```c#
class Service1 : IProtoDestroyService {
    public void Destroy () {
        // Additional deinitialization.
    }
}
```

`DestroyHere()` supports specifying a named call point through explicit specification of an additional parameter.

If this extension method doesn't suit and you need to get the behavior as a system - you can use manual creation of `DestroyHereSystem` system instance - the `DestroyHere()` extension is a wrapper over it.

# Pools


## Request or Add Components
To request an existing component or add a new one if it doesn't exist, you can use the following method:

```c#
// If you need to know about component existence before the call.
ref C1 c1 = ref aspect1.C1Pool.GetOrAdd (e, out bool added);
// If you don't need to know about component existence before the call.
ref C1 c1 = ref aspect1.C1Pool.GetOrAdd (e);
```


## Safe Component Removal
To remove an arbitrary component on an entity, you need to perform a preliminary check for its existence, or use the following method:

```c#
aspect1.C1Pool.DelIfExists (e);
```


# Modules


## Initialization
By default, module aspects are registered in the world separately. To simplify simultaneous registration of aspects,
systems and services of a module, a special class can be used:

```c#
// All modules can be passed to the class constructor.
ProtoModules modules = new (
    new Module1 (),
    new Module2 (),
    new Module3 ());
// Or through a separate method.
modules.AddModule (new Module4 ());
// It's also possible to connect separate aspects outside modules.
modules.AddAspect (new Aspect1 ());
// Pass composite aspect to world constructor, which automatically
// includes aspects of all connected modules.
ProtoWorld world = new (modules.BuildAspect ())
ProtoSystems systems = new (world);
systems
    // Perform connection of composite module, which automatically
    // will register all modules connected to it.
    .AddModule (modules.BuildModule ())
    .Init ();
```

> **IMPORTANT!** If `ProtoModules` is used - registration of separate modules through `IProtoSystems.AddModule()`
> is not recommended, all modules should go through `ProtoModules` uniformly.


# Utilities


## Service Locator
A class instance can be saved globally:
```c#
UserSession sess = new ();
Service<UserSession>.Set (sess);
```

A globally saved class instance can be obtained anywhere:
```c#
UserSession sess = Service<UserSession>.Get ();
```

> **IMPORTANT!** If the data is no longer needed - the reference to them should be reset by passing `null` to the Set() method.


# License
The extension is released under the MIT-ZARYA license, [details here](./LICENSE.md).
