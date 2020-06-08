using System;
using System.Collections.Generic;
using System.Text;

namespace O2DESNet.RandomVariables.Continuous
{
    interface IRandomVariable
    {
        double Mean { get; set; }
        double StandardDeviation { get; set; }
        double Sample(Random rs);
    }
}
