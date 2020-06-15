using System;

namespace O2DESNet.RandomVariables.Continuous
{
    public class Triangular : IRandomVariable
    {
        /// <summary>
        /// lower bound of the triangle distribution
        /// </summary>
        private double _lowerBound = 0;
        public double LowerBound
        {
            get { return _lowerBound; }
            set
            {
                if (value > UpperBound) UpperBound = value;
                if (value > Mode) Mode = value;
                _lowerBound = value;
                _mean = (_lowerBound + _upperBound + _mode) / 3;
                _std = Math.Sqrt(((_lowerBound) * (_lowerBound) +
                                  (_upperBound) * (_upperBound) +
                                  (_mode) * (_mode) -
                                  (_lowerBound) * (_upperBound) -
                                  (_lowerBound) * (_mode) -
                                  (_upperBound) * (_mode)) / 18);
            }
        }
        /// <summary>
        /// upper bound of the triangle distribution
        /// </summary>
        private double _upperBound = 1;
        public double UpperBound
        {
            get { return _upperBound; }
            set
            {
                if (value < LowerBound) LowerBound = value;
                if (value < Mode) Mode = value;
                _upperBound = value;
                _mean = (_lowerBound + _upperBound + _mode) / 3;
                _std = Math.Sqrt(((_lowerBound) * (_lowerBound) +
                                  (_upperBound) * (_upperBound) +
                                  (_mode) * (_mode) -
                                  (_lowerBound) * (_upperBound) -
                                  (_lowerBound) * (_mode) -
                                  (_upperBound) * (_mode)) / 18);
            }
        }
        /// <summary>
        /// mode of the triangle distribution
        /// </summary>
        private double _mode = 0.5;
        public double Mode
        {
            get { return _mode; }
            set
            {
                if (value < LowerBound) LowerBound = value;
                if (value > UpperBound) UpperBound = value;
                _mode = value;
                _mean = (_lowerBound + _upperBound + _mode) / 3;
                _std = Math.Sqrt(((_lowerBound) * (_lowerBound) +
                                  (_upperBound) * (_upperBound) +
                                  (_mode) * (_mode) -
                                  (_lowerBound) * (_upperBound) -
                                  (_lowerBound) * (_mode) -
                                  (_upperBound) * (_mode)) / 18);
            }
        }
        private double _mean = 0.5;
        public double Mean 
        { 
            get { return _mean; }
            set => throw new Exception("Users not allowed to define triangular random variable by setting mean value");
        }
        private double _std = Math.Sqrt(0.75 / 18);
        public double StandardDeviation 
        { 
            get { return _std; }
            set => throw new Exception("Users not allowed to define triangular random variable by setting standard deviation value");
        }

        public double Sample(Random rs)
        {
            return MathNet.Numerics.Distributions.Triangular.Sample(rs, LowerBound, UpperBound, Mode);
        }
    }
}
