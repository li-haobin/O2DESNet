using System;
using System.Collections.Generic;
using System.Text;

namespace O2DESNet.RandomVariables.Categorical
{
    interface IRandomVariable<T>
    {
        double Mean { get; set; }
        double StadndardDeviation { get; set; }
        double CoefficientVariation { get; set; }
        T Sample(Random rs);
    }
}
