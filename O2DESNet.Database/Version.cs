using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace O2DESNet.Database
{
    public class Version
    {
        public int Id { get; set; }
        public Project Project { get; set; }
        public ICollection<Scenario> Scenarios { get; set; } = new HashSet<Scenario>();
        public ICollection<InputPara> InputParas { get; set; } = new HashSet<InputPara>();
        public ICollection<OutputPara> OutputParas { get; set; } = new HashSet<OutputPara>();
        public string Number { get; set; }
        public string Comment { get; set; }
        public string URL { get; set; }
    }
}