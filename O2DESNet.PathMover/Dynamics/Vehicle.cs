using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.PathMover
{
    public class Vehicle
    {
        public PMStatus Status { get; private set; }
        public int Id { get; private set; }
        public ControlPoint Current { get; private set; } = null;
        public ControlPoint Next { get; private set; } = null;
        public double RemainingRatio { get; private set; } = 0;
        public DateTime LastActionTime { get; private set; }
        public double Speed { get; private set; } = 0; // m/s
        public DateTime? TimeToReach { get; private set; }
        public Path Path
        {
            get
            {
                if (Current == null || Next == null) return null;
                return Current.PathingTable[Current.RoutingTable[Next]];
            }
        }

        public ControlPoint Origin { get; set; }
        public DateTime DepartureTime { get; set; }
        public List<ControlPoint> Targets { get; set; }

        public Action OnMove { get; set; }
        public Action OnReach { get; set; }
        public Action OnCompletion { get; set; }

        public DenseVector Direction { get; set; } // for drawing

        internal protected Vehicle(PMStatus status, ControlPoint start, DateTime clockTime)
        {
            Status = status;
            Id = Status.VehicleId++;
            Current = start;
            LastActionTime = clockTime;
            TimeToReach = null;
        }

        /// <summary>
        /// Change status of vehicle as moving to the next control point
        /// </summary>
        /// <param name="next">A control point next to the current one</param>
        public void Move(ControlPoint next, DateTime clockTime)
        {
            Next = next;
            RemainingRatio = 1;
            Status.VehiclesOnPath[Current.PathingTable[next]].Add(this);
            Status.UpdateSpeeds(Current.PathingTable[next], clockTime);            
            LastActionTime = clockTime;
            if (TimeToReach == null) CalTimeToReach();
        }

        /// <summary>
        /// /// <summary>
        /// Update the speed of the vehicle
        /// </summary>
        public void SetSpeed(double speed, DateTime clockTime)
        {
            if (Next != null && speed != Speed)
            {
                if (clockTime > LastActionTime)
                    RemainingRatio -= Speed * (clockTime - LastActionTime).TotalSeconds / Current.GetDistanceTo(Next);
                if (RemainingRatio < 0)
                {
                    if (RemainingRatio > -1E-3) RemainingRatio = 0;
                    else throw new Exception("Vehicle has already reached next control point.");
                }
                Speed = speed;
                LastActionTime = clockTime;
                CalTimeToReach();
            }
            else Speed = speed;
        }

        public void SetTimeToReach(DateTime timeToReach, DateTime clockTime)
        {
            if (timeToReach < clockTime) throw new Exception("Time to reach cannot be earlier than the clock time.");
            if (Next != null && timeToReach != TimeToReach)
            {
                var dist = Current.GetDistanceTo(Next);
                RemainingRatio -= Speed * (clockTime - LastActionTime).TotalSeconds / Current.GetDistanceTo(Next);
                if (RemainingRatio < 0)
                {
                    if (RemainingRatio > -1E-3) RemainingRatio = 0;
                    else throw new Exception("Vehicle has already reached next control point.");
                }
                TimeToReach = timeToReach;
                LastActionTime = clockTime;
                CalSpeed();
                //if (Speed > Current.PathingTable[Next].FullSpeed)
                //    throw new Exception("Speed excceds the limit.");
            }
        }

        /// <summary>
        /// Change the status of vehicle as reached the next control point
        /// </summary>
        public void Reach(DateTime clockTime)
        {
            Status.VehiclesOnPath[Current.PathingTable[Next]].Remove(this);
            Current = Next;
            Next = null;
            RemainingRatio = 0;
            LastActionTime = clockTime;
            TimeToReach = null;
        }

        private void CalTimeToReach()
        {
            TimeToReach = LastActionTime + TimeSpan.FromSeconds(Current.GetDistanceTo(Next) * RemainingRatio / Speed);
        }
        private void CalSpeed()
        {
            Speed = Current.GetDistanceTo(Next) * RemainingRatio / (TimeToReach.Value - LastActionTime).TotalSeconds;
        }

        public override string ToString()
        {
            return string.Format("V{0}", Id);
        }

        #region For Display
        public string GetStr_Status()
        {
            string str = ToString();
            str += string.Format(" {0}->{1} ", Current, Next);
            foreach (var cp in Targets) str += ":" + cp;
            return str;
        }
        #endregion

        private static List<Color> _colors = new List<Color> {
            Color.DarkBlue, Color.DarkCyan, Color.DarkGoldenrod, Color.DarkGreen, Color.DarkRed, Color.DarkSeaGreen,
            Color.DarkKhaki, Color.DarkMagenta, Color.DarkOliveGreen, Color.DarkOrange, Color.DarkOrchid, Color.DarkSalmon,
            Color.DarkSlateBlue, Color.DarkTurquoise, Color.DarkViolet
        };

        public virtual void Draw(Graphics g, DrawingParams dParams, PMScenario pm, DateTime now)
        {
            var pen = new Pen(dParams.VehicleColor, dParams.VehicleBorder); // for vehicle

            DenseVector towards = null;
            var vColor = _colors[Id % _colors.Count];
            var start = pm.GetCoord(Current, ref towards);
            var end = pm.GetCoord(Next, ref towards);

            var ratio = Math.Min(1, 1 - RemainingRatio + (now - LastActionTime).TotalSeconds / (TimeToReach.Value - LastActionTime).TotalSeconds);

            var curPath = Current.PathingTable[Next];
            var rCurrent = Current.Positions[curPath] / curPath.Length;
            var rNext = Next.Positions[curPath] / curPath.Length;

            // draw vehicle shape                
            pen.Color = vColor;
            var curRatioOnPath = rCurrent + (rNext - rCurrent) * ratio;
            var curCoord = LinearTool.SlipOnCurve(curPath.Coordinates, ref towards, curRatioOnPath);
            if (Direction == null || !curPath.Crab) Direction = (DenseVector)(towards - curCoord).Normalize(2);
            DrawShape(g, dParams, pen, curCoord);            

            // draw destination
            var destPoint = dParams.GetPoint(pm.GetCoord(Targets.Last(), ref towards));
            g.DrawRectangle(pen, destPoint.X - dParams.VehicleRadius, destPoint.Y - dParams.VehicleRadius, dParams.VehicleRadius * 2, dParams.VehicleRadius * 2);

            // draw vehicle direction
            var curPoint = dParams.GetPoint(curCoord);
            if ((now - DepartureTime).TotalSeconds < 5)
            {
                var pen2 = new Pen(vColor, 5); // for vehicle direction

                var next = Next;
                var coords = new List<DenseVector>();
                coords.AddRange(LinearTool.GetCoordsInRange(curPath.Coordinates, curRatioOnPath, next.Positions[curPath] / curPath.Length));
                foreach (var target in Targets)
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

        protected virtual void DrawShape(Graphics g, DrawingParams dParams, Pen pen, DenseVector curCoord)
        {
            var curPoint = dParams.GetPoint(curCoord);
            g.DrawEllipse(pen, curPoint.X - dParams.VehicleRadius, curPoint.Y - dParams.VehicleRadius, dParams.VehicleRadius * 2, dParams.VehicleRadius * 2);
        }       

    }
}
