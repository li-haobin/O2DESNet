using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Optimizer
{
    public abstract class Constraint
    {
        protected DenseVector _lhs { get; set; }
        protected double _rhs { get; set; }
        public abstract DenseVector Coefficients { get; }

        internal bool FeasibleFor(DenseVector point) { return point.DotProduct(_lhs) <= _rhs; }
        internal double Slack(DenseVector point) { return _rhs - _lhs.DotProduct(point); }
        internal double SlackDistance(DenseVector point, DenseVector direction)
        {
            var ad = _lhs.DotProduct(direction);
            var slack = Slack(point);
            if (ad > 0) return slack / ad;
            else if (slack >= 0) return double.PositiveInfinity;
            return double.NaN; // impossible to satisfy the constraint given point and direction
        }
    }

    public class ConstraintLE : Constraint
    {
        public override DenseVector Coefficients { get { return _lhs; } }
        internal double UpperBound
        {
            get { return _rhs; }
            set { _rhs = value; }
        }

        public ConstraintLE(DenseVector coefficients, double upperBound)
        {
            _lhs = coefficients;
            UpperBound = upperBound;
        }
    }

    public class ConstraintGE : Constraint
    {
        public override DenseVector Coefficients { get { return -_lhs; } }
        internal double LowerBound
        {
            get { return -_rhs; }
            set { _rhs = -value; }
        }
        public ConstraintGE(DenseVector coefficients, double lowerBound)
        {
            _lhs = -coefficients;
            LowerBound = lowerBound;
        }
    }
}
