using O2DESNet.Demos.Workshop.Statics;
using System;

namespace O2DESNet.Demos.Workshop.Dynamics
{
    public class Product
    {
        public int Id { get; set; }
        public ProductType Type { get; set; }
        public DateTime EnterTime { get; set; }
        public DateTime ExitTime { get; set; }
        public int CurrentStage { get; set; }
        public Machine BeingProcessedBy { get; set; }

        public WorkStation CurrentWorkStation
        {
            get
            {
                if (CurrentStage < Type.JobSequence.Count) return Type.JobSequence[CurrentStage];
                return null;
            }
        }
    }
}

