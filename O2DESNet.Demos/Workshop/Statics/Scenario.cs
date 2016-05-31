using MathNet.Numerics.Distributions;
using O2DESNet.Demos.Workshop.Statics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace O2DESNet.Demos.Workshop
{
    public class Scenario : O2DESNet.Scenario
    {
        public List<WorkStation> WorkStations { get; private set; }
        public List<ProductType> ProductTypes { get; private set; }
        public int MachineCapacity { get; set; }

        public Scenario() { ProductTypes = new List<ProductType>(); }

        public void SetWorkStations(params int[] nMachines)
        {
            WorkStations = Enumerable.Range(0, nMachines.Count()).Select(index => new WorkStation { Id = index + 1, N_Machines = nMachines[index] }).ToList();
        }
        
        /// <param name="jobSequence">indices of work stations</param>
        public void AddProductType(int[] jobSequence, double priority, 
            Func<Random, TimeSpan> interArrivalTime, Func<Random, WorkStation, TimeSpan> processingTime)
        {
            ProductTypes.Add(new ProductType
            {
                Id = ProductTypes.Count + 1,
                JobSequence = jobSequence.Select(i => WorkStations[i]).ToList(),
                Priority = priority,
                InterArrivalTime = interArrivalTime,
                ProcessingTime = processingTime
            });
        }

        // as in MO2TOS paper
        public static Scenario GetExample_Xu2015(params int[] nMachines)
        {
            var scenario = new Scenario { MachineCapacity = 2 };
            scenario.SetWorkStations(nMachines);
            Func<Random, TimeSpan> iidNormal = rs => TimeSpan.FromHours(Math.Max(0, Normal.Sample(rs, 1.0, 0.25)));
            scenario.AddProductType(new int[] { 0, 1, 2, 4, 3, 2, 0 }, 1, rs => iidNormal(rs), (rs, ws) => iidNormal(rs));
            scenario.AddProductType(new int[] { 1, 0, 3, 4, 2, 4, 3 }, 2, rs => iidNormal(rs), (rs, ws) => iidNormal(rs));
            return scenario;
        }

        public static Scenario GetExample_PedrielliZhu2015(params int[] nMachines)
        {
            var scenario = new Scenario { MachineCapacity = 1 };
            scenario.SetWorkStations(nMachines);

            var JobArrivalRate_Hourly = 4.0;
            double frequency;
            int[] workStationIndices;
            double[] meanProcessingTimes_Hour;

            Func<Random, double, TimeSpan> interArrivalTime =
                (rs, freq) => TimeSpan.FromHours(Exponential.Sample(rs, JobArrivalRate_Hourly * freq));
            Func<Random, WorkStation, double[], TimeSpan> processingTime =
                (rs, ws, mpHours) => TimeSpan.FromHours(Erlang.Sample(rs, 2, 1 / mpHours[scenario.WorkStations.IndexOf(ws)]));

            // product type #1
            frequency = 0.3;
            meanProcessingTimes_Hour = new double[] { 0.6, 0.85, 0.5, 0, 0.5 };
            workStationIndices = new int[] { 2, 0, 1, 4 };
            scenario.AddProductType(workStationIndices, 0,
                rs => interArrivalTime(rs, frequency),
                (rs, ws) => processingTime(rs, ws, meanProcessingTimes_Hour));

            // product type #2
            frequency = 0.5;
            meanProcessingTimes_Hour = new double[] { 0.8, 0, 0.75, 1.1, 0 };
            workStationIndices = new int[] { 3, 0, 2 };
            scenario.AddProductType(workStationIndices, 0,
                rs => interArrivalTime(rs, frequency),
                (rs, ws) => processingTime(rs, ws, meanProcessingTimes_Hour));

            // product type #3
            frequency = 0.2;
            meanProcessingTimes_Hour = new double[] { 0.7, 1.2, 1, 0.9, 0.25 };
            workStationIndices = new int[] { 1, 4, 0, 3, 2 };
            scenario.AddProductType(workStationIndices, 0,
                rs => interArrivalTime(rs, frequency),
                (rs, ws) => processingTime(rs, ws, meanProcessingTimes_Hour));

            return scenario;
        }
    }
}
