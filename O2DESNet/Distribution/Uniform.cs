using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Distribution
{
    public class Uniform
    {
        public static TimeSpan Sample(Random rs, TimeSpan upperbound)
        {
            return TimeSpan.FromSeconds(rs.NextDouble() * upperbound.TotalSeconds);
        }
    }
}
