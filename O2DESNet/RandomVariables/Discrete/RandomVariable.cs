using System;

namespace O2DESNet.RandomVariables.Discrete
{
    public abstract class RandomVariable
    {
        public abstract int Sample(Random rs);
    }
}
