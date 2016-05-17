using O2DESNet.PathMover.Dynamics;
using O2DESNet.PathMover.Statics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.PathMover
{
    public class Status : Status<Scenario>
    {
        public HashSet<Vehicle> Vehicles { get; private set; }
        public Dictionary<Path,HashSet<Vehicle>> VehiclesOnPath { get; private set; }

        internal Status(Scenario scenario, int seed = 0) : base(scenario, seed)
        {
            scenario.Initialize();
            Vehicles = new HashSet<Vehicle>();
            VehiclesOnPath = Scenario.Paths.ToDictionary(p => p, p => new HashSet<Vehicle>());
        }

        public Vehicle PutOn(ControlPoint start, DateTime clockTime)
        {
            var vehicle = new Vehicle(this, start, clockTime);
            Vehicles.Add(vehicle);
            return vehicle;
        }

        public void PutOff(Vehicle vehicle)
        {
            if (!Vehicles.Contains(vehicle)) throw new Exception("Vehicle does not exist in the path-mover.");
            if (vehicle.Next != null) throw new Exception("Vehicle has not reached next control point.");
            Vehicles.Remove(vehicle);
        }

        #region Display
        public void Display_VehiclesOnPath()
        {
            foreach(var path in Scenario.Paths)
            {
                Console.Write("{0}:\t", path);
                foreach (var v in VehiclesOnPath[path].OrderBy(v => v.Id)) Console.Write("{0},", v);
                Console.WriteLine();
            }
            Console.WriteLine();
        }
        #endregion
    }
}
