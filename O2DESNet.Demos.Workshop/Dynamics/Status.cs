using System;
using System.Collections.Generic;
using System.Linq;

namespace O2DESNet.Demos.Workshop
{
    internal class Status
    {
        private Simulator _sim;
        internal List<Machine> Machines { get; private set; }
        internal List<Job> JobsInSystem { get; private set; }
        internal List<Job> JobsDeparted { get; private set; }
        internal List<Queue<Job>> Queues { get; private set; }
        internal int JobCounter { get; private set; }
        internal List<double> TimeSeries_JobHoursInSystem { get; private set; }

        internal Status(Simulator simulation)
        {
            _sim = simulation;
            Machines = _sim.Scenario.MachineTypes.SelectMany(type => Enumerable.Range(0, type.Count)
                .Select(i => new Machine { Type = type, Processing = null })).ToList();
            Queues = _sim.Scenario.MachineTypes.Select(t => new Queue<Job>()).ToList();
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

        internal Job Generate_EnteringJob(Random rs)
        {
            var job = new Job { Id = JobCounter++, Type = _sim.Scenario.Generate_JobType(rs), EnterTime = _sim.ClockTime, CurrentStage = 0 };
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
            if (Queues[machine.Type.Id].Count > 0)
                return Queues[machine.Type.Id].Dequeue();
            return null;
        }

        internal void Depart(Job departing)
        {
            departing.ExitTime = _sim.ClockTime;
            JobsDeparted.Add(departing);
            JobsInSystem.Remove(departing);
            TimeSeries_JobHoursInSystem.Add((departing.ExitTime - departing.EnterTime).TotalHours);
        }
    }
    

}
