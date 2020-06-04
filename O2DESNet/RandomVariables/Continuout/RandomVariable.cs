using System;
using System.Collections.Generic;
using System.Text;

namespace O2DESNet.RandomVariables.Continuous
{
    public abstract class RandomVariable
    {
        public abstract double Sample(Random rs);
    }
}
