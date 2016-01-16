using System;

namespace O2DESNet.Warehouse
{
    [Serializable]
    internal abstract class Event : IEvent
    {
        protected Simulator _sim { get; private set; }
        protected Event(Simulator simulator) { _sim = simulator; }
        public abstract void Invoke();
        public abstract void Backtrack();
    }
}
