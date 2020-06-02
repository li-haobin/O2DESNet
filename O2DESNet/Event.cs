using System;
using System.Collections.Generic;

namespace O2DESNet
{
    public class Event : IDisposable
    {
        private static int _count;

        internal int Index { get; } = _count++;
        internal string Tag { get; }
        internal Sandbox Owner { get; }
        internal DateTime ScheduledTime { get; }        
        internal Action Action { get; }

        internal Event(Sandbox owner, Action action, DateTime scheduledTime, string tag = null)
        {
            Owner = owner;
            Action = action;
            ScheduledTime = scheduledTime;
            Tag = tag;
        }

        internal void Invoke() { Action?.Invoke(); }

        public override string ToString()
        {
            return $"{Tag}#{Index}";
        }

        public void Dispose()
        {
        }
    }

    internal sealed class EventComparer : IComparer<Event>
    {
        /// <summary>
        /// Gets an instance of Event comparer.
        /// </summary>
        public static EventComparer Instance => new EventComparer();

        private EventComparer() { }

        public int Compare(Event x, Event y)
        {
            if (x == null) throw new ArgumentNullException(nameof(x));
            if (y == null) throw new ArgumentNullException(nameof(y));
            var compare = x.ScheduledTime.CompareTo(y.ScheduledTime);
            return compare == 0 ? x.Index.CompareTo(y.Index) : compare;
        }
    }
}
