using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace O2DESNet.Database
{
    public class InputValue
    {
        public int Id { get; set; }
        public double Value { get; set; }
        public Scenario Scenario { get; set; }
        public InputPara InputPara { get; set; }
    }
}