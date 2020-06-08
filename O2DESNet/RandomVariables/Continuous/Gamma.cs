using System;

namespace O2DESNet.RandomVariables.Continuous
{
    public class Gamma : RandomVariable
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
                if (value <= 0) throw new Exception("None positive mean value not applicable for beta distribution");
                _mean = value;
                _cv = _std / _mean;
                _alpha = _mean * _mean / _std / _std;
                _beta = _mean / _std / _std;
            }
        }
        /// <summary>
        /// standard deviation
        /// </summary>
        private double _std = 1;
        public double StandardDeviation
        {
            get
            {
                return _std;
            }
            set
            {
                if (value < 0) throw new Exception("Negative standard deviation not applicable");
                _std = value;
                _cv = _std / _mean;
                _alpha = _mean * _mean / _std / _std;
                _beta = _mean / _std / _std;
            }
        }
        /// <summary>
        /// coefficient variation cv = sigma/mu
        /// </summary>
        private double _cv = 1;
        private double CV
        {
            get
            {
                return _cv;
            }
            set
            {
                if (value < 0) throw new Exception("Negative coefficient variation not applicable");
                _cv = value;
                _std = _cv * _mean;
                _alpha = 1 / _cv / _cv;
                _beta = _mean / _std / _std;
            }
        }
        /// <summary>
        /// shape of gamma distribution, refer to wikipedia
        /// </summary>
        private double _alpha = 1;
        public double Alpha
        {
            get
            {
                return _alpha;
            }
            set
            {
                if (value <= 0) throw new Exception("negative or zero alpha value not applicable");
                _alpha = value;
                _mean = _alpha / _beta;
                _std = Math.Sqrt(_alpha / _beta / _beta);
                _cv = _std / _mean;
            }
        }
        /// <summary>
        /// rate of gamma distribution, refer to wikipedia
        /// </summary>
        private double _beta = 1;
        public double Beta
        {
            get
            {
                return _beta;
            }
            set
            {
                if (value <= 0) throw new Exception("negative or zero beta value not applicable");
                _beta = value;
                _mean = _alpha / _beta;
                _std = Math.Sqrt(_alpha / _beta / _beta);
                _cv = _std / _mean;
            }
        }
        public override double Sample(Random rs)
        {
            if (Mean == 0) return 0;
            if (CV == 0) return Mean;
            return MathNet.Numerics.Distributions.Gamma.Sample(rs, Alpha, Beta);
        }
    }
}
