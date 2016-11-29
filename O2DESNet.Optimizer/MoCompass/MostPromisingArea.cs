using MathNet.Numerics.LinearAlgebra.Double;
using O2DESNet.Optimizer.Samplings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Optimizer
{
    internal class MostPromisingArea
    {
        internal MoCompass MoCompass { get; private set; }        
        internal DenseVector Superior { get; private set; }
        private Dictionary<DenseVector, Constraint> _cutsToInferiors;
        private ConvexSet _convexSet;

        internal MostPromisingArea(MoCompass moCompass, DenseVector superior, IEnumerable<DenseVector> inferiors = null)
        {
            MoCompass = moCompass;
            Superior = superior;
            var space = MoCompass.DecisionSpace;
            _convexSet = new ConvexSet(dimension: space.Dimension, constraints: space.Constraints);
            _cutsToInferiors = new Dictionary<DenseVector, Constraint>();
            if (inferiors != null) Add(inferiors);
        }

        internal void Add(IEnumerable<DenseVector> inferiors)
        {
            foreach (var inferior in inferiors) Add(inferior);
        }
        internal void Add(DenseVector inferior)
        {
            if (!_cutsToInferiors.ContainsKey(inferior))
            {
                var cut = GetCut(inferior);
                _cutsToInferiors.Add(inferior, cut);
                _convexSet.Constraints.Add(cut);
            }
        }

        internal void Remove(IEnumerable<DenseVector> inferiors)
        {
            foreach (var inferior in inferiors) Remove(inferior);
        }
        internal void Remove(DenseVector inferior)
        {
            if (_cutsToInferiors.ContainsKey(inferior))
            {
                _convexSet.Constraints.Remove(_cutsToInferiors[inferior]);
                _cutsToInferiors.Remove(inferior);
            }
        }

        internal DenseVector Sample(MoCompass.SamplingScheme samplingScheme, Random rs)
        {
            DenseVector direction;
            DenseVector uniGradient;
            switch (samplingScheme)
            {
                case MoCompass.SamplingScheme.CoordinateSampling:
                    direction = DenseMatrix.CreateIdentity(_convexSet.Dimension).ToRowArrays()[rs.Next(_convexSet.Dimension)];
                    if (rs.NextDouble() < 0.5) direction = -direction;
                    break;
                case MoCompass.SamplingScheme.PolarUniform:
                    direction = PolarRandom.Uniform(_convexSet.Dimension, rs);
                    break;
                case MoCompass.SamplingScheme.GoPolars:
                    uniGradient = MoCompass.UnifiedGradient[Superior];
                    if (uniGradient != null && uniGradient.Count(g => double.IsNaN(g)) < 1)
                        direction = PolarRandom.Oriented(-uniGradient, 0.5, rs);
                    else direction = PolarRandom.Uniform(_convexSet.Dimension, rs);
                    break;
                case MoCompass.SamplingScheme.GoCS:
                    uniGradient = MoCompass.UnifiedGradient[Superior];
                    var directions = DenseMatrix.CreateIdentity(_convexSet.Dimension).ToRowArrays().Concat(
                        (DenseMatrix.CreateIdentity(_convexSet.Dimension) * (-1)).ToRowArrays())
                        .OrderBy(dir => uniGradient.DotProduct((DenseVector)dir));
                    direction = TruncatedGeometric.Sample(directions, 0.2, rs);
                    break;
                default: throw new Exception("Non-specified sampling scheme.");
            }
            return Sample(direction, rs);
        }
        internal DenseVector Sample(DenseVector direction, Random rs)
        {
            var r = _convexSet.Distance(Superior, direction);
            return Superior + direction * r * rs.NextDouble();
        }

        private Constraint GetCut(DenseVector inferior)
        {
            var yMinusX = inferior - Superior;
            return new ConstraintLE(yMinusX, yMinusX.Sum(v => v * v) / 2 + yMinusX.DotProduct(Superior));
        }
    }
}
