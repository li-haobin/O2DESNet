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
        [ForeignKey("Scenario_Id"), DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Scenario Scenario { get; set; }
        public int Scenario_Id { get; set; }
        public DateTime CreateTime { get; set; }
        public string CreateBy { get; set; }
        public ICollection<Snapshot> Snapshots { get; set; } = new HashSet<Snapshot>();
    }
}