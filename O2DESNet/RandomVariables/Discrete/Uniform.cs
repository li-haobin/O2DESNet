using System;

namespace O2DESNet.RandomVariables.Discrete
{
    public class Uniform : IRandomVariable
    {
        private int _lowerBound = 0;
        public int LowerBound
        {
            get { return _lowerBound; }
            set
            {
                _lowerBound = value;
                if (value > UpperBound)
                {
                    UpperBound = value;
                    _mean = value;
                    _std = 0;
                }
                else
                {
                    _mean = (_lowerBound + _upperBound) / 2;
                    double tempSquareSum = 0;
                    var n = _upperBound - _lowerBound + 1;
                    if (IncludeBound)
                    {
                        
                        for (int i = _lowerBound; i <= _upperBound; i++) tempSquareSum += (i - _mean) * (i - _mean);
                        _std = Math.Sqrt(tempSquareSum / Convert.ToDouble(n));
                    }
                    else
                    {
                        if (_upperBound - _lowerBound <= 1) throw new Exception("nothing between lower bound and upper bound if IncludeBound == fasle");
                        for (int i = _lowerBound+1; i <= _upperBound-1; i++) tempSquareSum += (i - _mean) * (i - _mean);
                        _std = Math.Sqrt(tempSquareSum / Convert.ToDouble(n));
                    }
                }
            }
        }
        private int _upperBound = 1;
        public int UpperBound
        {
            get { return _upperBound; }
            set
            {
                _upperBound = value;
                if (value < LowerBound)
                { 
                    LowerBound = value;
                    _mean = value;
                    _std = 0;
                }
                else
                {
                    _mean = (_lowerBound + _upperBound) / 2;
                    double tempSquareSum = 0;
                    var n = _upperBound - _lowerBound + 1;
                    if (IncludeBound)
                    {

                        for (int i = _lowerBound; i <= _upperBound; i++) tempSquareSum += (i - _mean) * (i - _mean);
                        _std = Math.Sqrt(tempSquareSum / Convert.ToDouble(n));
                    }
                    else
                    {
                        if (_upperBound - _lowerBound <= 1) throw new Exception("nothing between lower bound and upper bound if IncludeBound == fasle");
                        for (int i = _lowerBound + 1; i <= _upperBound - 1; i++) tempSquareSum += (i - _mean) * (i - _mean);
                        _std = Math.Sqrt(tempSquareSum / Convert.ToDouble(n));
                    }
                }
            }
        }
        public bool IncludeBound { get; set; } = true;
        private double _mean = 0.5;
        public double Mean 
        { 
            get { return _mean; }
            set => throw new Exception("Users not allowed to define discrete uniform random variable by setting mean value"); 
        }
        private double _std = 0.5;
        public double StadndardDeviation 
        {
            get { return _std; }
            set => throw new Exception("Users not allowed to define discrete uniform random variable by setting standard deviation value");
        }
        public int Sample(Random rs)
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
