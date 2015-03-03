using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscreteEventSimulation;
using CSharpSimulator.Model;

namespace CSharpSimulator
{
    public class Simulation : Base
    {
        private Random _rs;

        public Cluster Cluster;
        public List<Job> JobsInSystem;
        public List<List<Job>> Queues;
        public int JobCount {get;private set;}

        public Simulation(Cluster cluster, int seed)
        {
            Cluster = cluster;
            _rs = new Random(seed);
            JobCount = 0;

            JobsInSystem = new List<Job>();
            Queues = new List<List<Job>> { }; for (int i = 0; i < 5; i++) Queues.Add(new List<Job>());

            ScheduleEvent(Arrival(), ClockTime + Job.GetInterArrivalTime(_rs));
        }

        private Event Arrival()
        {
            return delegate()
            {
                var job = new Job { Id = ++JobCount, Type = Job.GetType(_rs), EnterTime = ClockTime, CurrentStage = 0 };
                JobsInSystem.Add(job);
                Console.WriteLine("{0}: Job #{1} (Type {2}) Arrives.", ClockTime, job.Id, job.Type);
                Process(job)();
                ScheduleEvent(Arrival(), ClockTime + Job.GetInterArrivalTime(_rs));                
            };
        }

        private Event Process(Job job)
        {
            return delegate()
            {
                var machine = Cluster.GetIdleMachine(job.CurrentMachineType);
                if (machine != null)
                {
                    machine.Processing = job;
                    job.BeingProcessed = machine;
                    ScheduleEvent(Finish(job), ClockTime + job.GetProcessingTime(_rs));
                    Console.WriteLine("{0}: Job #{1} (Type {2}) starts process at Machine (Type {3}).", ClockTime, job.Id, job.Type, machine.Type);
                }
                else
                {
                    Queues[job.CurrentMachineType - 1].Add(job);
                }
            };
        }

        private Event Finish(Job job)
        {
            return delegate()
            {
                var machine = job.BeingProcessed;
                job.CurrentStage++;
                job.BeingProcessed = null;
                machine.Processing = null;
                Console.WriteLine("{0}: Job #{1} (Type {2}) finishes process at Machine (Type {3}).", ClockTime, job.Id, job.Type, machine.Type);

                if (Queues[machine.Type - 1].Count > 0)
                {
                    var waiting = Queues[machine.Type - 1].First();
                    Queues[machine.Type - 1].Remove(waiting);
                    Process(waiting)();
                }
                if (job.CurrentMachineType > 0) Process(job)();
                else Depart(job)();
            };
        }

        private Event Depart(Job job)
        {
            return delegate()
            {
                job.ExitTime = ClockTime;
                JobsInSystem.Remove(job);
                Console.WriteLine("{0}: Job #{1} (Type {2}) Departs.", ClockTime, job.Id, job.Type);
            };
        }
    }
}
