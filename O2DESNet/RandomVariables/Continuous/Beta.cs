using System;

namespace O2DESNet.RandomVariables.Continuous
{
    public class Beta : IRandomVariable
    {
        private double _mean = 0.5;
        public double Mean
        {
            get
            {
                return _mean;
            }
            set
            {
                if (value <= 0) throw new Exception("None positive mean value not applicable for beta distribution");
                if (value > 1) throw new Exception("Mean value of beta distribution should not exceed 1");
                _mean = value;
                _cv = _std / _mean;
                var alphaTemp = _mean * _mean * (1 - _mean) / _std / _std - _mean;
                var betaTemp = (1 - _mean) * (1 - _mean) * _mean / _std / _std + _mean - 1;
                if (alphaTemp > 0) _alpha = alphaTemp;
                else throw new Exception("This setting of mean and standard deviation will derive illegal alpha value");
                if (betaTemp > 0) _beta = betaTemp;
                else throw new Exception("This setting of mean and standard deviation will derive illegal beta value");
            }
        }
        /// <summary>
        /// standard deviation
        /// </summary>
        private double _std = Math.Sqrt(1.0 / 12.0);
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
                var alphaTemp = _mean * _mean * (1 - _mean) / _std / _std - _mean;
                var betaTemp = (1 - _mean) * (1 - _mean) * _mean / _std / _std + _mean - 1;
                if (alphaTemp > 0) _alpha = alphaTemp;
                else throw new Exception("This setting of mean and standard deviation will derive illegal alpha value");
                if (betaTemp > 0) _beta = betaTemp;
                else throw new Exception("This setting of mean and standard deviation will derive illegal beta value");
            }
        }
        /// <summary>
        /// coefficient variation
        /// </summary>
        private double _cv = Math.Sqrt(3) / 3;
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
                _std = _cv * _mean;
            }
        }
        /// <summary>
        /// parameter alpha
        /// </summary>
        private double _alpha = 1;
        public double AlphaValue
        {
            get
            {
                return _alpha;
            }
            set
            {
                if (value <= 0) throw new Exception("negative or zero alpha value not applicable");
                _alpha = value;
                _mean = _alpha / (_alpha + _beta);
                _std = Math.Sqrt(_alpha * _beta / (_alpha + _beta) * (_alpha + _beta) / (_alpha + _beta + 1));
                _cv = _std / _mean;
            }
        }
        /// <summary>
        /// parameter beta
        /// </summary>
        private double _beta = 1;
        public double BetaValue
        {
            get
            {
                return _beta;
            }
            set
            {
                if (value <= 0) throw new Exception("negative or zero beta value not applicable");
                _beta = value;
                _mean = _alpha / (_alpha + _beta);
                _std = Math.Sqrt(_alpha * _beta / (_alpha + _beta) * (_alpha + _beta) / (_alpha + _beta + 1));
                _cv = _std / _mean;
            }
        }

        public double StandardDeviation { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public double Sample(Random rs)
        {
            if (CV == 0) return Mean;
            return MathNet.Numerics.Distributions.Beta.Sample(rs, AlphaValue, BetaValue);
        }
    }
}
