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
        public PMScenario Statics { get; private set; }        
        public HashSet<Vehicle> Vehicles { get; private set; }
        public Dictionary<Path, HashSet<Vehicle>> VehiclesOnPath { get; private set; }
        public Dictionary<Path, HourCounter> PathUtils { get; private set; }
        internal int VehicleId { get; set; } = 0;

        public PMStatus(PMScenario statics)
        {
            Statics = statics;
            Statics.Initialize();
            Vehicles = new HashSet<Vehicle>();
            VehiclesOnPath = Statics.Paths.ToDictionary(p => p, p => new HashSet<Vehicle>());
            PathUtils = Statics.Paths.ToDictionary(p => p, p => new HourCounter(DateTime.MinValue));
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
            foreach (var path in Statics.Paths)
            {
                str += string.Format("{0}:\t", path);
                foreach (var v in VehiclesOnPath[path].OrderBy(v => v.Id)) str += string.Format("{0},", v);
                str += "\n";
            }
            return str;
        }

        //public void DrawToImage(string file, int width, int height)
        //{
        //    Resize(width, height);
        //    Bitmap bitmap = new Bitmap(Convert.ToInt32(_width), Convert.ToInt32(_height), PixelFormat.Format32bppArgb);
        //    Draw(Graphics.FromImage(bitmap));
        //    bitmap.Save(file, ImageFormat.Png);
        //}

        //public void Draw(Graphics g, int width, int height)
        //{
        //    // adjust width and height
        //    Resize(width, height);
        //    Draw(g);
        //}

        //private void Draw(Graphics g)
        //{
        //    //foreach (var path in Paths) DrawPath(g, path, Color.DarkSlateGray);
        //    //foreach (var cp in ControlPoints) DrawControlPoint(g, cp, Color.DarkSlateGray);
        //}
        #endregion        
    }
}
