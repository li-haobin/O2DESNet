using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Demos.Workshop
{
    public class Cluster
    {
        public List<Machine> Machines { get; set; }

        public Machine GetIdleMachine(int type)
        {
            return Machines.Where(m => m.Type == type && m.Processing == null).FirstOrDefault();
        }

        static public Cluster GetCluster1()
        {
            return new Cluster
            {
                Machines = new List<Machine> { 
                    new Machine{ Type = 1 },
                    new Machine{ Type = 1 },
                    new Machine{ Type = 1 },
                    new Machine{ Type = 2 },
                    new Machine{ Type = 2 },
                    new Machine{ Type = 3 },
                    new Machine{ Type = 3 },
                    new Machine{ Type = 3 },
                    new Machine{ Type = 3 },
                    new Machine{ Type = 4 },
                    new Machine{ Type = 4 },
                    new Machine{ Type = 4 },
                    new Machine{ Type = 5 },
                }
            };
        }
    }
}
