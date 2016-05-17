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
        public Status Status { get; private set; }
        public int Id { get; private set; }
        public ControlPoint Current { get; private set; } = null;
        public ControlPoint Next { get; private set; } = null;
        public double RemainingRatio { get; private set; } = 0;
        public DateTime LastActionTime { get; private set; }
        public double Speed { get; private set; } = 0; // m/s
        public DateTime? TimeToReach { get; private set; }

        public ControlPoint Target { get; set; }
        public Action OnTarget { get; set; }

        internal Vehicle(Status status, ControlPoint start, DateTime clockTime)
        {
            Status = status;
            Id = Status.Vehicles.Count;
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
            Status.VehiclesOnPath[Current.PathingTable[next]].Add(this);
            Status.UpdateSpeeds(Current.PathingTable[next], clockTime);
            Next = next;
            RemainingRatio = 1;
            LastActionTime = clockTime;            
            CalTimeToReach();
        }

        /// <summary>
        /// /// <summary>
        /// Update the speed of the vehicle
        /// </summary>
        internal void SetSpeed(double speed, DateTime clockTime)
        {
            if (Next != null && speed != Speed)
            {
                RemainingRatio -= Speed * (clockTime - LastActionTime).TotalSeconds / Current.GetDistanceTo(Next);
                if (RemainingRatio < 0) throw new Exception("Vehicle has already reached next control point.");
                Speed = speed;
                LastActionTime = clockTime;
                CalTimeToReach();
            }
            else Speed = speed;
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

        public override string ToString()
        {
            return string.Format("V{0}", Id);
        }

    }
}
