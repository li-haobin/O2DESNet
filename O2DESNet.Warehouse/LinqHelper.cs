using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Warehouse
{
    public static class LinqHelper
    {
        public static List<T> ExtractAll<T>(this List<T> source, Predicate<T> match)
        {
            List<T> extract = source.FindAll(match);
            source.RemoveAll(match);
            return extract;
        }
    }
}
