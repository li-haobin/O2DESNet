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
        public Dictionary<Path,HourCounter> PathUtils { get; private set; }

        public Status(Scenario scenario, int seed = 0) : base(scenario, seed)
        {
            scenario.Initialize();
            Vehicles = new HashSet<Vehicle>();
            VehiclesOnPath = Scenario.Paths.ToDictionary(p => p, p => new HashSet<Vehicle>());
            PathUtils = Scenario.Paths.ToDictionary(p => p, p => new HourCounter(DateTime.MinValue));
        }

        public override void WarmedUp(DateTime clockTime)
        {
            foreach (var util in PathUtils.Values) util.WarmedUp(clockTime);
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

        public virtual void UpdateSpeeds(Path path, DateTime clockTime)
        {
            foreach (var v in VehiclesOnPath[path]) v.SetSpeed(path.FullSpeed, clockTime);
            //foreach (var v in VehiclesOnPath[path]) v.SetSpeed(path.FullSpeed / VehiclesOnPath[path].Count, clockTime);
        }

        #region Display
        public string GetStr_VehiclesOnPath()
        {
            var str = "";
            foreach(var path in Scenario.Paths)
            {
                str += string.Format("{0}:\t", path);
                foreach (var v in VehiclesOnPath[path].OrderBy(v => v.Id)) str += string.Format("{0},", v);
                str += "\n";
            }
            return str;
        }
        #endregion
    }
}
