using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet;

namespace O2DESNet
{
    public class Synchronizer : State<Synchronizer.Statics>
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
        private abstract class InternalEvent : Event<Synchronizer, Statics> { }
        private class UpdStateEvent : InternalEvent
        {
            internal int Idx { get; set; }
            internal bool Value { get; set; }
            public override void Invoke()
            {
                if (Idx < 1 || Idx > Config.Size) throw new Exception("Index is out of range.");
                if (Value && !This.TrueIndices.Contains(Idx)) This.TrueIndices.Add(Idx);
                if (!Value && This.TrueIndices.Contains(Idx)) This.TrueIndices.Remove(Idx);
                This.AllTrue = This.TrueIndices.Count == This.Config.Size;
                This.AllFalse = This.TrueIndices.Count == 0;
                Execute(This.OnStateChg.Select(e => e()));
            }
            public override string ToString() { return string.Format("{0}_UpdState", This); }
        }
        #endregion

        #region Input Events - Getters
        /// <summary>
        /// Update whether the condition at index is satisfied
        /// </summary>
        /// <param name="idx">Range from 1 to defined size</param>
        /// <param name="value">true or false</param>
        /// <returns></returns>
        public Event UpdState(int idx, bool value) { return new UpdStateEvent { This = this, Idx = idx, Value = value }; }
        #endregion

        #region Output Events - Reference to Getters
        public List<Func<Event>> OnStateChg { get; private set; } = new List<Func<Event>>();
        #endregion
        
        public Synchronizer(Statics config, string tag = null) : base(config, 0, tag)
        {
            Name = "Synchronizer";
        }
    }
}
