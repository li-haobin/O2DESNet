using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace O2DESNet.Database
{
    public class OutputValue
    {
        public int Id { get; set; }
        public double Value { get; set; }
        public Snapshot Snapshot { get; set; }
        public OutputPara OutputPara { get; set; }
    }
}