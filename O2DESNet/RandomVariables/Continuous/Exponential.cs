using System;

namespace O2DESNet.RandomVariables.Continuous
{
    public class Exponential : IRandomVariable
    {
        private double _lambda = 1;
        public double Lambda 
        {
            get { return _lambda; }
            set
            {
                if(value<=0) throw new Exception("negative or zero arrival rate not applicable");
                _lambda = value;
                _mean = 1 / _lambda;
                _std = _mean;
            }
        }
        private double _mean = 1;
        public double Mean
        {
            get { return _mean; }
            set
            {
                if (value <= 0) throw new Exception("negative or zero mean value not applicable");
                _mean = value;
                _std = _mean;
                _lambda = 1 / _mean;
            }
        }
        private double _std = 1;
        public double StandardDeviation
        {
            get { return _std; }
            set
            {
                if (value <= 0) throw new Exception("negative or zero standard deviation not applicable");
                _std = value;
                _mean = _std;
                _lambda = 1 / _mean;
            }
        }
        public double Sample(Random rs)
        {
            return MathNet.Numerics.Distributions.Exponential.Sample(rs, Lambda);
        }
    }
}
