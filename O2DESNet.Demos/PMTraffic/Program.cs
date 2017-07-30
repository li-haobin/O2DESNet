using O2DESNet.Traffic;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace O2DESNet.Demos.PMTraffic
{
    partial class Program
    {
        static void Main()
        {
            bool? crossHatch = null, restrictedJobs = null;
            Console.Write("Cross-Hatch at Junctions? ");
            var readLine = Console.ReadLine().ToUpper();
            if (readLine == "Y") crossHatch = true;
            else if (readLine == "N") crossHatch = false;
            else throw new Exception("Wrong Input!");

            Console.Write("Restricted to Neighboring Jobs? ");
            readLine = Console.ReadLine().ToUpper();
            if (readLine == "Y") restrictedJobs = true;
            else if (readLine == "N") restrictedJobs = false;
            else throw new Exception("Wrong Input!");

            Console.Write("Starting #Vehicles: ");
            var nVehiclesMin = Convert.ToInt32(Console.ReadLine());
            Console.Write("Ending #Vehicles: ");
            var nVehiclesMax = Convert.ToInt32(Console.ReadLine());
            Console.Write("Min. Random Seed: ");
            var seedMin = Convert.ToInt32(Console.ReadLine());
            Console.Write("Max. Random Seed: ");
            var seedMax = Convert.ToInt32(Console.ReadLine());

            var pm = ExamplePM(crossHatch.Value);
            var quayCPs = pm.ControlPoints.Values.Where(cp => cp.Tag.StartsWith("Q_")).ToList();
            var transCPs = pm.ControlPoints.Values.Where(cp => cp.Tag.StartsWith("T_")).ToList();
            var exchgCPs = pm.ControlPoints.Values.Where(cp => cp.Tag.StartsWith("E_")).ToList();

            Stopwatch stopwatch = new Stopwatch();
            for (int seed = seedMin; seed <= seedMax; seed++)
            {
                Console.WriteLine("Random Seed: {0}", seedMin);
                for (int nVehicles = nVehiclesMin; nVehicles <= nVehiclesMax; nVehicles += 5)
                {
                    var sim = new Simulator(new Testbed_PathMover(
                        new Testbed_PathMover.Statics
                        {
                            PathMover = pm,
                            Origins = quayCPs, // GetOrigins(pm),
                            Destinations = transCPs.Concat(exchgCPs).ToList(), // GetDestinations(pm),
                            VehicleCategory = new Vehicle.Statics(),
                            NVehicles = nVehicles,
                            RestrictedNeighboringJobs = restrictedJobs.Value,
                        },
                        seed: seed));

                    sim.State.Display = false;

                    stopwatch.Restart();
                    sim.Run(TimeSpan.FromDays(30));
                    stopwatch.Stop();

                    Output(sim, string.Format("{0}_{1}_{2}", crossHatch, restrictedJobs, seed), stopwatch);
                }
            }
        }

        static double VehicleLength = 16 + 1.75;
        static double Clearence = 4;
        static double MaxSpeed = 7;
        static double Deceleration = 1.55;
        static void ConfigPath(Traffic.Path.Statics path)
        {
            path.Capacity = Math.Max(1, (int)Math.Floor(path.Length / (VehicleLength + Clearence)));
            if (path.Length > VehicleLength + Clearence) path.SpeedByDensity = SpeedByDensity;
        }

        static double SpeedByDensity(double densityPerLane)
        {
            var safetyDistance = Math.Max(0, 1 / densityPerLane - VehicleLength);
            var speed = Math.Min(MaxSpeed, Math.Sqrt(2 * Deceleration * safetyDistance));
            if (speed == 0) throw new Exception();
            return speed;
        }

        static void Output(Simulator sim, string tag, Stopwatch stopwatch)
        {
            var state = (Testbed_PathMover)sim.Assembly;

            Console.WriteLine("{0}\t{1:F4}\t{2:F4}\t{3:F4}\t{4:F4}\t{5:F4}\t{6:F4}\t{7:F4}\t{8:F4}\t{9:F4}\t{10:F2}",
                state.Config.NVehicles,
                state.JobsCounter.DecrementRate,
                state.JobsCounter.DecrementRate / state.Config.NVehicles,
                state.PathMover.AverageSpeed,
                state.PathMover.AverageSpeed,
                state.PathMover.HCounter_Departing.AverageCount,
                state.PathMover.HCounter_Travelling.AverageCount,
                state.PathMover.Paths.Values.Sum(p => p.HC_VehiclesTravelling.AverageCount),
                state.PathMover.Paths.Values.Sum(p => p.HC_VehiclesCompleted.AverageCount),
                state.DeadlocksCounter.IncrementRate,
                stopwatch.Elapsed.TotalSeconds
                );

            using (StreamWriter sw = new StreamWriter("output_" + tag + ".csv", true))
            {
                sw.Write("{0},", DateTime.Now.ToString("yyyy-MMM-dd HH:mm:ss"));
                sw.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}",
                    state.Config.NVehicles,
                    state.JobsCounter.DecrementRate,
                    state.JobsCounter.DecrementRate / state.Config.NVehicles,
                    state.PathMover.AverageSpeed,
                    state.PathMover.AverageSpeed,
                    state.PathMover.HCounter_Departing.AverageCount,
                    state.PathMover.HCounter_Travelling.AverageCount,
                    state.PathMover.Paths.Values.Sum(p => p.HC_VehiclesTravelling.AverageCount),
                    state.PathMover.Paths.Values.Sum(p => p.HC_VehiclesCompleted.AverageCount),
                    state.DeadlocksCounter.IncrementRate,
                    stopwatch.Elapsed.TotalSeconds
                    );
            }
        }
    }
}
