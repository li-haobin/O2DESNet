using System.Linq;

namespace O2DESNet
{
    public class FIFOServer<TLoad> : Server<TLoad>
    {
        #region Dynamics
        public System.Collections.Generic.Queue<TLoad> Sequence { get; private set; } = new System.Collections.Generic.Queue<TLoad>();
        protected override void PushIn(TLoad load) { base.PushIn(load); Sequence.Enqueue(load); }
        protected override bool ChkToDepart() { return base.ChkToDepart() && Served.Contains(Sequence.First()); }
        protected override TLoad GetToDepart() { return Sequence.Dequeue(); }
        #endregion

        public FIFOServer(Statics config, int seed, string tag = null) : base(config, seed, tag) { Name = "FIFOServer"; }
    }
}