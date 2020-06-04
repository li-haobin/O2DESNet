using System;

namespace O2DESNet.RandomVariables.Continuous
{
    public class Triangular : RandomVariable
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
            }
        }
        public override double Sample(Random rs)
        {
            return MathNet.Numerics.Distributions.Triangular.Sample(rs, LowerBound, UpperBound, Mode);
        }
        public Triangular()
        {

        }
    }
}
