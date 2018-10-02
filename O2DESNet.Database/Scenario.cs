using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace O2DESNet.Database
{
    public class Scenario
    {
        public int Id { get; set; }
        public Version Version { get; set; }
        public string Comment { get; set; }
        public DateTime CreateTime { get; set; }
        public ICollection<InputValue> InputValues { get; set; } = new HashSet<InputValue>();
        public ICollection<Replication> Replications { get; set; } = new HashSet<Replication>();
    }
}