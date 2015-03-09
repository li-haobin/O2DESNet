using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpSimulator.Model
{
    public class Job
    {
        public int Id { get; set; }
        public int Type { get; set; }
        public DateTime EnterTime { get; set; }
        public DateTime ExitTime { get; set; }
        public int CurrentStage { get; set; }
        public Machine BeingProcessed { get; set; }

        public int CurrentMachineType
        {
            get
            {
                List<int> machineSequence = new List<int>();
                switch (Type)
                {
                    case 1: machineSequence = new List<int> { 3, 1, 2, 5 }; break;
                    case 2: machineSequence = new List<int> { 4, 1, 3 }; break;
                    case 3: machineSequence = new List<int> { 2, 5, 1, 4, 3 }; break;
                }
                if (CurrentStage < machineSequence.Count) return machineSequence[CurrentStage];
                return 0;
            }
        }

        static public int GetType(Random rs)
        {
            var probabilities = new List<double> { 0.3, 0.5, 0.2 };
            var p = rs.NextDouble();
            double sum = 0;
            for (int i = 0; i < probabilities.Count; i++)
            {
                sum += probabilities[i];
                if (p < sum) return i + 1;
            }
            return 0;
        }
        
        static public TimeSpan GetInterArrivalTime(Random rs)
        {
            double lb = 5, ub = 15; //minutes
            return TimeSpan.FromMinutes(lb + (ub - lb) * rs.NextDouble());
        }

        public TimeSpan GetProcessingTime(Random rs)
        {
            double lb = 5; //minutes
            List<List<double>> ubs = new List<List<double>> { 
                new List<double> { 10, 15, 20, 25, 30},
                new List<double> { 12, 13, 14, 15, 16},
                new List<double> { 15, 18, 21, 24, 37},
            };
            return TimeSpan.FromMinutes(lb + (ubs[Type - 1][CurrentMachineType - 1] - lb) * rs.NextDouble());
        }
    }
}

