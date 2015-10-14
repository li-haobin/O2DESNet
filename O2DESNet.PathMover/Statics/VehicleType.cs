using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.PathMover
{
    public class VehicleType
    {
        public int Id { get; private set; }
        public double MaxSpeed { get; private set; }
        public double MaxAcceleration { get; private set; }
        public double MaxDeceleration { get; private set; }

        internal VehicleType(int id, double maxSpeed, double maxAcceleration, double maxDeceleration)
        {
            Id = id;
            if (maxSpeed <= 0 || maxSpeed == double.PositiveInfinity)
                throw new Exceptions.InfeasibleConstruction(
                    "Vehicle max speed must be positive finite.");
            if (maxAcceleration <= 0 || maxAcceleration == double.PositiveInfinity)
                throw new Exceptions.InfeasibleConstruction(
                    "Vehicle max acceleration must be positive finite.");
            if (maxDeceleration <= 0 || maxDeceleration == double.PositiveInfinity)
                throw new Exceptions.InfeasibleConstruction(
                    "Vehicle max deceleration must be positive finite.");
            MaxSpeed = maxSpeed;
            MaxAcceleration = maxAcceleration;
            MaxDeceleration = maxDeceleration;
        }
        /// <summary>
        /// Get shortest time traveling between two adjacent control points, given the start and end speed.
        /// </summary>
        /// <param name="peakSpeed">the peak speed achieved in the journey</param>
        public double GetShortestTravelingTime(ControlPoint from, ControlPoint to, double startSpeed, double endSpeed, out double peakSpeed)
        {
            var distance = from.GetDistanceTo(to);
            TravelingFeasibilityCheck(distance, startSpeed, endSpeed);
            var speedLimit = Math.Min(MaxSpeed, from.PathingTable[to].SpeedLimit);
            peakSpeed = Math.Min(speedLimit, Math.Sqrt((distance * MaxAcceleration * MaxDeceleration * 2 + startSpeed * startSpeed * MaxDeceleration +
                endSpeed * endSpeed * MaxAcceleration) / (MaxAcceleration + MaxDeceleration)));
            var t1 = (peakSpeed - startSpeed) / MaxAcceleration;
            var s1 = startSpeed * t1 + MaxAcceleration * t1 * t1 / 2;
            var t2 = (peakSpeed - endSpeed) / MaxDeceleration;
            var s2 = endSpeed * t2 + MaxDeceleration * t2 * t2 / 2;
            var t_star = (distance - s1 - s2) / peakSpeed;
            return t1 + t2 + t_star;
        }
        /// <summary>
        /// Get shortest time traveling between two adjacent control points, given the start and end speed,
        /// in which the end speed can be reduced is it is infeasible,
        /// </summary>
        /// <param name="peakSpeed">the peak speed achieved in the journey</param>
        public double GetShortestTravelingTime(ControlPoint from, ControlPoint to, double startSpeed, ref double endSpeed, out double peakSpeed)
        {
            try { return GetShortestTravelingTime(from, to, startSpeed, endSpeed, out peakSpeed); }
            catch (Exceptions.InfeasibleTravelling)
            {
                if (startSpeed < endSpeed)
                {
                    endSpeed = Math.Sqrt(startSpeed * startSpeed + from.GetDistanceTo(to) * MaxAcceleration * 2);
                    peakSpeed = endSpeed;
                    return (endSpeed - startSpeed) / MaxAcceleration;
                }
                else
                    throw new Exceptions.InfeasibleTravelling(
                        "Travelling profile is infeasible between the two control points.\n" +
                        "Reduce start speed, or increase deceleration of the vehicle.");
            }
        }        

        /// <summary>
        /// Check whether the vehicle is able to travel on the distance given from & to speed
        /// </summary>
        /// <param name="distance">the total traveling distance</param>
        /// <param name="fromSpeed">the speed vehicle start with</param>
        /// <param name="toSpeed">the speed vehicle end with</param>
        private void TravelingFeasibilityCheck(double distance, double fromSpeed, double toSpeed)
        {
            double v1, v2, a;
            if (fromSpeed < toSpeed) { v1 = fromSpeed; v2 = toSpeed; a = MaxAcceleration; }
            else { v1 = toSpeed; v2 = fromSpeed; a = MaxDeceleration; }
            if ((v2 * v2 - v1 * v1) / a / 2 > distance)
                throw new Exceptions.InfeasibleTravelling(
                    "Travelling profile is infeasible between the two control points.\n" +
                    "Eith reduce speed diffrence, or enlarge acceleration / deceleration of the vehicle.");
        }
    }
}
