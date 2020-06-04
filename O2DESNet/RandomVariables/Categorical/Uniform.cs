using System;
using System.Collections.Generic;
using System.Linq;

namespace O2DESNet.RandomVariables.Categorical
{
    public class Uniform<T> : RandomVariable<T>
    {
        public IEnumerable<T> Candidates { get; set; }
        public override T Sample(Random rs)
        {
            if (Candidates.Count() == 0) return default;
            return Candidates.ElementAt(rs.Next(Candidates.Count()));
        }
    }
}
