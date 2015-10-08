using System;
using System.Collections.Generic;
using System.Linq;

namespace O2DESNet.Demos.Workshop
{
    public class Scenario
    {
        public List<MachineType> MachineTypes { get; private set; }
        public List<JobType> JobTypes { get; private set; }
        public double JobArrivalRate_Hourly { get; set; }

        public Scenario() { JobTypes = new List<JobType>(); }

        public void SetMachineTypes(params int[] counts)
        {
            MachineTypes = Enumerable.Range(0, counts.Count()).Select(index => new MachineType { Id = index, Count = counts[index] }).ToList();
        }

        public void AddJobType(double frequence, double[] meanProcessTimes, int[] machineSequence)
        {
            JobTypes.Add(new JobType
            {
                Id = JobTypes.Count,
                Frequence = frequence,
                MachineSequence = machineSequence.ToList(),
                MeanProcessingTimes = meanProcessTimes.ToList()
            });
        }

        #region Random Generators
        internal JobType Generate_JobType(Random rs)
        {
            var sumFrequence = JobTypes.Sum(t => t.Frequence);
            var p = rs.NextDouble() * sumFrequence;
            double sum = 0;
            for (int i = 0; i < JobTypes.Count; i++)
            {
                sum += JobTypes[i].Frequence;
                if (p < sum) return JobTypes[i];
            }
            return null;
        }

        internal TimeSpan Generate_InterArrivalTime(Random rs)
        {
            return TimeSpan.FromHours(MathNet.Numerics.Distributions.Exponential.Sample(rs, JobArrivalRate_Hourly));
        }

        internal TimeSpan Generate_ProcessingTime(int jobTypeIndex, int machineTypeIndex, Random rs)
        {
            return TimeSpan.FromHours(MathNet.Numerics.Distributions.Erlang
                .Sample(rs, 2, 1 / JobTypes[jobTypeIndex].MeanProcessingTimes[machineTypeIndex]));
        }
        #endregion

        public static Scenario GetExample(params int[] machineCounts)
        {
            var scenario = new Scenario();
            scenario.JobArrivalRate_Hourly = 4.0;
            scenario.SetMachineTypes(machineCounts);
            scenario.AddJobType(0.3, new double[] { 0.6, 0.85, 0.5, 0, 0.5 }, new int[] { 2, 0, 1, 4 });
            scenario.AddJobType(0.5, new double[] { 0.8, 0, 0.75, 1.1, 0 }, new int[] { 3, 0, 2 });
            scenario.AddJobType(0.2, new double[] { 0.7, 1.2, 1, 0.9, 0.25 }, new int[] { 1, 4, 0, 3, 2 });
            return scenario;
        }
    }
}
