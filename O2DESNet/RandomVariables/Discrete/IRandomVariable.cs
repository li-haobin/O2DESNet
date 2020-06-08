using System;

namespace O2DESNet.RandomVariables.Discrete
{
    interface IRandomVariable
    {
        double Mean { get; set; }
        double StadndardDeviation { get; set; }
        int Sample(Random rs);
    }
}
