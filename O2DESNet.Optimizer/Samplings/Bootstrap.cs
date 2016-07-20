using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Optimizer.Samplings
{
    public class Bootstrap
    {
        public static List<T> Sample<T>(IEnumerable<T> pool, int size, Random rs)
        {
            var list = pool.ToList();
            return Enumerable.Range(0, size).Select(i => list[rs.Next(list.Count)]).ToList();
        }
    }
}
