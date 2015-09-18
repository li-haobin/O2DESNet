using O2DESNet.PathMover;
using System;
using System.Linq;

namespace PathMover_Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var pm = new O2DESNet.PathMover.System();
            var paths = Enumerable.Range(0,6).Select(i=> pm.CreatePath(100, 1, Direction.Forward)).ToArray();
            pm.Connect(paths[0], paths[1]);
            pm.Connect(paths[1], paths[2]);
            pm.Connect(paths[2], paths[3]);
            pm.Connect(paths[3], paths[0]);
            pm.Connect(paths[0], paths[4], 50, 0);
            pm.Connect(paths[2], paths[4], 50, 100);
            pm.Connect(paths[1], paths[5], 50, 0);
            pm.Connect(paths[3], paths[5], 50, 100);
            pm.Connect(paths[4], paths[5], 50, 50);

            pm.CreateControlPoint(paths[0], 30);

            pm.Initialize();
            //DisplayRouteTable(pm);


            Console.WriteLine(GetShortestTravellingTime(800, 30, 50, 80, 5, 10));

        }

        static void DisplayRouteTable(O2DESNet.PathMover.System pm)
        {
            foreach (var cp in pm.ControlPoints)
            {
                Console.WriteLine("Route Table at CP_{0}:", cp.Id);
                foreach (var item in cp.RoutingTable)
                    Console.WriteLine("{0}:{1}", item.Key.Id, item.Value.Id);
                Console.WriteLine();
            }
        }
        
        // vehicleType, cp_from, cp_to, startSpeed, endSpeed
        static double GetShortestTravellingTime(double distance, double startSpeed, double endSpeed, double speedLimit, double acceleration, double deceleration)
        {
            FeasibilityCheck(distance, startSpeed, endSpeed, acceleration, deceleration);
            var maxSpeed = Math.Min(speedLimit, Math.Sqrt((distance * acceleration * deceleration * 2 + startSpeed * startSpeed * deceleration +
                endSpeed * endSpeed * acceleration) / (acceleration + deceleration)));
            var t1 = (speedLimit - startSpeed) / acceleration;
            var s1 = startSpeed * t1 + acceleration * t1 * t1 / 2;
            var t2 = (speedLimit - endSpeed) / deceleration;
            var s2 = endSpeed * t2 + deceleration * t2 * t2 / 2;
            var t_star = (distance - s1 - s2) / speedLimit;
            return t1 + t2 + t_star;
        }

        static void FeasibilityCheck(double distance, double startSpeed, double endSpeed, double acceleration, double deceleration)
        {
            double v1, v2, a;
            if (startSpeed < endSpeed) { v1 = startSpeed; v2 = endSpeed; a = acceleration; }
            else { v1 = endSpeed; v2 = startSpeed; a = deceleration; }
            if ((v1 * (v2 - v1) * 2 + Math.Pow(v2 - v1, 2)) / a / 2 > distance)
                throw new InfeasibleTravellingException("Travelling profile is infeasible.\n" +
                    "Eith reduce speed diffrence, enlarge acceleration / deceleration of the vehicle, or increase the distance");
        }

        class InfeasibleTravellingException : Exception
        {
            public InfeasibleTravellingException() { }
            public InfeasibleTravellingException(string message) : base(message) { }
            public InfeasibleTravellingException(string message, Exception inner) : base(message, inner) { }
        }
    }
}
