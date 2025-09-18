// ----------------------------------------------------------------------------
// Лицензия MIT-ZARYA
// (c) 2025 Leopotam <leopotam@yandex.ru>
// ----------------------------------------------------------------------------

using System.Runtime.CompilerServices;
#if ENABLE_IL2CPP
using Unity.IL2CPP.CompilerServices;
#endif

namespace Leopotam.EcsProto.QoL {
#if ENABLE_IL2CPP
    [Il2CppSetOption (Option.NullChecks, false)]
    [Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
    public static class ProtoPoolExtensions {
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static ref T GetOrAdd<T> (this ProtoPool<T> pool, ProtoEntity entity, out bool added) where T : struct {
            added = !pool.Has (entity);
            return ref added ? ref pool.Add (entity) : ref pool.Get (entity);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static ref T GetOrAdd<T> (this ProtoPool<T> pool, ProtoEntity entity) where T : struct {
            return ref pool.Has (entity) ? ref pool.Get (entity) : ref pool.Add (entity);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static ref T NewEntity<T> (this ProtoPool<T> pool) where T : struct {
            pool.NewEntity (out var e);
            return ref pool.Get (e);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static void DelIfExists<T> (this ProtoPool<T> pool, ProtoEntity entity) where T : struct {
            if (pool.Has (entity)) {
                pool.Del (entity);
            }
        }
    }
}
