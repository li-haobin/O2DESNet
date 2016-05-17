using O2DESNet.PathMover.Statics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.PathMover.Dynamics
{
    public class Vehicle
    {
        public ControlPoint Current { get; private set; } = null;
        public ControlPoint Next { get; private set; } = null;
        public double RemainingRatio { get; private set; } = 0;
        public DateTime LastActionTime { get; private set; }
        public double Speed { get; private set; } = 0; // m/s
        public DateTime? TimeToReach { get; private set; }
        
        internal Vehicle(ControlPoint start, DateTime clockTime)
        {
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
            if (Speed <= 0) throw new Exception("Vechicle speed has not beed set.");
            Next = next;
            RemainingRatio = 1;
            LastActionTime = clockTime;
            CalTimeToReach();
        }

        /// <summary>
        /// /// <summary>
        /// Update the speed of the vehicle
        /// </summary>
        /// <param name="clockTime">Must NOT be null if the vehicle is moving</param>
        public void SetSpeed(double speed, DateTime? clockTime = null)
        {
            if (Next != null)
            {
                RemainingRatio = 1 - Speed * (clockTime.Value - LastActionTime).TotalSeconds / Current.GetDistanceTo(Next);
                if (RemainingRatio < 0) throw new Exception("Vehicle has already reached next control point.");
                CalTimeToReach();
                LastActionTime = clockTime.Value;
            }
            Speed = speed;
        }

        /// <summary>
        /// Change the status of vehicle as reached the next control point
        /// </summary>
        public void Reach(DateTime clockTime)
        {
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

    }
}
