using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.PathMover.Exceptions
{
    class InfeasibleConstruction : Exception
    {
        public InfeasibleConstruction() { }
        public InfeasibleConstruction(string message) : base(message) { }
        public InfeasibleConstruction(string message, Exception inner) : base(message, inner) { }
    }
    class InfeasibleTravelling : Exception
    {
        public InfeasibleTravelling() { }
        public InfeasibleTravelling(string message) : base(message) { }
        public InfeasibleTravelling(string message, Exception inner) : base(message, inner) { }
    }

}
