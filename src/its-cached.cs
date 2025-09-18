// ----------------------------------------------------------------------------
// Лицензия MIT-ZARYA
// (c) 2025 Leopotam <leopotam@yandex.ru>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
#if ENABLE_IL2CPP
using Unity.IL2CPP.CompilerServices;
#endif

namespace Leopotam.EcsProto.QoL {
    public delegate int SortCachingCallback (ProtoEntity a, ProtoEntity b);

#if ENABLE_IL2CPP
    [Il2CppSetOption (Option.NullChecks, false)]
    [Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
    sealed class ProtoEntityComparer : IComparer<ProtoEntity> {
        public SortCachingCallback Handler;
        public int Compare (ProtoEntity x, ProtoEntity y) => Handler (y, x);
    }

#if ENABLE_IL2CPP
    [Il2CppSetOption (Option.NullChecks, false)]
    [Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
    public sealed class ProtoItCached : IProtoIt {
        readonly ProtoIt _it;
        readonly ProtoEntityComparer _comparer;
        ProtoEntity[] _cachedEntities;
        int _cachedLen;
#if DEBUG
        bool _inited;
        bool _cached;

        // обслуживается внутренним итератором.
        void IProtoIt.AddBlocker (int amount) { }
#endif

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public ProtoItCached (Type[] iTypes) {
#if DEBUG
            if (iTypes == null || iTypes.Length < 1) { throw new Exception ("некорректный список include-пулов для инициализации итератора"); }
#endif
            _it = new (iTypes);
            _comparer = new ();
            _cachedEntities = Array.Empty<ProtoEntity> ();
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public IProtoIt Init (ProtoWorld world) {
            _it.Init (world);
#if DEBUG
            _inited = true;
#endif
            return this;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public void BeginCaching () {
#if DEBUG
            if (!_inited) { throw new Exception ("итератор не инициализирован"); }
            if (_cached) { throw new Exception ("итератор уже закеширован"); }
#endif
            var cap = _it.World ().EntityGens ().Cap ();
            if (_cachedEntities.Length != cap) {
                _cachedEntities = new ProtoEntity[cap];
            }
            _cachedLen = 0;
            foreach (var e in _it) {
                _cachedEntities[_cachedLen++] = e;
            }
#if DEBUG
            _it.AddBlocker (2);
            _cached = true;
#endif
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public void EndCaching () {
#if DEBUG
            if (!_inited) { throw new Exception ("итератор не инициализирован"); }
            if (!_cached) { throw new Exception ("итератор не закеширован"); }
            _it.AddBlocker (-2);
            _cached = false;
#endif
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public void Sort (SortCachingCallback sortCb) {
#if DEBUG
            if (sortCb == null) { throw new Exception ("нет обработчика для сортировки"); }
            if (!_inited) { throw new Exception ("итератор не инициализирован"); }
            if (!_cached) { throw new Exception ("итератор не закеширован"); }
#endif
            if (_cachedLen > 0) {
                _comparer.Handler = sortCb;
                Array.Sort (_cachedEntities, 0, _cachedLen, _comparer);
                _comparer.Handler = null;
            }
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public ItEnumerator GetEnumerator () => new (this);

        ProtoItIEnumerator IProtoIt.GetEnumerator () => new (this, _it.IncMaskIndices (), null);

        public ref struct ItEnumerator {
            readonly ProtoEntity[] _entities;
            int _id;

            [MethodImpl (MethodImplOptions.AggressiveInlining)]
            public ItEnumerator (ProtoItCached it) {
#if DEBUG
                if (!it._inited) { throw new Exception ("итератор не инициализирован"); }
                if (!it._cached) { throw new Exception ("итератор не закеширован"); }
#endif
                _entities = it._cachedEntities;
                _id = it._cachedLen;
            }

            public ProtoEntity Current {
                [MethodImpl (MethodImplOptions.AggressiveInlining)]
                get => _entities[_id];
            }

            [MethodImpl (MethodImplOptions.AggressiveInlining)]
            public bool MoveNext () {
                _id--;
                return _id >= 0;
            }
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public ProtoWorld World () => _it.World ();

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public bool Has (ProtoEntity entity) => _it.Has (entity);

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public Slice<MaskItem> IncMaskIndices () => _it.IncMaskIndices ();

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public IProtoPool[] Includes () => _it.Includes ();

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public int LenSlow () {
            var len = 0;
            foreach (var _ in this) { len++; }
            return len;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public bool IsEmptySlow () {
            foreach (var _ in this) { return false; }
            return true;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public (ProtoEntity Entity, bool Ok) FirstSlow () {
            foreach (var e in this) { return (e, true); }
            return (default, false);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public (ProtoEntity[] entities, int len) CachedData () => (_cachedEntities, _cachedLen);
    }

#if ENABLE_IL2CPP
    [Il2CppSetOption (Option.NullChecks, false)]
    [Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
    public sealed class ProtoItExcCached : IProtoIt {
        readonly ProtoItExc _it;
        readonly ProtoEntityComparer _comparer;
        ProtoEntity[] _cachedEntities;
        int _cachedLen;
#if DEBUG
        bool _inited;
        bool _cached;

        // обслуживается внутренним итератором.
        void IProtoIt.AddBlocker (int amount) { }
#endif

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public ProtoItExcCached (Type[] iTypes, Type[] eTypes) {
#if DEBUG
            if (iTypes == null || iTypes.Length < 1) { throw new Exception ("некорректный список include-пулов для инициализации итератора"); }
#endif
            _it = new (iTypes, eTypes);
            _comparer = new ();
            _cachedEntities = Array.Empty<ProtoEntity> ();
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public IProtoIt Init (ProtoWorld world) {
            _it.Init (world);
#if DEBUG
            _inited = true;
#endif
            return this;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public ItEnumerator GetEnumerator () => new (this);

        ProtoItIEnumerator IProtoIt.GetEnumerator () => new (this, _it.IncMaskIndices (), _it.ExcMaskIndices ());

        public ref struct ItEnumerator {
            readonly ProtoEntity[] _entities;
            int _id;

            [MethodImpl (MethodImplOptions.AggressiveInlining)]
            public ItEnumerator (ProtoItExcCached it) {
#if DEBUG
                if (!it._inited) { throw new Exception ("итератор не инициализирован"); }
                if (!it._cached) { throw new Exception ("итератор не закеширован"); }
#endif
                _entities = it._cachedEntities;
                _id = it._cachedLen;
            }

            public ProtoEntity Current {
                [MethodImpl (MethodImplOptions.AggressiveInlining)]
                get => _entities[_id];
            }

            [MethodImpl (MethodImplOptions.AggressiveInlining)]
            public bool MoveNext () {
                _id--;
                return _id >= 0;
            }
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public ProtoWorld World () => _it.World ();

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public bool Has (ProtoEntity entity) => _it.Has (entity);

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public Slice<MaskItem> IncMaskIndices () => _it.IncMaskIndices ();

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public IProtoPool[] Includes () => _it.Includes ();

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public int LenSlow () {
            var len = 0;
            foreach (var _ in this) { len++; }
            return len;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public bool IsEmptySlow () {
            foreach (var _ in this) { return false; }
            return true;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public (ProtoEntity Entity, bool Ok) FirstSlow () {
            foreach (var e in this) { return (e, true); }
            return (default, false);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public (ProtoEntity[] entities, int len) CachedData () => (_cachedEntities, _cachedLen);

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public void BeginCaching () {
#if DEBUG
            if (!_inited) { throw new Exception ("итератор не инициализирован"); }
            if (_cached) { throw new Exception ("итератор уже закеширован"); }
#endif
            var cap = _it.World ().EntityGens ().Cap ();
            if (_cachedEntities.Length != cap) {
                _cachedEntities = new ProtoEntity[cap];
            }
            _cachedLen = 0;
            foreach (var e in _it) {
                _cachedEntities[_cachedLen++] = e;
            }
#if DEBUG
            _it.AddBlocker (2);
            _cached = true;
#endif
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public void EndCaching () {
#if DEBUG
            if (!_inited) { throw new Exception ("итератор не инициализирован"); }
            if (!_cached) { throw new Exception ("итератор не закеширован"); }
            _it.AddBlocker (-2);
            _cached = false;
#endif
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public void Sort (SortCachingCallback sortCb) {
#if DEBUG
            if (sortCb == null) { throw new Exception ("нет обработчика для сортировки"); }
            if (!_inited) { throw new Exception ("итератор не инициализирован"); }
            if (!_cached) { throw new Exception ("итератор не закеширован"); }
#endif
            if (_cachedLen > 0) {
                _comparer.Handler = sortCb;
                Array.Sort (_cachedEntities, 0, _cachedLen, _comparer);
                _comparer.Handler = null;
            }
        }
    }
}
