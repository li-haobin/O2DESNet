using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace O2DESNet.Database
{
    public class Project
    {
        public int ProjectId { get; set; }
        public string Name { get; set; }
        public ICollection<InputDesc> InputDescs { get; set; } = new HashSet<InputDesc>();
        public ICollection<OutputDesc> OutputDescs { get; set; } = new HashSet<OutputDesc>();
    }
}