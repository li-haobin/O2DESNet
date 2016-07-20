using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Optimizer.Samplings
{
    public class TruncatedGeometric
    {
        public static T Sample<T>(IEnumerable<T> items, double p, Random rs)
        {
            if (items.Count() == 0) throw new Exception_EmptySet();
            if (p <= 0) throw new Exception_NonPositiveValue();
            int i = 0;
            while (true)
            {
                if (rs.NextDouble() < p) return items.ElementAt(i);
                i = (i + 1) % items.Count();
            }
        }
    }
}
