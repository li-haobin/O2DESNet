using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.PathMover
{
    public class PMStatus
    {
        public PMScenario PMScenario { get; private set; }        
        public HashSet<Vehicle> Vehicles { get; private set; }
        public Dictionary<Path, HashSet<Vehicle>> VehiclesOnPath { get; private set; }
        public Dictionary<Path, HourCounter> PathUtils { get; private set; }
        internal int VehicleId { get; set; } = 0;

        public PMStatus(PMScenario statics)
        {
            PMScenario = statics;
            PMScenario.Initialize();
            Vehicles = new HashSet<Vehicle>();
            VehiclesOnPath = PMScenario.Paths.ToDictionary(p => p, p => new HashSet<Vehicle>());
            PathUtils = PMScenario.Paths.ToDictionary(p => p, p => new HourCounter(DateTime.MinValue));
        }

        public void WarmedUp(DateTime clockTime)
        {
            foreach (var util in PathUtils.Values) util.WarmedUp(clockTime);
        }

        public virtual Vehicle PutOn(ControlPoint start, DateTime clockTime)
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
            vehicle.ReachEventHashCode = null;
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
            foreach (var path in PMScenario.Paths)
            {
                str += string.Format("{0}:\t", path);
                foreach (var v in VehiclesOnPath[path].OrderBy(v => v.Id)) str += string.Format("{0},", v);
                str += "\n";
            }
            return str;
        }

        protected void InitDrawingParams(DrawingParams dParams)
        {
            PMScenario.InitDrawingParams(dParams);
        }

        public Bitmap DrawToImage(DrawingParams dParams, DateTime now, bool init = true)
        {
            if (init) InitDrawingParams(dParams);
            Bitmap bitmap = new Bitmap(Convert.ToInt32(dParams.Width), Convert.ToInt32(dParams.Height), PixelFormat.Format32bppArgb);
            Draw(Graphics.FromImage(bitmap), dParams, now, init: false);
            return bitmap;
        }

        public void DrawToFile(string file, DrawingParams dParams, DateTime now)
        {
            InitDrawingParams(dParams);
            DrawToImage(dParams, now, init: false).Save(file, ImageFormat.Png);
        }


        public virtual void Draw(Graphics g, DrawingParams dParams, DateTime now, bool init = true)
        {
            if (init) InitDrawingParams(dParams);
            //PMScenario.Draw(g, dParams, init: false);

            // draw for each vehicle            
            foreach (var v in VehiclesOnPath.Values.SelectMany(vs => vs)) v.Draw(g, dParams, PMScenario, now);
        }

        #endregion        
    }
}
