using System;
using System.Collections.Generic;
using System.Linq;

namespace O2DESNet.RandomVariables.Categorical
{
    public class Uniform<T> : IRandomVariable<T>
    {
        public IEnumerable<T> Candidates { get; set; }
        public double Mean 
        { 
            get => throw new Exception("Catigorical random variable mean not available"); 
            set => throw new Exception("Catigorical random variable mean not available");
        }
        public double StandardDeviation
        {
            get => throw new Exception("Catigorical random variable standard deviation not available");
            set => throw new Exception("Catigorical random variable standard deviation not available");
        }
        public T Sample(Random rs)
        {
            if (Candidates.Count() == 0) return default;
            return Candidates.ElementAt(rs.Next(Candidates.Count()));
        }
    }
}
