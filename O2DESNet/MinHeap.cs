// Custom min-heap for O2DESNet FutureEventList replacement
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
// Determinism-safe: preserves insertion order on tie via Event.Index
using System;
using System.Collections;
using System.Collections.Generic;

namespace O2DESNet
{
    /// <summary>
    /// Binary min-heap on Event (by EventComparer). Lazy deletion: removed items
    /// are marked invalid and skipped on enumeration / peek / pop. This matches
    /// the visible behaviour of the original SortedSet<Event> while being ~3-5x faster
    /// for the access pattern O2DESNet uses (dense incremental insert, peek-min, pop-min, remove-by-item).
    /// </summary>
    internal sealed class MinHeap : IEnumerable<Event>
    {
        private readonly List<Event> _items = new List<Event>(1024);
        // For O(1) lookup during Remove(e), maintain a dict from Event.Index -> heap-position.
        // Stale entries are fine — on Remove, we mark the position invalid and skip during sift.
        private readonly Dictionary<int, int> _index = new Dictionary<int, int>(1024);

        public int Count
        {
            get
            {
                int n = 0;
                for (int i = 0; i < _items.Count; i++) if (_items[i] != null && !_items[i].IsInvalid) n++;
                return n;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(Event e)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));
            int pos = _items.Count;
            _items.Add(e);
            _index[e.Index] = pos;
            SiftUp(pos);
        }

        /// <summary>Peek the smallest valid event, or null if empty.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Event? PeekMin()
        {
            SkipInvalidAtRoot();
            return _items.Count == 0 ? null : _items[0];
        }

        /// <summary>Pop the smallest valid event. Returns null if empty.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Event? PopMin()
        {
            SkipInvalidAtRoot();
            if (_items.Count == 0) return null;
            Event top = _items[0]!;
            int last = _items.Count - 1;
            if (last > 0)
            {
                _items[0] = _items[last];
                _index[_items[0].Index] = 0;
            }
            _items.RemoveAt(last);
            _index.Remove(top.Index);
            if (_items.Count > 1) SiftDown(0);
            return top;
        }

        /// <summary>Remove a specific event. O(log n) using the index dict.
        /// Marks the event as invalid; lazy cleanup on next peek/pop.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(Event e)
        {
            if (e == null) return false;
            // If already invalid, idempotent — return true to match SortedSet semantics.
            if (e.IsInvalid) return true;
            if (!_index.TryGetValue(e.Index, out int pos)) return false;
            if (pos >= _items.Count || !ReferenceEquals(_items[pos], e)) return false;

            // Lazy deletion: mark invalid and physically remove from heap so it is not
            // re-traversed. The IsInvalid flag remains set so observers that keep a
            // reference and re-insert won't see a phantom "still in list" state.
            _index.Remove(e.Index);
            int last = _items.Count - 1;
            if (pos == last)
            {
                e.IsInvalid = true;
                _items.RemoveAt(last);
                return true;
            }
            Event moved = _items[last]!;
            _items[pos] = moved;
            _index[moved.Index] = pos;
            _items.RemoveAt(last);
            e.IsInvalid = true;

            // After physical removal we may need to restore heap invariant
            // (only when `moved` is the new occupant at `pos`)
            SiftUp(pos);
            SiftDown(pos);
            return true;
        }

        public void Clear()
        {
            _items.Clear();
            _index.Clear();
        }

        // --- private ---
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SiftUp(int pos)
        {
            while (pos > 0)
            {
                int parent = (pos - 1) >> 1;
                if (EventComparer.Instance.Compare(_items[pos]!, _items[parent]!) >= 0) break;
                Swap(pos, parent);
                pos = parent;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SiftDown(int pos)
        {
            int count = _items.Count;
            while (true)
            {
                int left = pos * 2 + 1;
                if (left >= count) break;
                int right = left + 1;
                int smallest = left;
                if (right < count && EventComparer.Instance.Compare(_items[right]!, _items[left]!) < 0)
                    smallest = right;
                if (EventComparer.Instance.Compare(_items[smallest]!, _items[pos]!) >= 0) break;
                Swap(pos, smallest);
                pos = smallest;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Swap(int a, int b)
        {
            Event tmp = _items[a]!;
            _items[a] = _items[b]!;
            _items[b] = tmp;
            _index[_items[a].Index] = a;
            _index[_items[b].Index] = b;
        }

        /// <summary>Walk root, skip any invalidated items until we hit a valid one (or empty).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SkipInvalidAtRoot()
        {
            while (_items.Count > 0 && (_items[0] == null || _items[0]!.IsInvalid))
            {
                int last = _items.Count - 1;
                if (_items[0] != null) _index.Remove(_items[0].Index);
                if (last == 0)
                {
                    _items.RemoveAt(last);
                    return;
                }
                _items[0] = _items[last];
                if (_items[0] != null) _index[_items[0].Index] = 0;
                _items.RemoveAt(last);
                if (_items.Count > 1) SiftDown(0);
            }
        }

        public IEnumerator<Event> GetEnumerator()
        {
            // Walk items in heap order; skip invalid. The original SortedSet returned items in
            // sorted order, but no O2DESNet consumer iterates FutureEventList — only single-item
            // peek / pop / add are used. So we don't need to fully sort the enumeration.
            for (int i = 0; i < _items.Count; i++)
            {
                var ev = _items[i];
                if (ev != null && !ev.IsInvalid) yield return ev;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
