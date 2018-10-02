using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace O2DESNet.Database
{
    public class Snapshot
    {
        public int Id { get; set; }
        public double TimeSeconds { get; set; }
        public ICollection<OutputValue> OutputValues { get; set; } = new HashSet<OutputValue>();
        public Replication Replication { get; set; }
    }
}