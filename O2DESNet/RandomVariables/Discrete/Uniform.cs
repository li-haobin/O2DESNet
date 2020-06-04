using System;

namespace O2DESNet.RandomVariables.Discrete
{
    public class Uniform : RandomVariable
    {
        private int _lowerBound = 0;
        public int LowerBound
        {
            get { return _lowerBound; }
            set
            {
                if (value > UpperBound) UpperBound = value;
                _lowerBound = value;
            }
        }
        private int _upperBound = 1;
        public int UpperBound
        {
            get { return _upperBound; }
            set
            {
                if (value < LowerBound) LowerBound = value;
                _upperBound = value;
            }
        }
        public bool IncludeBound { get; set; } = true;
        public override int Sample(Random rs)
        {
            int temp;
            if (IncludeBound)
            {
                int dummyLowerBound = LowerBound - 1;
                int dummyUpperBound = UpperBound + 1;
                temp = dummyLowerBound;
                while (temp == dummyLowerBound || temp == dummyUpperBound)
                {
                    temp = Convert.ToInt32(Math.Round(dummyLowerBound + (dummyUpperBound - dummyLowerBound) * rs.NextDouble()));
                }
            }
            else
            {
                temp = LowerBound;
                while (temp == LowerBound || temp == UpperBound)
                {
                    temp = Convert.ToInt32(Math.Round(LowerBound + (UpperBound - LowerBound) * rs.NextDouble()));
                }
            }
            return temp;
        }
    }
}
