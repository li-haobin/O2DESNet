using System;
using System.Collections.Generic;
using System.Text;

namespace O2DESNet.RandomVariables.Categorical
{
    interface IRandomVariable<T>
    {
        double Mean { get; set; }
        double StandardDeviation { get; set; }
        T Sample(Random rs);
    }
}
