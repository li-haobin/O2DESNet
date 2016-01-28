using O2DESNet.Distributions;
using O2DESNet.Replicators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Explorers
{
    public class MoCompass<TScenario, TStatus, TSimulator> : Explorer<TScenario, TStatus, TSimulator>
        where TScenario : Scenario
        where TStatus : Status<TScenario>
        where TSimulator : Simulator<TScenario, TStatus>
    {
        public MoCompass(
            DecisionSpace decisionSpace,
            Func<double[], TScenario> constrScenario,
            Func<TScenario, int, TStatus> constrStatus,
            Func<TStatus, TSimulator> constrSimulator,
            Func<TStatus, bool> terminate,
            Func<TStatus, double[]> objectives,
            bool discrete = false,
            int seed = 0,
            bool inParallel = true) : 
            base(decisionSpace, constrScenario, constrStatus, constrSimulator, terminate, objectives, discrete, seed)
        {
            Replicator = new MOCBA<TScenario, TStatus, TSimulator>(new TScenario[] { }, 
                constrStatus, constrSimulator, terminate, objectives, inParallel);
            int dim = DecisionSpace.Dimension;            
            var initialWidth = 10.0;
            _dynLbs = Enumerable.Repeat(-initialWidth, dim).ToArray();
            _dynUbs = Enumerable.Repeat(initialWidth, dim).ToArray();
            SamplingScheme = SamplingSchemes.Coordinate;
        }

        /// <summary>
        /// Dynamic lower bounds for unbounded decisions space
        /// </summary>
        private double[] _dynLbs;
        /// <summary>
        /// Dynamic upper bounds for unbounded decisions space
        /// </summary>
        private double[] _dynUbs;
        public TScenario[] ParetoSet { get { return ((MOCBA<TScenario, TStatus, TSimulator>)Replicator).ParetoSet; } }
        public SamplingSchemes SamplingScheme { get; set; }

        protected override List<double[]> Sample(int size)
        {
            var stopingMPASize = 0.000000001;
            var paretoSet = new HashSet<TScenario>(ParetoSet);
            if (paretoSet.Count == 0) return base.Sample(size);
            foreach (var s in paretoSet) RelaxDynBounds(s);
            var inferiors = Replicator.Scenarios.Where(s => !paretoSet.Contains(s)).Select(s => Decisions[s]).ToArray();
            var mpas = paretoSet.Select(s => new MPA(DecisionSpace, Decisions[s], inferiors, _dynUbs, _dynLbs))
                .Where(mpa => mpa.Size > stopingMPASize).ToArray();
            var samples = new List<double[]>();
            if (mpas.Length == 0) return samples;
            while (samples.Count < size)
            {
                var pivot = mpas[DefaultRS.Next(mpas.Length)];
                switch (SamplingScheme)
                {
                    case SamplingSchemes.Coordinate:
                        samples.Add(pivot.CoordinateSample(DefaultRS));
                        break;
                    case SamplingSchemes.Polar:
                        samples.Add(pivot.PolarSample(DefaultRS));
                        break;
                }
            }
            return samples;
        }   
        
        /// <summary>
        /// Extend the dynamic bounds to cover current Pareto set with buffer range
        /// </summary>
        private void RelaxDynBounds(TScenario scenario)
        {
            var decisions = Decisions[scenario];
            foreach (var i in DecisionSpace.Coords)
            {
                while (decisions[i] > _dynUbs[i] / 2) _dynUbs[i] *= 2;
                while (decisions[i] < _dynLbs[i] / 2) _dynLbs[i] *= 2;
            }
        }
        //private double[] GetFeasibleDecisions()
        //{
        //    if (Constraints.Count() == 0) return Enumerable.Repeat(0.0, Dimension).ToArray();
        //    var phaseI = new MoCompass(Dimension, new double[][] { },
        //        decisions =>
        //        {
        //            return new double[] {
        //                    Constraints.Sum(c => Math.Max(0, _indices.Sum(i => decisions[i] * c[i]) - c.Last()))
        //            };
        //        });
        //    phaseI.StoppingCondition_ObjectiveValues = (objs => objs[0] == 0);
        //    phaseI.Run(1, int.MaxValue, 0.0001, new Random(138));
        //    if (phaseI.ParetoSet[0][Dimension] == 0)
        //        return _indices.Select(i => phaseI.ParetoSet[0][i]).ToArray();
        //    throw new Exception("Cannot find an initial feasible solution.");
        //}
        
        public enum SamplingSchemes {
            /// <summary>
            /// Coordinate Sampling
            /// </summary>
            Coordinate,
            /// <summary>
            /// Polar Randome Sampling
            /// </summary>
            Polar
        };

        /// <summary>
        /// Most Promising Area
        /// </summary>
        internal class MPA
        {
            /// <summary>
            /// Decision space as from COMPASS
            /// </summary>
            private DecisionSpace _dSpace;
            /// <summary>
            /// The core decisions, i.e., the "good" point at the center
            /// </summary>
            private double[] _core;
            /// <summary>
            /// Lower bounds of MPA at all coordinates
            /// </summary>
            private double[] _ubs;
            /// <summary>
            /// Upper bounds of MPA at all coordinates
            /// </summary>
            private double[] _lbs;
            /// <summary>
            /// Dynamic upper bounds at all coordinates, as properties of COMPASS for unbounded decision space
            /// </summary>
            private double[] _dynUbs;
            /// <summary>
            /// Dynamic lower bounds at all coordinates, as properties of COMPASS for unbounded decision space
            /// </summary>
            private double[] _dynLbs;
            /// <summary>
            /// The preferred direction, e.g., gradient, for oriented polar sampling
            /// </summary>
            private double[] _orient;
            /// <summary>
            /// Indicator to measure the size of MPA
            /// </summary>
            internal double Size { get; private set; }

            internal MPA(DecisionSpace dSpace, double[] core, double[][] inferiors, double[] dynUbs, double[] dynLbs, double[] orient = null)
            {
                _dSpace = dSpace; _core = core;
                _lbs = _dSpace.Lowerbounds.ToArray();
                _ubs = _dSpace.Upperbounds.ToArray();
                _dynLbs = dynLbs; _dynUbs = dynUbs;
                _orient = orient;
                foreach (var constraint in _dSpace.Constraints) Construct(constraint);
                foreach (var inferior in inferiors) Refine(inferior);
                Resize();
            }   
            /// <summary>
            /// Initialize MPA by constraints
            /// </summary>
            private void Construct(List<double> constraint)
            {
                var slack = constraint.Last() - _dSpace.Coords.Sum(i => constraint[i] * _core[i]);
                if (slack < -1E-14) throw new Exception("The solution is infeasible.");
                foreach (var i in _dSpace.Coords)
                {
                    if (constraint[i] > 0) _ubs[i] = Math.Min(_ubs[i], _core[i] + slack / constraint[i]);
                    else if (constraint[i] < 0) _lbs[i] = Math.Max(_lbs[i], _core[i] + slack / constraint[i]);
                }
            }
            /// <summary>
            /// Refine MPA by inferior decisions, i.e., cutting
            /// </summary>
            private void Refine(double[] inferior)
            {
                var yMinusX = _dSpace.Coords.Select(i => inferior[i] - _core[i]).ToArray();
                double numerator = 0.5 * yMinusX.Sum(v => v * v);
                foreach (var i in _dSpace.Coords)
                {
                    double d = numerator / yMinusX[i];
                    if (d > 0) _ubs[i] = Math.Min(_ubs[i], d + _core[i]);
                    else if (d < 0) _lbs[i] = Math.Max(_lbs[i], d + _core[i]);
                }
                Resize();
            }
            /// <summary>
            /// Re-calculate the size of MPA
            /// </summary>
            private void Resize() { Size = Math.Sqrt(_dSpace.Coords.Sum(i => Math.Pow(_ubs[i] - _lbs[i], 2))); }

            #region Sampling Methods
            internal double[] CoordinateSample(Random rs)
            {
                var sample = _core.ToArray();
                var i = rs.Next(_dSpace.Dimension);
                // dynamic bounds are effective only when MPA is open at the corresponding coordinate
                var lb = _lbs[i] > double.NegativeInfinity ? _lbs[i] : _dynLbs[i];
                var ub = _ubs[i] < double.PositiveInfinity ? _ubs[i] : _dynUbs[i];
                sample[i] = lb + (ub - lb) * rs.NextDouble();
                return sample;
            }
            internal double[] PolarSample(Random rs)
            {
                if (_orient == null) return SampleOnDirection(PolarSampling.Uniform(_dSpace.Dimension, rs), rs);
                else return SampleOnDirection(PolarSampling.Oriented(_orient, Math.PI / 3, rs), rs, 0);
            }
            private double[] SampleOnDirection(double[] d, Random rs, double rLb = double.NegativeInfinity, double rUb = double.PositiveInfinity)
            {
                // check constraints
                foreach (var constraint in _dSpace.Constraints)
                {
                    var a = constraint.Take(_dSpace.Dimension).ToArray();
                    var ad = Vector.DotProduct(a, d);
                    var b_minus_ax0 = constraint.Last() - Vector.DotProduct(a, _core);
                    if (ad > 0) rUb = Math.Min(rUb, b_minus_ax0 / ad);
                    else if (ad < 0) rLb = Math.Max(rLb, b_minus_ax0 / ad);
                }
                // check MPA hyper-retangular & dynamic bounds
                foreach (var i in _dSpace.Coords)
                {
                    var lb = _lbs[i] > double.NegativeInfinity ? _lbs[i] : _dynLbs[i];
                    var ub = _ubs[i] < double.PositiveInfinity ? _ubs[i] : _dynUbs[i];
                    if (d[i] > 0)
                    {
                        rUb = Math.Min(rUb, (ub - _core[i]) / d[i]);
                        rLb = Math.Max(rLb, (lb - _core[i]) / d[i]);
                    }
                    else if (d[i] < 0)
                    {
                        rUb = Math.Min(rUb, (lb - _core[i]) / d[i]);
                        rLb = Math.Max(rLb, (ub - _core[i]) / d[i]);
                    }
                }
                rLb = Math.Min(0, rLb); rUb = Math.Max(0, rUb);
                // sample radius
                var r = rLb + (rUb - rLb) * rs.NextDouble();
                // sample point
                return Vector.Add(_core, Vector.Multiply(d, r));
            }
            #endregion            
        }

    }
}
