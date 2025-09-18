// ----------------------------------------------------------------------------
// Лицензия MIT-ZARYA
// (c) 2025 Leopotam <leopotam@yandex.ru>
// ----------------------------------------------------------------------------

using System;
using System.Runtime.CompilerServices;
#if ENABLE_IL2CPP
using Unity.IL2CPP.CompilerServices;
#endif

namespace Leopotam.EcsProto.QoL {
#if ENABLE_IL2CPP
    [Il2CppSetOption (Option.NullChecks, false)]
    [Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
    public struct ProtoPackedEntity : IEquatable<ProtoPackedEntity> {
        public short Gen;
        public ProtoEntity Id;

        public override int GetHashCode () {
            unchecked {
                return (23 * 31 + Id.GetHashCode ()) * 31 + Gen;
            }
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public bool Equals (ProtoPackedEntity rhs) => Id.Equals (rhs.Id) && Gen == rhs.Gen;

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static bool operator == (ProtoPackedEntity lhs, ProtoPackedEntity rhs) {
            return lhs.Id.Equals (rhs.Id) && lhs.Gen == rhs.Gen;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static bool operator != (ProtoPackedEntity lhs, ProtoPackedEntity rhs) {
            return !lhs.Id.Equals (rhs.Id) || lhs.Gen != rhs.Gen;
        }

        public override bool Equals (Object obj) {
            return obj is ProtoPackedEntity rhs && this == rhs;
        }
    }

#if ENABLE_IL2CPP
    [Il2CppSetOption (Option.NullChecks, false)]
    [Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
    public struct ProtoPackedEntityWithWorld : IEquatable<ProtoPackedEntityWithWorld> {
        public short Gen;
        public ProtoEntity Id;
        public ProtoWorld World;

        public override int GetHashCode () {
            unchecked {
                return ((23 * 31 + Id.GetHashCode ()) * 31 + Gen) * 31 + (World?.GetHashCode () ?? 0);
            }
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public bool Equals (ProtoPackedEntityWithWorld rhs) => Id.Equals (rhs.Id) && Gen == rhs.Gen && World == rhs.World;

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static bool operator == (ProtoPackedEntityWithWorld lhs, ProtoPackedEntityWithWorld rhs) {
            return lhs.Id.Equals (rhs.Id) && lhs.Gen == rhs.Gen && lhs.World == rhs.World;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static bool operator != (ProtoPackedEntityWithWorld lhs, ProtoPackedEntityWithWorld rhs) {
            return !lhs.Id.Equals (rhs.Id) || lhs.Gen != rhs.Gen || lhs.World != rhs.World;
        }

        public override bool Equals (Object obj) {
            return obj is ProtoPackedEntityWithWorld rhs && this == rhs;
        }
    }

#if ENABLE_IL2CPP
    [Il2CppSetOption (Option.NullChecks, false)]
    [Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
    public struct ProtoSlowEntity : IEquatable<ProtoSlowEntity> {
        internal short _gen;
        internal ProtoEntity _id;
        internal ProtoWorld _world;

        public override int GetHashCode () {
            unchecked {
                return ((23 * 31 + _id.GetHashCode ()) * 31 + _gen) * 31 + (_world?.GetHashCode () ?? 0);
            }
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public bool Equals (ProtoSlowEntity rhs) => _id.Equals (rhs._id) && _gen == rhs._gen && _world == rhs._world;

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static bool operator == (ProtoSlowEntity lhs, ProtoSlowEntity rhs) {
            return lhs._id.Equals (rhs._id) && lhs._gen == rhs._gen && lhs._world == rhs._world;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static bool operator != (ProtoSlowEntity lhs, ProtoSlowEntity rhs) {
            return !lhs._id.Equals (rhs._id) || lhs._gen != rhs._gen || lhs._world != rhs._world;
        }

        public override bool Equals (Object obj) => obj is ProtoSlowEntity rhs && this == rhs;
    }

#if ENABLE_IL2CPP
    [Il2CppSetOption (Option.NullChecks, false)]
    [Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
    public static class ProtoEntityExtensions {
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static ProtoPackedEntity PackEntity (this ProtoWorld world, ProtoEntity entity) {
            ProtoPackedEntity packed;
            packed.Id = entity;
            packed.Gen = world.EntityGen (entity);
            return packed;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static ProtoPackedEntityWithWorld PackEntityWithWorld (this ProtoWorld world, ProtoEntity entity) {
            ProtoPackedEntityWithWorld packed;
            packed.Id = entity;
            packed.Gen = world.EntityGen (entity);
            packed.World = world;
            return packed;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static bool TryUnpack (this in ProtoPackedEntity packed, ProtoWorld world, out ProtoEntity entity) {
            entity = packed.Id;
            return world != null && world.IsAlive () && world.EntityGen (packed.Id) == packed.Gen;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static bool TryUnpack (this in ProtoPackedEntityWithWorld packed, out ProtoWorld world, out ProtoEntity entity) {
            entity = packed.Id;
            world = packed.World;
            return world != null && world.IsAlive () && world.EntityGen (packed.Id) == packed.Gen;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static ProtoSlowEntity PackSlowEntity (this ProtoWorld world, ProtoEntity entity) {
            ProtoSlowEntity slowE;
            slowE._id = entity;
            slowE._gen = world.EntityGen (entity);
            slowE._world = world;
            return slowE;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static ref T NewSlowEntity<T> (this ProtoPool<T> pool, out ProtoSlowEntity slowEntity) where T : struct {
            pool.NewEntity (out var entity);
            slowEntity._id = entity;
            slowEntity._world = pool.World ();
            slowEntity._gen = slowEntity._world.EntityGen (entity);
            return ref pool.Get (entity);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static bool IsAlive (this ProtoSlowEntity entity) {
            return entity._world != null && entity._world.IsAlive () && entity._world.EntityGen (entity._id) == entity._gen;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static ProtoEntity Unpack (this ProtoSlowEntity entity) {
#if DEBUG
            if (!entity.IsAlive ()) { new Exception ("не могу получить доступ к удаленной сущности"); }
#endif
            return entity._id;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static ref T Add<T> (this ProtoSlowEntity entity) where T : struct {
#if DEBUG
            if (!entity.IsAlive ()) { new Exception ("не могу получить доступ к удаленной сущности"); }
#endif
            return ref ((ProtoPool<T>) entity._world.Pool (typeof (T))).Add (entity._id);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static ref T Get<T> (this ProtoSlowEntity entity) where T : struct {
#if DEBUG
            if (!entity.IsAlive ()) { new Exception ("не могу получить доступ к удаленной сущности"); }
#endif
            return ref ((ProtoPool<T>) entity._world.Pool (typeof (T))).Get (entity._id);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static ref T GetOrAdd<T> (this ProtoSlowEntity entity) where T : struct {
#if DEBUG
            if (!entity.IsAlive ()) { new Exception ("не могу получить доступ к удаленной сущности"); }
#endif
            return ref ((ProtoPool<T>) entity._world.Pool (typeof (T))).GetOrAdd (entity._id);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static bool Has<T> (this ProtoSlowEntity entity) where T : struct {
#if DEBUG
            if (!entity.IsAlive ()) { new Exception ("не могу получить доступ к удаленной сущности"); }
#endif
            return ((ProtoPool<T>) entity._world.Pool (typeof (T))).Has (entity._id);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static void Del<T> (this ProtoSlowEntity entity) where T : struct {
#if DEBUG
            if (!entity.IsAlive ()) { new Exception ("не могу получить доступ к удаленной сущности"); }
#endif
            ((ProtoPool<T>) entity._world.Pool (typeof (T))).Del (entity._id);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static void DelEntity (this ProtoSlowEntity entity) {
#if DEBUG
            if (!entity.IsAlive ()) { new Exception ("не могу получить доступ к удаленной сущности"); }
#endif
            entity._world.DelEntity (entity._id);
        }
    }
}
