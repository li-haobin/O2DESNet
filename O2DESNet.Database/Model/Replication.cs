using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace O2DESNet.Database
{
    public class Replication
    {
        public int Id { get; set; }
        public int Seed { get; set; }
        public Scenario Scenario { get; set; }
        public DateTime Timestamp { get; set; }
        public string Operator { get; set; }
        public int Thread_UID { get; set; }
        public ICollection<Snapshot> Snapshots { get; set; } = new HashSet<Snapshot>();
        /// <summary>
        /// Wether the replication should be excluded from the experiment statistics
        /// </summary>
        public bool Excluded { get; set; } = false;
    }
}