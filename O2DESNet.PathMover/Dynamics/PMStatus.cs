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

        public Bitmap DrawToImage(DrawingParams dParams, DateTime now, bool init = true)
        {
            if (!init) PMScenario.InitDrawingParams(dParams);
            Bitmap bitmap = new Bitmap(Convert.ToInt32(dParams.Width), Convert.ToInt32(dParams.Height), PixelFormat.Format32bppArgb);
            Draw(Graphics.FromImage(bitmap), dParams, now, init: false);
            return bitmap;
        }

        public void DrawToFile(string file, DrawingParams dParams, DateTime now)
        {
            PMScenario.InitDrawingParams(dParams);
            DrawToImage(dParams, now, init: false).Save(file, ImageFormat.Png);
        }


        public void Draw(Graphics g, DrawingParams dParams, DateTime now, bool init = true)
        {
            if (init) PMScenario.InitDrawingParams(dParams);
            //PMScenario.Draw(g, dParams, init: false);

            var pen = new Pen(dParams.VehicleColor, dParams.VehicleBorder); // for vehicle
            var colors = new List<Color> { Color.DarkBlue, Color.DarkCyan, Color.DarkGoldenrod, Color.DarkGreen, Color.DarkRed, Color.DarkSeaGreen, Color.DarkKhaki, Color.DarkMagenta, Color.DarkOliveGreen, Color.DarkOrange, Color.DarkOrchid, Color.DarkSalmon, Color.DarkSlateBlue, Color.DarkTurquoise, Color.DarkViolet };

            // draw for each vehicle
            foreach (var v in VehiclesOnPath.Values.SelectMany(vs => vs))
            {
                DenseVector towards = null;
                var vColor = colors[v.Id % colors.Count];
                var start = PMScenario.GetCoord(v.Current, ref towards);
                var end = PMScenario.GetCoord(v.Next, ref towards);

                var ratio = Math.Min(1, 1 - v.RemainingRatio + (now - v.LastActionTime).TotalSeconds / (v.TimeToReach.Value - v.LastActionTime).TotalSeconds);

                var curPath = v.Current.PathingTable[v.Next];
                var rCurrent = v.Current.Positions[curPath] / curPath.Length;
                var rNext = v.Next.Positions[curPath] / curPath.Length;

                // draw vehicle shape                
                pen.Color = vColor;
                var curRatioOnPath = rCurrent + (rNext - rCurrent) * ratio;
                var curCoord = LinearTool.SlipOnCurve(curPath.Coordinates, ref towards, curRatioOnPath);
                var curPoint = dParams.GetPoint(curCoord);
                g.DrawEllipse(pen, curPoint.X - dParams.VehicleRadius, curPoint.Y - dParams.VehicleRadius, dParams.VehicleRadius * 2, dParams.VehicleRadius * 2);

                // draw destination
                var destPoint = dParams.GetPoint(PMScenario.GetCoord(v.Targets.Last(), ref towards));
                g.DrawRectangle(pen, destPoint.X - dParams.VehicleRadius, destPoint.Y - dParams.VehicleRadius, dParams.VehicleRadius * 2, dParams.VehicleRadius * 2);
                
                // draw vehicle direction
                if ((now - v.DepartureTime).TotalSeconds < 5)
                {
                    var pen2 = new Pen(vColor, 5); // for vehicle direction

                    var next = v.Next;
                    var coords = new List<DenseVector>();
                    coords.AddRange(LinearTool.GetCoordsInRange(curPath.Coordinates, curRatioOnPath, next.Positions[curPath] / curPath.Length));
                    foreach (var target in v.Targets)
                    {
                        while (next != target)
                        {
                            var curCP = next;
                            next = next.RoutingTable[target];
                            var p = curCP.PathingTable[next];
                            coords.AddRange(LinearTool.GetCoordsInRange(p.Coordinates, curCP.Positions[p] / p.Length, next.Positions[p] / p.Length));
                        }
                    }
                    foreach (var coord in coords)
                    {
                        var destination = dParams.GetPoint(coord);
                        g.DrawLine(pen2, curPoint, destination);
                        curPoint = destination;
                    }
                }
            }
        }

        #endregion        
    }
}
