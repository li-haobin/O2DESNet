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
        public bool Changed { get; set; } = true;

        public PMStatus(PMScenario statics)
        {
            PMScenario = statics;
            PMScenario.Initialize();
            Vehicles = new HashSet<Vehicle>();
            VehiclesOnPath = PMScenario.Paths.ToDictionary(p => p, p => new HashSet<Vehicle>());
            PathUtils = PMScenario.Paths.ToDictionary(p => p, p => new HourCounter(DateTime.MinValue));
            Changed = true; 
        }

        public void WarmedUp(DateTime clockTime)
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
            foreach (var path in PMScenario.Paths)
            {
                str += string.Format("{0}:\t", path);
                foreach (var v in VehiclesOnPath[path].OrderBy(v => v.Id)) str += string.Format("{0},", v);
                str += "\n";
            }
            return str;
        }

        public Image DrawToImage(DrawingParams dParams)
        {
            PMScenario.InitDrawingParams(dParams);
            Bitmap bitmap = new Bitmap(Convert.ToInt32(dParams.Width), Convert.ToInt32(dParams.Height), PixelFormat.Format32bppArgb);
            Draw(Graphics.FromImage(bitmap), dParams, init: false);
            return bitmap;
        }

        public void DrawToFile(string file, DrawingParams dParams)
        {
            PMScenario.InitDrawingParams(dParams);
            Bitmap bitmap = new Bitmap(Convert.ToInt32(dParams.Width), Convert.ToInt32(dParams.Height), PixelFormat.Format32bppArgb);
            Draw(Graphics.FromImage(bitmap), dParams, init: false);
            bitmap.Save(file, ImageFormat.Png);
        }


        public void Draw(Graphics g, DrawingParams dParams, bool init = true)
        {
            if (init) PMScenario.InitDrawingParams(dParams);
            //PMScenario.Draw(g, dParams, init: false);
            var pen = new Pen(dParams.VehicleColor, dParams.VehicleBorder);

            foreach (var path in PMScenario.Paths)
            {
                int n = VehiclesOnPath[path].Count;
                var coords = PMScenario.GetCoords(path);
                for (int i = 0; i < n;i++)
                {
                    var p = dParams.GetPoint(LinearTool.SlipByRatio(coords[0], coords[1], (i + 0.5) / n));
                    g.DrawEllipse(pen, p.X - dParams.VehicleRadius, p.Y - dParams.VehicleRadius, dParams.VehicleRadius * 2, dParams.VehicleRadius * 2);
                }
                
            }
        }

        #endregion        
    }
}
