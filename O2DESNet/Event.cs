using System;
using System.Collections.Generic;

namespace O2DESNet
{
    public sealed class Event : IDisposable
    {
        private static int _count = 0;
        internal int Index { get; } = _count++;
        internal string? Tag { get; }
        internal Sandbox Owner { get; }
        internal DateTime ScheduledTime { get; }
        internal Action Action { get; }

        internal Event(Sandbox owner, Action action, DateTime scheduledTime, string? tag = null)
        {
            Owner = owner;
            Action = action;
            ScheduledTime = scheduledTime;
            Tag = tag;
        }

        internal void Invoke() => Action();

        public override string ToString() => string.Format("{0}#{1}", Tag, Index);

        public void Dispose() { }
    }

    internal sealed class EventComparer : IComparer<Event>
    {
        private static readonly EventComparer _instance = new();
        private EventComparer() { }
        public static EventComparer Instance => _instance;
        public int Compare(Event? x, Event? y)
        {
            if (x == null || y == null) return 0;
            var compare = x.ScheduledTime.CompareTo(y.ScheduledTime);
            if (compare == 0) return x.Index.CompareTo(y.Index);
            return compare;
        }
    }
}
