using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Demos.Workshop
{
    internal class Status
    {
        internal Scenario Scenario { get; private set; }
        internal Simulator Simulator { get; private set; }
        internal Random RS { get; private set; }
        internal List<Machine> Machines { get; private set; }
        internal List<Job> JobsInSystem { get; private set; }
        internal List<Job> JobsDeparted { get; private set; }
        internal List<Queue<Job>> Queues { get; private set; }
        private int _jobCounter = 0;

        internal List<double> TimeSeries_JobHoursInSystem { get; private set; }

        internal Status(Simulator simulation, Scenario scenario, int seed)
        {
            Simulator = simulation;
            Scenario = scenario;
            RS = new Random(seed);
            Machines = Scenario.MachineTypes.SelectMany(type => Enumerable.Range(0, type.Count)
                .Select(i => new Machine { Type = type, Processing = null })).ToList();
            Queues = Scenario.MachineTypes.Select(t => new Queue<Job>()).ToList();
            for (int i = 0; i < Machines.Count; i++) Machines[i].Id = i;
            JobsInSystem = new List<Job>();
            JobsDeparted = new List<Job>();
            TimeSeries_JobHoursInSystem = new List<double>();
        }

        internal Machine Get_IdleMachine(int typeIndex)
        {
            return Machines.Where(m => m.Type.Id == typeIndex && m.IsIdle).FirstOrDefault();
        }

        internal Job Generate_EnteringJob()
        {
            var job = new Job { Id = _jobCounter++, Type = Scenario.Generate_JobType(RS), EnterTime = Simulator.ClockTime, CurrentStage = 0 };
            JobsInSystem.Add(job);
            return job;
        }

        internal void StartProcessing(Job job, Machine machine)
        {
            machine.Processing = job;
            job.BeingProcessed = machine;
        }

        internal void Enqueue(Job job)
        {
            Queues[job.CurrentMachineTypeIndex].Enqueue(job);
        }

        internal Job FinishProcessing(Job job)
        {
            var machine = job.BeingProcessed;
            job.CurrentStage++;
            job.BeingProcessed = null;
            machine.Processing = null;
            if (Queues[machine.Type.Id].Count > 0)
                return Queues[machine.Type.Id].Dequeue();
            return null;
        }

        internal void Depart(Job job)
        {
            job.ExitTime = Simulator.ClockTime;
            JobsDeparted.Add(job);
            JobsInSystem.Remove(job);
            TimeSeries_JobHoursInSystem.Add((job.ExitTime - job.EnterTime).TotalHours);
        }
    }
    

}
