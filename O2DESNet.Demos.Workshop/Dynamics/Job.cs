using System;

namespace O2DESNet.Demos.Workshop
{
    public class Job
    {
        public int Id { get; set; }
        public JobType Type { get; set; }
        public DateTime EnterTime { get; set; }
        public DateTime ExitTime { get; set; }
        public int CurrentStage { get; set; }
        public Machine BeingProcessedBy { get; set; }

        public int CurrentMachineTypeIndex
        {
            get
            {
                if (CurrentStage < Type.MachineSequence.Count) return Type.MachineSequence[CurrentStage];
                return -1;
            }
        }
    }
}

