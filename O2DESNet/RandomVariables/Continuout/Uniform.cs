using System;

namespace O2DESNet.RandomVariables.Continuous
{
    public class Uniform : RandomVariable
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
            }
        }
        public override double Sample(Random rs)
        {
            return LowerBound + (UpperBound - LowerBound) * rs.NextDouble();
        }
        public Uniform()
        {

        }
    }
}
