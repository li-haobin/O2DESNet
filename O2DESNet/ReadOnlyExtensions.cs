using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace O2DESNet
{
    public static class ReadOnlyExtensions
    {
        public static IReadOnlyList<T> AsReadOnly<T>(this HashSet<T> hashSet)
        {
            return hashSet.ToList().AsReadOnly();
        }
        public static IReadOnlyList<T1> AsReadOnly<T1, T2>(this ICollection<T2> collection, Func<T2, T1> asReadOnly)
        {
            return collection.Select(i => asReadOnly(i)).ToList().AsReadOnly();
        }
        public static IReadOnlyDictionary<T1, T2> AsReadOnly<T1, T2>(this Dictionary<T1, T2> dict)
        {
            return AsReadOnly(dict, i => i);
        }
        public static IReadOnlyDictionary<T1, IReadOnlyList<T2>> AsReadOnly<T1, T2>(this Dictionary<T1, List<T2>> dict)
        {
            return AsReadOnly(dict, list => (IReadOnlyList<T2>)list.AsReadOnly());
        }
        public static IReadOnlyDictionary<T1, IReadOnlyList<T2>> AsReadOnly<T1, T2>(this Dictionary<T1, HashSet<T2>> dict)
        {
            return AsReadOnly(dict, hashSet => (IReadOnlyList<T2>)hashSet.ToList().AsReadOnly());
        }
        public static IReadOnlyDictionary<T1, T2> AsReadOnly<T1, T2, T3>(this Dictionary<T1, T3> dict, Func<T3, T2> asReadOnly)
        {
            return new ReadOnlyDictionary<T1, T2>(dict.ToDictionary(i => i.Key, i => asReadOnly(i.Value)));
        }
        public static IReadOnlyDictionary<T1, T2> AsReadOnly<T1, T2, T3, T4>(this Dictionary<T3, T4> dict, Func<T3, T1> keyAsReadOnly, Func<T4, T2> valueAsReadOnly)
        {
            return new ReadOnlyDictionary<T1, T2>(dict.ToDictionary(i => keyAsReadOnly(i.Key), i => valueAsReadOnly(i.Value)));
        }

        public static IReadOnlyDictionary<TKey, TValue> ToReadOnlyDictionary<T, TKey, TValue>
            (this IEnumerable<T> enumerable, Func<T, TKey> keySelector, Func<T, TValue> elementSelector)
        {
            return enumerable.ToDictionary(keySelector, elementSelector).AsReadOnly();
        }
    }
}
