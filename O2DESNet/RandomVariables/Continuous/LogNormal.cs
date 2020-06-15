using System;

namespace O2DESNet.RandomVariables.Continuous
{
    public class LogNormal : IRandomVariable
    {
        /// <summary>
        /// Expectation of LogNormal random variable
        /// </summary>
        private double _mean = Math.Exp(0.5);
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
                var muTemp = Math.Log(_mean) - 0.5 * Math.Log(1 + _std * _std / _mean / _mean);
                var sigmaTemp = Math.Sqrt(Math.Log(1 + _std * _std / _mean / _mean));
                _mu = muTemp;
                _sigma = sigmaTemp;
                if (value == 0) _cv = double.MaxValue;
                else _cv = _std / _mean;
            }
        }
        /// <summary>
        /// standard deviation
        /// </summary>
        private double _std = Math.Sqrt(Math.E * Math.E - Math.E);
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
                var muTemp = Math.Log(_mean) - 0.5 * Math.Log(1 + _std * _std / _mean / _mean);
                var sigmaTemp = Math.Sqrt(Math.Log(1 + _std * _std / _mean / _mean));
                _mu = muTemp;
                _sigma = sigmaTemp;
                if (_mean != 0) _cv = _std / _mean;
            }
        }
        /// <summary>
        /// coefficient variation
        /// </summary>
        private double _cv = Math.Sqrt(Math.E - 1);
        public double CV
        {
            get
            {
                return _cv;
            }
            set
            {
                if (value < 0) throw new Exception("Negative coefficient of variation not applicable for log normal distribution");
                _cv = value;
                _std = _cv * _mean;
            }
        }
        private double _mu = 0;
        /// <summary>
        /// The log-scale(mu) of the distribution
        /// </summary>
        public double Mu
        {
            get
            {
                return _mu;
            }
            set
            {
                _mu = value;
                var meanTemp = Math.Exp(_mu + _sigma * _sigma / 2);
                var stdTemp = Math.Sqrt((Math.Exp(_sigma * _sigma) - 1) * Math.Exp(2 * _mu + _sigma * _sigma));
                _mean = meanTemp;
                _std = stdTemp;
                _cv = _std / _mean;
            }
        }
        /// <summary>
        /// The shape of the distribution
        /// </summary>
        private double _sigma = 1;
        public double Sigma
        {
            get
            {
                return _sigma;
            }
            set
            {
                if (value < 0) throw new Exception("Negative shape parameter not applicable");
                _sigma = value;
                var meanTemp = Math.Exp(_mu + _sigma * _sigma / 2);
                var stdTemp = Math.Sqrt((Math.Exp(_sigma * _sigma) - 1) * Math.Exp(2 * _mu + _sigma * _sigma));
                _mean = meanTemp;
                _std = stdTemp;
                _cv = _std / _mean;
            }
        }
        /// <summary>
        /// coefficient variation
        /// </summary>

        public double Sample(Random rs)
        {
            return MathNet.Numerics.Distributions.LogNormal.Sample(rs, Mu, Sigma);
        }
        public LogNormal()
        {

        }
    }
}
