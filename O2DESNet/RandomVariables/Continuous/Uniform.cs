using System;

namespace O2DESNet.RandomVariables.Continuous
{
    public class Uniform : IRandomVariable
    {
        /// <summary>
        /// lower bound
        /// </summary>
        private double _lowerBound = 0;
        public double LowerBound
        {
            get
            {
                return _lowerBound;
            }
            set
            {
                if (value > UpperBound) UpperBound = value;
                _lowerBound = value;
                _mean = (_lowerBound + _upperBound) / 2;
                _std = (_upperBound - _lowerBound) * (_upperBound - _lowerBound) / 12;
            }
        }
        /// <summary>
        /// upper bound
        /// </summary>
        private double _upperBound = 1;
        public double UpperBound
        {
            get
            {
                return _upperBound;
            }
            set
            {
                if (value < LowerBound) LowerBound = value;
                _upperBound = value;
                _mean = (_lowerBound + _upperBound) / 2;
                _std = (_upperBound - _lowerBound) * (_upperBound - _lowerBound) / 12;
            }
        }
        private double _mean = 0.5;
        public double Mean 
        { 
            get { return _mean; }
            set => throw new Exception("Users not allowed to define continuous uniform random variable by setting mean value");
        }
        private double _std = 0.5;
        public double StandardDeviation
        { 
            get { return _std; }
            set => throw new Exception("Users not allowed to define continuous uniform random variable by setting standard deviation value");
        }

        public double Sample(Random rs)
        {
            return LowerBound + (UpperBound - LowerBound) * rs.NextDouble();
        }
    }
}
