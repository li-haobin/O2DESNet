using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet;

namespace O2DESNet
{
    public class Synchronizer : Component<Synchronizer.Statics>
    {
        #region Statics
        public class Statics : Scenario
        {
            public int Size { get; private set; }
            public Statics(int size) { Size = size; }
        }
        #endregion

        #region Dynamics
        public HashSet<int> TrueIndices { get; private set; } = new HashSet<int>();
        public bool AllTrue { get; private set; } = false;
        public bool AllFalse { get; private set; } = true;
        #endregion

        #region Events
        private class UpdStateEvent : Event
        {
            public Synchronizer Synchronizer { get; private set; }
            public int Index { get; private set; }
            public bool Value { get; private set; }
            internal UpdStateEvent(Synchronizer synchronizer, int index, bool value)
            {
                Synchronizer = synchronizer;
                Index = index;
                Value = value;
            }
            public override void Invoke()
            {
                if (Index < 1 || Index > Synchronizer.Config.Size) throw new Exception("Index is out of range.");
                if (Value && !Synchronizer.TrueIndices.Contains(Index)) Synchronizer.TrueIndices.Add(Index);
                if (!Value && Synchronizer.TrueIndices.Contains(Index)) Synchronizer.TrueIndices.Remove(Index);
                Synchronizer.AllTrue = Synchronizer.TrueIndices.Count == Synchronizer.Config.Size;
                Synchronizer.AllFalse = Synchronizer.TrueIndices.Count == 0;
                foreach (var evnt in Synchronizer.OnStateChg) Execute(evnt(Synchronizer));
            }
            public override string ToString() { return string.Format("{0}_UpdState", Synchronizer); }
        }
        #endregion

        #region Input Events - Getters
        /// <summary>
        /// Update whether the condition at index is satisfied
        /// </summary>
        /// <param name="index">Range from 1 to defined size</param>
        /// <param name="value">true or false</param>
        /// <returns></returns>
        public Event UpdState(int index, bool value) { return new UpdStateEvent(this, index, value); }
        #endregion

        #region Output Events - Reference to Getters
        public List<Func<Synchronizer, Event>> OnStateChg { get; private set; } = new List<Func<Synchronizer, Event>>();
        #endregion
        
        public Synchronizer(Statics config, string tag = null) : base(config, 0, tag)
        {
            Name = "Synchronizer";
        }
    }
}
