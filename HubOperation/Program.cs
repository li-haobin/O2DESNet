﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubOperation
{
    class Program
    {
        static void Main(string[] args)
        {
            //var scenario = new Scenario();
            //var status = new Status(scenario, seed: 0) { Display = true };
            //var simulator = new Simulator(status);            
            //simulator.Run(TimeSpan.FromHours(1));

            var scenario = new Scenario();
            var status = new Status(scenario);
            var simulator = new Simulator(status);
            simulator.Run(TimeSpan.FromHours(2));

            Console.WriteLine("System Time of Packages Unloading Operation is {0}", status.getPackagesUnloadTime());
            Console.WriteLine("System Time of Last Ready Delivery Van is {0}", status.getLastReadyVanTime());
            Console.ReadKey();
        }
    }
}
