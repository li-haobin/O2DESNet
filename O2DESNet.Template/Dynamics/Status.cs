using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Template
{
    internal class Status
    {
        private Simulator _sim;   
        // include other properties here     

        internal Status(Simulator simulation)
        {
            _sim = simulation;
           
            // do other initialization
        }

        // if necessary, encapsulate status updating methods
    }
}
