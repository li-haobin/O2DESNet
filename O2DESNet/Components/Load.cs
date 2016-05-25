using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet
{
    public class Load
    {
        private static int _count = 0;
        public int Id { get; protected set; }
        public Load() { Id = _count++; }
        public override string ToString() { return string.Format("Load#{0}", Id); }
        public DateTime TimeStamp_Arrive { get; internal set; }
        public DateTime? TimeStamp_StartProcess { get; internal set; } = null;
        public DateTime? TimeStamp_FinishProcess { get; internal set; } = null;
        public TimeSpan TimeSpan_InSystem { get { return TimeStamp_FinishProcess.Value - TimeStamp_Arrive; } }
        public TimeSpan TimeSpan_Processing { get { return TimeStamp_FinishProcess.Value - TimeStamp_StartProcess.Value; } }
        public TimeSpan TimeSpan_Waiting { get { return TimeStamp_StartProcess.Value - TimeStamp_Arrive; } }
    }
}
