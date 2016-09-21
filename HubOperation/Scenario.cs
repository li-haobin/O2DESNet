using HubOperation.Statics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubOperation
{
    /// <summary>
    /// The Scenario class that specifies what to simulate
    /// </summary>
    public class Scenario : O2DESNet.Scenario
    {
        public List<Dynamics.Container> ContainersList = new List<Dynamics.Container>();

        public List<Dynamics.InputStation> StationsList = new List<Dynamics.InputStation>();

        public List<Dynamics.DeliveryVan> DeliveryVansList = new List<Dynamics.DeliveryVan>();

        public Scenario()
        {


            for (int i = 1; i <= 2; i++)
            {
                StationsList.Add(new Dynamics.InputStation(TimeSpan.FromSeconds(8)));
            }

        }



        // encapsulate all static properties here
        // ...
    }
}
