using System;

namespace O2DESNet.RandomVariables.Discrete
{
    public class Poisson : IRandomVariable
    {
        /// <summary>
        /// arrival rate
        /// </summary>
        private double _lambda = 1;
        public double Lambda
        {
            get { return _lambda; }
            set
            {
                if (value < 0) throw new Exception("negative arrival rate not applicable");
                _lambda = value;
                _mean = value;
                _std = Math.Sqrt(value);

            }
        }
        private double _mean = 1;
        public double Mean 
        { 
            get { return _mean; }
            set
            {
                if (value < 0) throw new Exception("negative mean value not applicable");
                _mean = value;
                _lambda = value;
                _std = Math.Sqrt(value);
            }
        }
        private double _std = 1;
        public double StandardDeviation 
        {
            get { return _std; }
            set
            {
                if (value < 0) throw new Exception("negative standard deviation not applicable");
                _std = value;
                _mean = value * value;
                _lambda = value * value;
            }
        }
        public int Sample(Random rs)
        {
            return MathNet.Numerics.Distributions.Poisson.Sample(rs, Lambda);
        }
    }
}
