using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using O2DESNet;

namespace Test
{
    /// <summary>
    /// O2DESNet Event - Depart
    /// </summary>
    public class Archive : Event<Scenario, Status>
    {
        public Load Load { get; private set; }
        public Archive(Load load) { Load = load; }
        public override void Invoke()
        {
            Status.Processed.Add(Load);
        }
    }
}
