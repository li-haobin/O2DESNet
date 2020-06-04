using System;
using System.Collections.Generic;
using System.Text;

namespace O2DESNet.RandomVariables.Categorical
{
    public abstract class RandomVariable<T>
    {
        public abstract T Sample(Random rs);
    }
}
