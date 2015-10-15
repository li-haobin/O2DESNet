using O2DESNet.PathMover.Statics;
using System;
using System.Collections.Generic;

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
        public ControlPoint Current { get; internal set; }
        public ControlPoint Towards { get; internal set; }
        public double Speed { get; internal set; }
        public double TargetSpeed { get; internal set; }
        public double Acceleration { get; internal set; }

        public double DistanceToTravel { get; internal set; }
        public double TimeToTargetSpeed { get; private set; }
        public double DistanceToTargetSpeed { get; private set; }
        public double EndingSpeed { get; internal set; }
        public double EndingTime { get; private set; }

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
                EndingTime = TimeToTargetSpeed + (DistanceToTravel - DistanceToTargetSpeed) / TargetSpeed;
            }
            else
            {
                EndingSpeed = Math.Sqrt(Speed * Speed + Acceleration * DistanceToTravel * 2);
                EndingTime = (EndingSpeed - Speed) / Acceleration;
            }
        }        
    }
}
