using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Demos.Workshop
{
    public class JobType
    {
        public int Id { get; internal set; }
        public double Frequence { get; internal set; }
        public List<int> MachineSequence { get; internal set; }
        public List<double> MeanProcessingTimes { get; internal set; }
    }
}
