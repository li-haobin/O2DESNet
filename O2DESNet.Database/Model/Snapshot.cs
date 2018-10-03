using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace O2DESNet.Database
{
    public class Snapshot
    {
        public int Id { get; set; }
        /// <summary>
        /// In total days from DateTime.MinValue
        /// </summary>
        public double ClockTime { get; set; }
        public DateTime CheckinTime { get; set; }
        public string CheckinBy { get; set; }
        public ICollection<OutputValue> OutputValues { get; set; } = new HashSet<OutputValue>();
        public Replication Replication { get; set; }
    }
}