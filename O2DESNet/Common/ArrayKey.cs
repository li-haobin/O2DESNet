using System.Collections.Generic;
using System.Linq;

namespace O2DESNet
{
    internal class ArrayKey<T>
    {
        private T[] _values;
        internal ArrayKey(IEnumerable<T> values) { _values = values.ToArray(); }
        public override int GetHashCode()
        {
            int hc = _values.Length;
            foreach (var v in _values) hc = unchecked(hc * 314159 + v.GetHashCode());
            return hc;
        }
        public override bool Equals(object obj)
        {
            var key = obj as ArrayKey<T>;
            if (_values.Length != key._values.Length) return false;
            for (int i = 0; i < _values.Length; i++) if (!_values[i].Equals(key._values[i])) return false;
            return true;
        }
        public static bool operator ==(ArrayKey<T> k1, ArrayKey<T> k2) { return k1.Equals(k2); }
        public static bool operator !=(ArrayKey<T> k1, ArrayKey<T> k2) { return !(k1 == k2); }
    }
}
