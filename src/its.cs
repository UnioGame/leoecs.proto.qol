// ----------------------------------------------------------------------------
// Лицензия MIT-ZARYA
// (c) 2025 Leopotam <leopotam@yandex.ru>
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
#if ENABLE_IL2CPP
using Unity.IL2CPP.CompilerServices;
#endif

namespace Leopotam.EcsProto.QoL {
#if ENABLE_IL2CPP
    [Il2CppSetOption (Option.NullChecks, false)]
    [Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
    public static class It {
        public static Type[] List (params object[] components) {
            var types = new Type[components.Length];
            for (var i = 0; i < components.Length; i++) {
                types[i] = components[i].GetType ();
            }
            return types;
        }

        public static Type[] Inc<T1> () => new[] { typeof (T1) };
        public static Type[] Inc<T1, T2> () => new[] { typeof (T1), typeof (T2) };
        public static Type[] Inc<T1, T2, T3> () => new[] { typeof (T1), typeof (T2), typeof (T3) };

        public static Type[] Inc<T1, T2, T3, T4> () => new[] {
            typeof (T1), typeof (T2), typeof (T3), typeof (T4)
        };

        public static Type[] Inc<T1, T2, T3, T4, T5> () => new[] {
            typeof (T1), typeof (T2), typeof (T3), typeof (T4), typeof (T5)
        };

        public static Type[] Inc<T1, T2, T3, T4, T5, T6> () => new[] {
            typeof (T1), typeof (T2), typeof (T3), typeof (T4), typeof (T5), typeof (T6)
        };

        public static Type[] Exc<T1> () => new[] { typeof (T1) };
        public static Type[] Exc<T1, T2> () => new[] { typeof (T1), typeof (T2) };
        public static Type[] Exc<T1, T2, T3> () => new[] { typeof (T1), typeof (T2), typeof (T3) };

        public static ProtoItChain Chain<T> () where T : struct => new ProtoItChain ().Inc<T> ();
    }

#if ENABLE_IL2CPP
    [Il2CppSetOption (Option.NullChecks, false)]
    [Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
    public sealed class ProtoItChain {
        readonly List<Type> _iTypes;

        internal ProtoItChain () => _iTypes = new (4);

        public ProtoItChain Inc<T> () where T : struct {
            _iTypes.Add (typeof (T));
            return this;
        }

        public ProtoItChainExc Exc<T> () where T : struct => new ProtoItChainExc (_iTypes).Exc<T> ();

        public ProtoIt End () => new (_iTypes.ToArray ());

#if ENABLE_IL2CPP
    [Il2CppSetOption (Option.NullChecks, false)]
    [Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
        public sealed class ProtoItChainExc {
            readonly List<Type> _iTypes;
            readonly List<Type> _eTypes;

            internal ProtoItChainExc (List<Type> iTypes) {
                _iTypes = iTypes;
                _eTypes = new (4);
            }

            public ProtoItChainExc Inc<T> () where T : struct {
                _iTypes.Add (typeof (T));
                return this;
            }

            public ProtoItChainExc Exc<T> () where T : struct {
                _eTypes.Add (typeof (T));
                return this;
            }

            public ProtoItExc End () => new (_iTypes.ToArray (), _eTypes.ToArray ());
        }
    }
}
