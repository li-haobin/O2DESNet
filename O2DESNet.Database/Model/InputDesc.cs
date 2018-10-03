using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace O2DESNet.Database
{
    public class InputDesc
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Comment { get; set; }
        public DateTime CreateTime { get; set; }
        public Project Project { get; set; }
        public ICollection<InputPara> InputParas { get; set; } = new HashSet<InputPara>();
    }
}