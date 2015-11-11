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
        private double _epsilon = 0.00001;
        private Status _status { get; set; }
        public int Id { get; private set; }
        public VehicleType Type { get; private set; }
        internal Vehicle(Status status, VehicleType type) { _status = status; Id = ++_count; Type = type; On = false; HistoricalPath = new List<ControlPoint>(); }
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

        internal void PutOn(ControlPoint controlPoint)
        {
            if (On)
                throw new Exceptions.VechicleStatusError(this, "The vehicle is on already.");
            On = true;
            LastTime = _status._sim.ClockTime;
            LastControlPoint = controlPoint;
            NextControlPoint = null;
            Speed_ToNextControlPoint = 0;
            TargetSpeed = 0;
            Acceleration = 0;
            _status.OffVehicles.Remove(this);
            _status.OutgoingVehicles[controlPoint].Add(this);
            _status.VehicleCounters[controlPoint].ObserveChange(1);
        }
        internal void PutOff()
        {
            _status.OutgoingVehicles[LastControlPoint].Remove(this);
            _status.IncomingVehicles[NextControlPoint].Remove(this);
            _status.OffVehicles.Add(this);
            _status.VehicleCounters[LastControlPoint].ObserveChange(-1);
            LastControlPoint = null;
            NextControlPoint = null;
            On = false;
        }
        internal void MoveToNext(ControlPoint nextControlPoint, double? targetSpeed = null, double? acceleration = null)
        {
            //HistoricalPath.Add(LastControlPoint);
            Distance_ToNextControlPoint = LastControlPoint.GetDistanceTo(nextControlPoint);
            _status.IncomingVehicles[nextControlPoint].Add(this);
            NextControlPoint = nextControlPoint;

            // to be combined
            SpeedControl(targetSpeed, acceleration);

            ForwardCalculate();
            //Console.WriteLine("{0}:\tV{1} moved CP{2} -> CP{3} at S{4:F4}/{5:F4} & A{6}", _status._sim.ClockTime.ToString("yyyy/MM/dd HH:mm:ss"), Id, LastControlPoint.Id, NextControlPoint.Id, Speed, TargetSpeed, Acceleration);
            _status.VehicleCounters[nextControlPoint].ObserveChange(1);
        }
        internal void MoveToNext(ControlPoint nextControlPoint, double bufferTime, out bool withinBuffer)
        {
            //HistoricalPath.Add(LastControlPoint);
            Distance_ToNextControlPoint = LastControlPoint.GetDistanceTo(nextControlPoint);
            _status.IncomingVehicles[nextControlPoint].Add(this);
            NextControlPoint = nextControlPoint;

            // to be combined
            SpeedControl(bufferTime: bufferTime, wihtinBuffer: out withinBuffer);

            ForwardCalculate();
            //Console.WriteLine("{0}:\tV{1} moved CP{2} -> CP{3} at S{4:F4}/{5:F4} & A{6}", _status._sim.ClockTime.ToString("yyyy/MM/dd HH:mm:ss"), Id, LastControlPoint.Id, NextControlPoint.Id, Speed, TargetSpeed, Acceleration);
            _status.VehicleCounters[nextControlPoint].ObserveChange(1);
        }
        internal void Reach()
        {
            Speed = Speed_ToNextControlPoint;
            _status.OutgoingVehicles[LastControlPoint].Remove(this);
            _status.VehicleCounters[LastControlPoint].ObserveChange(-1);
            _status.IncomingVehicles[NextControlPoint].Remove(this);
            _status.OutgoingVehicles[NextControlPoint].Add(this);
            LastTime = _status._sim.ClockTime;
            LastControlPoint = NextControlPoint;
            NextControlPoint = null;
        }
        public double GetDistance(DateTime time)
        {
            var t = (time - LastTime).TotalSeconds;
            if (t < 0 || t > Time_ToNextControlPoint) throw new Exception("Given time is out of range.");
            if (t < Time_ToTargetSpeed) return Speed * t + Acceleration * t * t / 2;
            else return (TargetSpeed * TargetSpeed - Speed * Speed) / Acceleration / 2 + TargetSpeed * (t - Time_ToTargetSpeed);
        }
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
        internal void SpeedControl(double bufferTime, out bool wihtinBuffer)
        {
            wihtinBuffer = true;
            double t_min, t_max, time_ToNextControlPoint;
            double s = Distance_ToNextControlPoint, v1 = Speed, v_max = Type.MaxSpeed;
            var distance_ToMaxSpeed = (v_max * v_max - v1 * v1) / Type.MaxAcceleration / 2;
            if (distance_ToMaxSpeed >= s) t_min = (Math.Sqrt(v1 * v1 + Type.MaxAcceleration * s * 2) - v1) / Type.MaxAcceleration;
            else t_min = (v_max - v1) / Type.MaxAcceleration + (s - distance_ToMaxSpeed) / v_max;
            var v2_square = v1 * v1 - Type.MaxDeceleration * s * 2;
            if (v2_square < 0) t_max = double.PositiveInfinity;
            else t_max = (v1 - Math.Sqrt(v2_square)) / Type.MaxDeceleration;

            var arrivalTimes = _status.IncomingVehicles[NextControlPoint].Select(v => (v.LastTime - LastTime).TotalSeconds + v.Time_ToNextControlPoint).Where(t => t + bufferTime > t_min && t - bufferTime < t_max).OrderBy(t => t).ToArray();
            var blackouts = arrivalTimes.Select(t => new double[] { t - bufferTime, t + bufferTime }).ToList();

            // to prevent conflict of passover
            var vehicleToPassOver = _status.IncomingVehicles[NextControlPoint].Intersect(_status.OutgoingVehicles[LastControlPoint]).OrderBy(v => v.Time_ToNextControlPoint).LastOrDefault();
            if (vehicleToPassOver != null) blackouts.Insert(0, new double[] { 0, (vehicleToPassOver.LastTime - LastTime).TotalSeconds + vehicleToPassOver.Time_ToNextControlPoint + bufferTime }); 

            if (blackouts.Count == 0 || t_min <= blackouts[0][0]) time_ToNextControlPoint = t_min;
            else
            {
                // combine overlapping blackouts with 1st 
                while (blackouts.Count > 1)
                    if (blackouts[0][1] >= blackouts[1][0])
                    {
                        blackouts[0][1] = blackouts[1][1];
                        blackouts.RemoveAt(1);
                    }
                    else break;
                if (blackouts[0][1] <= t_max) time_ToNextControlPoint = blackouts[0][1];
                else
                {
                    List<double[]> buffers = new List<double[]>();
                    buffers.Add(new double[] { t_min, arrivalTimes.Min(a => Math.Abs(a - t_min)) });
                    buffers.Add(new double[] { t_max, arrivalTimes.Min(a => Math.Abs(a - t_max)) });

                    arrivalTimes = arrivalTimes.Where(t => t >= t_min && t <= t_max).ToArray();
                    if (arrivalTimes.Length > 0) buffers.AddRange(Enumerable.Range(0, arrivalTimes.Length - 1).Select(i => new double[] { (arrivalTimes[i] + arrivalTimes[i + 1]) / 2, (arrivalTimes[i + 1] - arrivalTimes[i]) / 2 }));
                    time_ToNextControlPoint = buffers.OrderBy(b => b[1]).First()[0];
                    wihtinBuffer = false;
                }
            }
            var targetSpeed = GetTargetSpeed(time_ToNextControlPoint);
            if (targetSpeed >= Speed) SpeedControl(targetSpeed, Type.MaxAcceleration);
            else SpeedControl(targetSpeed, -Type.MaxDeceleration);
        }
        private double GetTargetSpeed(double time_ToNextControlPoint)
        {
            var roots_acceleration = Quadratic.Solve(0.5 / Type.MaxAcceleration, -time_ToNextControlPoint - Speed / Type.MaxAcceleration, Speed * Speed / Type.MaxAcceleration / 2 + Distance_ToNextControlPoint, _epsilon).Where(v => v > Speed - _epsilon && v < Speed + Type.MaxAcceleration * time_ToNextControlPoint + _epsilon);
            var roots_deceleration = Quadratic.Solve(0.5 / -Type.MaxDeceleration, -time_ToNextControlPoint + Speed / Type.MaxDeceleration, Speed * Speed / -Type.MaxDeceleration / 2 + Distance_ToNextControlPoint, _epsilon).Where(v => v >= 0 && v < Speed + _epsilon && v > Speed - Type.MaxDeceleration * time_ToNextControlPoint - _epsilon);
            var targetSpeeds = roots_acceleration.Concat(roots_deceleration).ToArray();

            var r1 = Quadratic.Solve(0.5 / Type.MaxAcceleration, -time_ToNextControlPoint - Speed / Type.MaxAcceleration, Speed * Speed / Type.MaxAcceleration / 2 + Distance_ToNextControlPoint, _epsilon).ToArray();
            var r2 = Quadratic.Solve(0.5 / -Type.MaxDeceleration, -time_ToNextControlPoint + Speed / Type.MaxDeceleration, Speed * Speed / -Type.MaxDeceleration / 2 + Distance_ToNextControlPoint, _epsilon).ToArray();

            if (targetSpeeds.Length > 1 && targetSpeeds.Max() - targetSpeeds.Min() > _epsilon) Console.WriteLine();
            return targetSpeeds[0];

        }
        private void ForwardCalculate()
        {
            Time_ToTargetSpeed = (TargetSpeed - Speed) / Acceleration;
            if (Time_ToTargetSpeed < 0) Time_ToTargetSpeed = double.PositiveInfinity;
            Distance_ToTargetSpeed = Speed * Time_ToTargetSpeed + Acceleration * Time_ToTargetSpeed * Time_ToTargetSpeed / 2;
            if (Distance_ToTargetSpeed < Distance_ToNextControlPoint - _epsilon)
            {
                Speed_ToNextControlPoint = TargetSpeed;
                Time_ToNextControlPoint = Time_ToTargetSpeed + (Distance_ToNextControlPoint - Distance_ToTargetSpeed) / TargetSpeed;
            }
            else
            {
                var vSquare = Speed * Speed + Acceleration * Distance_ToNextControlPoint * 2;
                if (vSquare > -_epsilon * _epsilon && vSquare < _epsilon * _epsilon) vSquare = 0;
                Speed_ToNextControlPoint = Math.Sqrt(vSquare);
                Time_ToNextControlPoint = (Speed_ToNextControlPoint - Speed) / Acceleration;
                if (Time_ToNextControlPoint == double.NegativeInfinity || double.IsNaN(Time_ToNextControlPoint)) Console.WriteLine();
            }
            if (Time_ToNextControlPoint == double.NegativeInfinity || double.IsNaN(Time_ToNextControlPoint)) Console.WriteLine();
        }
        

        public override string ToString()
        {
            var str = string.Format("[Vehicle #{0}]\n", Id);
            str += string.Format("CP#{0}->#{1} since {2}.\n", LastControlPoint.Id, NextControlPoint.Id, LastTime);
            str += string.Format("with speed {0:F4}->{1:F4} m/s \nand acc. {2:F4} m/s2.", Speed, TargetSpeed, Acceleration);
            return str;
        }
                
        internal bool IdentifyConflicts_PassOver()
        {
            _status.Conflicts_PassOver[this] = new Dictionary<Vehicle, DateTime[]>();
            foreach (var v in _status.OutgoingVehicles[LastControlPoint].Intersect(_status.IncomingVehicles[NextControlPoint]))
                if (v != this)
                {
                    var times = GetTimes_PassOver(v);
                    if (times.Length > 0) _status.Conflicts_PassOver[this].Add(v, times);
                }
            return _status.Conflicts_PassOver[this].Count > 0;
        }
        internal bool IdentifyConflicts_CrossOver()
        {
            _status.Conflicts_CrossOver[this] = new Dictionary<Vehicle, DateTime>();
            foreach (var v in _status.OutgoingVehicles[NextControlPoint].Intersect(_status.IncomingVehicles[LastControlPoint]))
                _status.Conflicts_CrossOver[this].Add(v, GetTime_CrossOver(v));
            return _status.Conflicts_CrossOver[this].Count > 0;
        }

        #region Identify passing-over & crossing-over another vehicle
        private DateTime[] GetTimes_PassOver(Vehicle vehicle)
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
        private DateTime GetTime_CrossOver(Vehicle vehicle)
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
