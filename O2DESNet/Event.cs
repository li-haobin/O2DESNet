using System;
using System.Collections.Generic;

namespace O2DESNet
{
    public class Event : IDisposable
    {
        private static int _count = 0;
        internal int Index { get; private set; } = _count++;
        internal string Tag { get; private set; }
        internal Sandbox Owner { get; private set; }
        internal DateTime ScheduledTime { get; private set; }        
        internal Action Action { get; private set; }

        internal Event(Sandbox owner, Action action, DateTime scheduledTime, string tag = null)
        {
            Owner = owner;
            Action = action;
            ScheduledTime = scheduledTime;
            Tag = tag;
        }
        internal void Invoke() { Action.Invoke(); }
        public override string ToString()
        {
            return string.Format("{0}#{1}", Tag, Index);
        }

        public void Dispose()
        {
        }
    }
    internal sealed class EventComparer : IComparer<Event>
    {
        private static readonly EventComparer _instance = new EventComparer();
        private EventComparer() { }
        public static EventComparer Instance { get { return _instance; } }
        public int Compare(Event x, Event y)
        {
            int compare = x.ScheduledTime.CompareTo(y.ScheduledTime);
            if (compare == 0) return x.Index.CompareTo(y.Index);
            return compare;
        }
    }
}
