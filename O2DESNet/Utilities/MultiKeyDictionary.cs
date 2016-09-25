using System;
using System.Collections.Generic;
using System.Linq;

namespace Utilities
{
    /// <summary>
    /// A multi-key dictionary where the order of the keys matters
    /// </summary>
    /// <typeparam name="TKey1"></typeparam>
    /// <typeparam name="TKey2"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class MultiKeyDictionary<TKey1, TKey2, TValue>
        where TValue : IComparable
    {
        /// <summary>
        /// Internal data structure
        /// </summary>
        private Dictionary<TKey1, Dictionary<TKey2, TValue>> Dict;

        public MultiKeyDictionary()
        {
            Dict = new Dictionary<TKey1, Dictionary<TKey2, TValue>>();
        }

        /// <summary>
        /// Accessor with two comma-separated keys. If the specified key is not found, a get operation throws a KeyNotFoundException, and a set operation creates a new element with the specified key.
        /// </summary>
        /// <param name="Key1"></param>
        /// <param name="Key2"></param>
        /// <returns></returns>
        public TValue this[TKey1 Key1, TKey2 Key2]
        {
            get
            {
                return Dict[Key1][Key2];
            }
            set
            {
                if (!Dict.ContainsKey(Key1))
                    Dict.Add(Key1, new Dictionary<TKey2, TValue>());

                Dict[Key1][Key2] = value;
            }
        }
        public int Count
        {
            get
            {
                return Dict.Values.Sum(d => d.Count);
            }
        }

        public bool ContainsKeys(TKey1 Key1, TKey2 Key2)
        {
            return Dict.ContainsKey(Key1) && Dict[Key1].ContainsKey(Key2);
        }
        public bool ContainsKeys(Tuple<TKey1, TKey2> KeyTuple)
        {
            return ContainsKeys(KeyTuple.Item1, KeyTuple.Item2);
        }

        public void Add(TKey1 Key1, TKey2 Key2, TValue Value)
        {
            if (!Dict.ContainsKey(Key1))
                Dict.Add(Key1, new Dictionary<TKey2, TValue>());

            Dict[Key1].Add(Key2, Value);
        }
        public void Add(Tuple<TKey1, TKey2> KeyTuple, TValue Value)
        {
            Add(KeyTuple.Item1, KeyTuple.Item2, Value);
        }
        public void Remove(TKey1 Key1, TKey2 Key2)
        {
            Dict[Key1].Remove(Key2);
            if (Dict[Key1].Count == 0) Dict.Remove(Key1);
        }
        public void Remove(Tuple<TKey1, TKey2> KeyTuple)
        {
            Remove(KeyTuple.Item1, KeyTuple.Item2);
        }

        public Tuple<TKey1, TKey2> MaxByValue()
        {
            var maxKey1 = Dict.Keys.First();
            var maxKey2 = Dict.Values.First().Keys.First();
            var maxValue = this[maxKey1, maxKey2];

            foreach (var key1 in Dict.Keys)
            {
                foreach (var key2 in Dict[key1].Keys)
                {
                    if (this[key1, key2].CompareTo(maxValue) > 0)
                    {
                        maxKey1 = key1;
                        maxKey2 = key2;
                        maxValue = this[key1, key2];
                    }
                }
            }

            return Tuple.Create(maxKey1, maxKey2);
        }
        public TValue Max()
        {
            var tuple = MaxByValue();
            return this[tuple.Item1, tuple.Item2];
        }
        public Tuple<TKey1, TKey2> MinByValue()
        {
            var maxKey1 = Dict.Keys.First();
            var maxKey2 = Dict.Values.First().Keys.First();
            var maxValue = this[maxKey1, maxKey2];

            foreach (var key1 in Dict.Keys)
            {
                foreach (var key2 in Dict[key1].Keys)
                {
                    if (this[key1, key2].CompareTo(maxValue) < 0)
                    {
                        maxKey1 = key1;
                        maxKey2 = key2;
                        maxValue = this[key1, key2];
                    }
                }
            }

            return Tuple.Create(maxKey1, maxKey2);
        }
        public TValue Min()
        {
            var tuple = MinByValue();
            return this[tuple.Item1, tuple.Item2];
        }
    }
}
