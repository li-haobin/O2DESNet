using O2DESNet.PathMover.Methods;
using O2DESNet.PathMover.Statics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace O2DESNet.PathMover.Dynamics
{
    public class Vehicle
    {
        private static int _count = 0;
        public int Id { get; private set; }
        public VehicleType Type { get; private set; }
        internal Vehicle(VehicleType type) { Id = ++_count; Type = type; On = false; HistoricalPath = new List<ControlPoint>(); }

        /// <summary>
        /// Whether the vehicle is activated in the path mover system. If set to "false", it does not incur conflicts at any time.
        /// </summary>
        public bool On { get; internal set; }
        public DateTime LastTime { get; internal set; }
        public ControlPoint LastControlPoint { get; internal set; }
        public ControlPoint NextControlPoint { get; internal set; }
        public double Speed { get; internal set; }
        public double TargetSpeed { get; internal set; }
        public double Acceleration { get; internal set; }

        public double DistanceToTravel { get; internal set; }
        public double TimeToTargetSpeed { get; private set; }
        public double DistanceToTargetSpeed { get; private set; }
        public double EndingSpeed { get; internal set; }
        public double TimeToEnd { get; private set; }

        public Dictionary<Vehicle, DateTime> Conflicts_PassingOver;

        public List<ControlPoint> HistoricalPath { get; private set; }

        internal void SpeedControl(double? targetSpeed = null, double? acceleration = null)
        {
            if (targetSpeed != null)
            {
                TargetSpeed = Math.Min(Type.MaxSpeed, targetSpeed.Value); // set within limit
                if (acceleration != null)
                    Acceleration = Math.Max(-Type.MaxDeceleration, Math.Min(Type.MaxAcceleration, acceleration.Value)); // set within limits
                else
                { // use max acceleration / deceleration
                    if (TargetSpeed > Speed) Acceleration = Type.MaxAcceleration;
                    else if (TargetSpeed < Speed) Acceleration = -Type.MaxDeceleration;
                }
            }
        }
        internal void ForwardCalculate()
        {
            TimeToTargetSpeed = (TargetSpeed - Speed) / Acceleration;
            if (TimeToTargetSpeed < 0) TimeToTargetSpeed = double.PositiveInfinity;
            DistanceToTargetSpeed = Speed * TimeToTargetSpeed + Acceleration * TimeToTargetSpeed * TimeToTargetSpeed / 2;
            if (DistanceToTargetSpeed < DistanceToTravel)
            {
                EndingSpeed = TargetSpeed;
                TimeToEnd = TimeToTargetSpeed + (DistanceToTravel - DistanceToTargetSpeed) / TargetSpeed;
            }
            else
            {
                EndingSpeed = Math.Sqrt(Speed * Speed + Acceleration * DistanceToTravel * 2);
                TimeToEnd = (EndingSpeed - Speed) / Acceleration;
            }
        }
        public double GetDistance(DateTime time)
        {
            var t = (time - LastTime).TotalSeconds;
            if (t < 0 || t > TimeToEnd) throw new Exception("Given time is out of range.");
            if (t < TimeToTargetSpeed) return Speed * t + Acceleration * t * t / 2;
            else return (TargetSpeed * TargetSpeed - Speed * Speed) / Acceleration / 2 + TargetSpeed * (t - TimeToTargetSpeed);
        }
        #region Identify conflict of passing-Over another vehicle
        internal DateTime[] GetTime_PassingOver(Vehicle vehicle)
        {
            double v11, v12, v21, v22, t12, t21, t22, tMax, a1, a2;
            v11 = Speed; v12 = TargetSpeed; v21 = vehicle.Speed; v22 = vehicle.TargetSpeed;
            t12 = TimeToTargetSpeed;
            t21 = (vehicle.LastTime - LastTime).TotalSeconds; t22 = t21 + vehicle.TimeToTargetSpeed;
            tMax = Math.Min(TimeToEnd, t21 + vehicle.TimeToEnd);
            a1 = Acceleration; a2 = vehicle.Acceleration;
            double[] times;
            if (t22 < 0)
            {
                times = SolvePassOverEq14(v11, v21, v22, t22, a1, a2, tMax).Where(t => t < t12).Concat(
                    SolvePassOverEq24(v11, v12, v21, v22, t12, t22, a1, a2, tMax).Where(t => t12 <= t)).ToArray();

            }
            else if (t22 < t12)
            {
                times = SolvePassOverEq13(v11, v21, t21, a1, a2, tMax).Where(t => t < t22).Concat(
                    SolvePassOverEq14(v11, v21, v22, t22, a1, a2, tMax).Where(t => t22 <= t && t < t12)).Concat(
                    SolvePassOverEq24(v11, v12, v21, v22, t12, t22, a1, a2, tMax).Where(t => t12 <= t)).ToArray();
            }
            else
            {
                times = SolvePassOverEq13(v11, v21, t21, a1, a2, tMax).Where(t => t < t12).Concat(
                   SolvePassOverEq23(v11, v12, v21, t12, t21, a1, a2, tMax).Where(t => t12 <= t && t < t22)).Concat(
                    SolvePassOverEq24(v11, v12, v21, v22, t12, t22, a1, a2, tMax).Where(t => t22 <= t)).ToArray();
            }
            return times.OrderBy(t => t).Select(t => LastTime.AddSeconds(t)).ToArray();
        }
        private static IEnumerable<double> SolvePassOverEq13(double v11, double v21, double t21, double a1, double a2, double tMax)
        {
            return Quadratic.Solve((a1 - a2) / 2, v11 - v21 + a2 * t21, v21 * t21 - a2 * t21 * t21 / 2).Where(t => t >= 0 && t < tMax);
        }
        private static IEnumerable<double> SolvePassOverEq14(double v11, double v21, double v22, double t22, double a1, double a2, double tMax)
        {
            return Quadratic.Solve(a1 / 2, v11 - v22, v22 * t22 - (v22 * v22 - v21 * v21) / (a2 * 2)).Where(t => t >= 0 && t < tMax);
        }
        private static IEnumerable<double> SolvePassOverEq23(double v11, double v12, double v21, double t12, double t21, double a1, double a2, double tMax)
        {
            return Quadratic.Solve(a2 / 2, v21 - v12 - a2 * t21, v12 * t12 - v21 * t21 + a2 * t21 * t21 / 2 - (v12 * v12 - v11 * v11) / (a1 * 2)).Where(t => t >= 0 && t < tMax);
        }
        private static IEnumerable<double> SolvePassOverEq24(double v11, double v12, double v21, double v22, double t12, double t22, double a1, double a2, double tMax)
        {
            return new double[] { ((v22 * v22 - v21 * v21) / (a2 * 2) - (v12 * v12 - v11 * v11) / (a1 * 2) + v12 * t12 - v22 * t22) / (v12 - v22) }.Where(t => t >= 0 && t < tMax);
        }
        #endregion

        public override string ToString()
        {
            var str = string.Format("[Vehicle #{0}]\n", Id);
            str += string.Format("CP#{0}->#{1} since {2}.\n", LastControlPoint.Id, NextControlPoint.Id, LastTime);
            str += string.Format("with speed {0:F4}->{1:F4} m/s \nand acc. {2:F4} m/s2.", Speed, TargetSpeed, Acceleration);
            return str;
        }
    }
}
