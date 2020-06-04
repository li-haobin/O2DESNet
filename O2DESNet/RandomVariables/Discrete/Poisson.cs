using System;

namespace O2DESNet.RandomVariables.Discrete
{
    public class Poisson : RandomVariable
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
            }
        }
        public override int Sample(Random rs)
        {
            return MathNet.Numerics.Distributions.Poisson.Sample(rs, Lambda);
        }
        public Poisson()
        {

        }
    }
}
