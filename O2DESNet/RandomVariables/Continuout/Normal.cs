using System;

namespace O2DESNet.RandomVariables.Continuous
{
    public class Normal : RandomVariable
    {
        private double _mean = 1;
        public double Mean
        {
            get
            {
                return _mean;
            }
            set
            {
                _mean = value;
                if (value == 0) _cv = double.MaxValue;
                else _cv = _std / Math.Abs(_mean);
            }
        }
        /// <summary>
        /// standard deviation
        /// </summary>
        private double _std = 1;
        public double STD
        {
            get
            {
                return _std;
            }
            set
            {
                if (value < 0) throw new Exception("Negative standard deviation not applicable");
                _std = value;
                if (_mean != 0) _cv = _std / Math.Abs(_mean);
            }
        }
        /// <summary>
        /// coefficient variation
        /// </summary>
        private double _cv = 1;
        public double CV
        {
            get
            {
                return _cv;
            }
            set
            {
                if (value < 0) throw new Exception("Negative coefficient variation not applicable");
                _cv = value;
                _std = _cv * Math.Abs(_mean);
            }
        }
        public override double Sample(Random rs)
        {
            if (CV == 0) return Mean;
            return MathNet.Numerics.Distributions.Normal.Sample(rs, Mean, STD);
        }
        public Normal()
        {

        }
    }
}
