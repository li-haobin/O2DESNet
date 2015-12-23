using O2DESNet.Demos.Workshop.Dynamics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace O2DESNet.Demos.Workshop
{
    public class Status:Status<Scenario>
    {
        public List<Machine> Machines { get; private set; }
        public List<Job> JobsInSystem { get; private set; }
        public List<Job> JobsDeparted { get; private set; }
        public List<Queue<Job>> Queues { get; private set; }
        public int JobCounter { get; private set; }
        public List<double> TimeSeries_JobHoursInSystem { get; private set; }

        internal Status(Scenario scenario, int seed = 0) : base(scenario, seed)
        {
            Machines = Scenario.MachineTypes.SelectMany(type => Enumerable.Range(0, type.Count)
                .Select(i => new Machine { Type = type, Processing = null })).ToList();
            Queues = Scenario.MachineTypes.Select(t => new Queue<Job>()).ToList();
            for (int i = 0; i < Machines.Count; i++) Machines[i].Id = i;
            JobsInSystem = new List<Job>();
            JobsDeparted = new List<Job>();
            TimeSeries_JobHoursInSystem = new List<double>();
            JobCounter = 0;
        }

        internal Machine Get_IdleMachine(int typeIndex)
        {
            return Machines.Where(m => m.Type.Id == typeIndex && m.IsIdle).FirstOrDefault();
        }

        internal Job Generate_EnteringJob(Random rs, DateTime timestamp)
        {
            var job = new Job { Id = JobCounter++, Type = Scenario.Generate_JobType(rs), EnterTime = timestamp, CurrentStage = 0 };
            JobsInSystem.Add(job);
            return job;
        }

        internal void Update_StartProcess(Job starting, Machine machine)
        {
            machine.Processing = starting;
            starting.BeingProcessedBy = machine;            
        }

        internal void Enqueue(Job toWait)
        {
            Queues[toWait.CurrentMachineTypeIndex].Enqueue(toWait);
        }

        internal Job GetWaitingJob_onFinishProcess(Job finishing)
        {
            var machine = finishing.BeingProcessedBy;
            finishing.CurrentStage++;
            finishing.BeingProcessedBy = null;
            machine.Processing = null;
            if (Queues[machine.Type.Id].Count > 0) return Queues[machine.Type.Id].Dequeue();
            return null;
        }

        internal void LogDeparture(Job departing, DateTime timestamp)
        {
            departing.ExitTime = timestamp;
            JobsDeparted.Add(departing);
            JobsInSystem.Remove(departing);
            TimeSeries_JobHoursInSystem.Add((departing.ExitTime - departing.EnterTime).TotalHours);
        }
    }
    

}
