using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace O2DESNet.Database
{
    public class Replication
    {
        public int Id { get; set; }
        public int Seed { get; set; }
        public Scenario Scenario { get; set; }
        public ICollection<Snapshot> Snapshots { get; set; } = new HashSet<Snapshot>();
    }
}