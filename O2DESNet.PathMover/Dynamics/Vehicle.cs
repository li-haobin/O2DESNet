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

        public double Distance_ToNextControlPoint { get; internal set; }
        public double Distance_ToTargetSpeed { get; private set; }
        public double Time_ToNextControlPoint { get; private set; }
        public double Time_ToTargetSpeed { get; private set; }
        public double Speed_ToNextControlPoint { get; internal set; }        

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
            Time_ToTargetSpeed = (TargetSpeed - Speed) / Acceleration;
            if (Time_ToTargetSpeed < 0) Time_ToTargetSpeed = double.PositiveInfinity;
            Distance_ToTargetSpeed = Speed * Time_ToTargetSpeed + Acceleration * Time_ToTargetSpeed * Time_ToTargetSpeed / 2;
            if (Distance_ToTargetSpeed < Distance_ToNextControlPoint)
            {
                Speed_ToNextControlPoint = TargetSpeed;
                Time_ToNextControlPoint = Time_ToTargetSpeed + (Distance_ToNextControlPoint - Distance_ToTargetSpeed) / TargetSpeed;
            }
            else
            {
                Speed_ToNextControlPoint = Math.Sqrt(Speed * Speed + Acceleration * Distance_ToNextControlPoint * 2);
                Time_ToNextControlPoint = (Speed_ToNextControlPoint - Speed) / Acceleration;
            }
        }
        public double GetDistance(DateTime time)
        {
            var t = (time - LastTime).TotalSeconds;
            if (t < 0 || t > Time_ToNextControlPoint) throw new Exception("Given time is out of range.");
            if (t < Time_ToTargetSpeed) return Speed * t + Acceleration * t * t / 2;
            else return (TargetSpeed * TargetSpeed - Speed * Speed) / Acceleration / 2 + TargetSpeed * (t - Time_ToTargetSpeed);
        }

        public override string ToString()
        {
            var str = string.Format("[Vehicle #{0}]\n", Id);
            str += string.Format("CP#{0}->#{1} since {2}.\n", LastControlPoint.Id, NextControlPoint.Id, LastTime);
            str += string.Format("with speed {0:F4}->{1:F4} m/s \nand acc. {2:F4} m/s2.", Speed, TargetSpeed, Acceleration);
            return str;
        }

        #region Identify passing-over & crossing-over another vehicle
        internal DateTime[] GetTimes_PassOver(Vehicle vehicle)
        {
            double v11, v12, v21, v22, t12, t21, t22, tMax, a1, a2;
            v11 = Speed; v12 = TargetSpeed; v21 = vehicle.Speed; v22 = vehicle.TargetSpeed;
            t12 = Time_ToTargetSpeed;
            t21 = (vehicle.LastTime - LastTime).TotalSeconds; t22 = t21 + vehicle.Time_ToTargetSpeed;
            tMax = Math.Min(Time_ToNextControlPoint, t21 + vehicle.Time_ToNextControlPoint);
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
            return times.Select(t => LastTime.AddSeconds(t)).ToArray();
        }
        internal DateTime GetTime_CrossOver(Vehicle vehicle)
        {
            double v11, v12, v21, v22, t12, t21, t22, a1, a2, s;
            v11 = Speed; v12 = TargetSpeed; v21 = vehicle.Speed; v22 = vehicle.TargetSpeed;
            t12 = Time_ToTargetSpeed;
            t21 = (vehicle.LastTime - LastTime).TotalSeconds; t22 = t21 + vehicle.Time_ToTargetSpeed;
            s = Distance_ToNextControlPoint;
            a1 = Acceleration; a2 = vehicle.Acceleration;
            double[] times;
            if (t22 < 0)
            {
                times = SolveCrossOverEq14(v11, v21, v22, t22, a1, a2, s).Where(t => t < t12).Concat(
                    SolveCrossOverEq24(v11, v12, v21, v22, t12, t22, a1, a2, s).Where(t => t12 <= t)).ToArray();

            }
            else if (t22 < t12)
            {
                times = SolveCrossOverEq13(v11, v21, t21, a1, a2, s).Where(t => t < t22).Concat(
                    SolveCrossOverEq14(v11, v21, v22, t22, a1, a2, s).Where(t => t22 <= t && t < t12)).Concat(
                    SolveCrossOverEq24(v11, v12, v21, v22, t12, t22, a1, a2, s).Where(t => t12 <= t)).ToArray();
            }
            else
            {
                times = SolveCrossOverEq13(v11, v21, t21, a1, a2, s).Where(t => t < t12).Concat(
                   SolveCrossOverEq23(v11, v12, v21, t12, t21, a1, a2, s).Where(t => t12 <= t && t < t22)).Concat(
                    SolveCrossOverEq24(v11, v12, v21, v22, t12, t22, a1, a2, s).Where(t => t22 <= t)).ToArray();
            }
            //if (times.Length > 1) throw new Exception(); // for validation only
            return LastTime.AddSeconds(times.First());
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
        private static IEnumerable<double> SolveCrossOverEq13(double v11, double v21, double t21, double a1, double a2, double s)
        {
            return Quadratic.Solve((a1 + a2) / 2, v11 + v21 - a2 * t21, a2 * t21 * t21 / 2 - v21 * t21 - s).Where(t => t >= 0);
        }
        private static IEnumerable<double> SolveCrossOverEq14(double v11, double v21, double v22, double t22, double a1, double a2, double s)
        {
            return Quadratic.Solve(a1 / 2, v11 + v22, (v22 * v22 - v21 * v21) / (a2 * 2) - v22 * t22 - s).Where(t => t >= 0);
        }
        private static IEnumerable<double> SolveCrossOverEq23(double v11, double v12, double v21, double t12, double t21, double a1, double a2, double s)
        {
            return Quadratic.Solve(a2 / 2, v21 + v12 - a2 * t21, a2 * t21 * t21 / 2 - v12 * t12 - v21 * t21 + (v12 * v12 - v11 * v11) / (a1 * 2) - s).Where(t => t >= 0);
        }
        private static IEnumerable<double> SolveCrossOverEq24(double v11, double v12, double v21, double v22, double t12, double t22, double a1, double a2, double s)
        {
            return new double[] { (s - (v22 * v22 - v21 * v21) / (a2 * 2) - (v12 * v12 - v11 * v11) / (a1 * 2) + v12 * t12 + v22 * t22) / (v12 + v22) }.Where(t => t >= 0);
        }
        #endregion


    }
}
