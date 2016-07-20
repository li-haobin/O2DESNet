using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Optimizer
{
    public class Exception_InconsistentDimensions : Exception
    {
        public Exception_InconsistentDimensions() : base("Dimensions of two vectors are not consistent.") { }
        
    }
    public class Exception_EmptySet : Exception
    {
        public Exception_EmptySet() : base("Given set cannot be empty.") { }
    }
    public class Exception_NonPositiveValue : Exception
    {
        public Exception_NonPositiveValue() : base("Positive value is expected.") { }
    }
}
