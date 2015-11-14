using BulkDeliver.Model;
using ILOG.Concert;
using ILOG.CPLEX;
using MathNet.Numerics.Distributions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkDeliver
{
    class CplexSolver
    {
        private Scenario _scenario;
        private Cplex _model;
        private Random _rs;

        public CplexSolver(Scenario scenario, int nDays, int nSamples, int seed = 0)
        {
            _scenario = scenario;
            _model = new Cplex();
            _rs = new Random(seed);

            //decision variables
            INumVar weightThreshold = _model.NumVar(0, double.PositiveInfinity);
            INumVar daysThreshold = _model.NumVar(0, double.PositiveInfinity);
            // dependant variables
            INumVar[] sampleCosts = _model.NumVarArray(nSamples, 0, double.PositiveInfinity);

            double[] inWeights, inHoldingCosts;
            for (int i = 0; i < nSamples; i++)
            {
                //int i = 0;
                Sample(nDays, out inWeights, out inHoldingCosts);
                INumVar[] startingWeights = _model.NumVarArray(nDays, 0, double.PositiveInfinity);
                INumVar[] maxDelays = _model.NumVarArray(nDays, 0, double.PositiveInfinity, NumVarType.Int);
                INumVar[] toDeliver = _model.BoolVarArray(nDays);
                INumVar[] holdingCosts = _model.NumVarArray(nDays, 0, double.PositiveInfinity);
                INumVar[] deliveryCosts = _model.NumVarArray(nDays, 0, double.PositiveInfinity);
                _model.AddEq(startingWeights[0], 0);
                for (int j = 0; j < nDays; j++)
                {
                    if (j < nDays - 1)
                    {
                        // balancing
                        _model.Add(_model.IfThen(_model.Eq(toDeliver[j], 1), _model.Eq(startingWeights[j + 1], 0)));
                        _model.Add(_model.IfThen(_model.Eq(toDeliver[j], 0), _model.Eq(startingWeights[j + 1], _model.Sum(startingWeights[j], inWeights[j]))));
                        _model.Add(_model.IfThen(_model.Eq(toDeliver[j], 1), _model.Eq(maxDelays[j + 1], 0)));
                        _model.Add(_model.IfThen(_model.Eq(toDeliver[j], 0), _model.Eq(maxDelays[j + 1], _model.Sum(maxDelays[j], 1))));
                        // holding cost
                        _model.Add(_model.IfThen(_model.Eq(toDeliver[j], 1), _model.Eq(holdingCosts[j + 1], 0)));
                        _model.Add(_model.IfThen(_model.Eq(toDeliver[j], 0), _model.Eq(holdingCosts[j + 1], _model.Sum(holdingCosts[j], inHoldingCosts[j]))));
                    }
                    // threshold trigering
                    _model.Add(_model.IfThen(_model.Ge(_model.Sum(startingWeights[j], inWeights[j]), weightThreshold), _model.Eq(toDeliver[j], 1)));
                    _model.Add(_model.IfThen(_model.Ge(maxDelays[j], daysThreshold), _model.Eq(toDeliver[j], 1)));
                    // delivery cost
                    Constrain_PiecewiseLinearCost(_scenario.DeliveryCost, _model.Eq(toDeliver[j], 1), _model.Sum(startingWeights[j], inWeights[j]), deliveryCosts[j]);
                }
                _model.AddEq(sampleCosts[i], _model.Prod(1.0 / nDays, _model.Sum(_model.Sum(holdingCosts), _model.Sum(deliveryCosts))));
                // end trigering
                _model.Add(_model.IfThen(_model.Ge(_model.Sum(startingWeights[nDays - 1], inWeights[nDays - 1]), 0), _model.Eq(toDeliver[nDays - 1], 1)));
            }


            _model.AddMinimize(_model.Sum(_model.Prod(1.0 / nSamples, _model.Sum(sampleCosts)), _model.Prod(0.000001, weightThreshold)));

            if (_model.Solve())
            {
                Console.Clear();
                Console.WriteLine("Min. Average Sample Daily Cost = {0}", _model.GetObjValue());

                //var values1 = _model.GetValues(deliveryCosts); var sum1 = values1.Sum();
                //var values2 = _model.GetValues(holdingCosts); var sum2 = values2.Sum();
                //var values3 = _model.GetValues(toDeliver);
                //var values4 = _model.GetValues(maxDelays);

                Console.WriteLine("Decisions = {0}, {1}",
                    _model.GetValue(weightThreshold), _model.GetValue(daysThreshold));
            }
            else { Console.WriteLine("Failed to optimize LP."); }
        }

        private void Constrain_PiecewiseLinearCost(CostProfile costProfile, IConstraint activator, INumExpr weight, INumExpr cost)
        {
            double constan = costProfile.Constan;
            double unitCost = 0, startWeight = 0;
            foreach (var pieceCost in costProfile.Pieces)
            {
                _model.Add(_model.IfThen(activator, _model.IfThen(
                    _model.Ge(weight, startWeight), _model.IfThen(_model.Le(weight, pieceCost.StartWeight),
                        _model.Eq(cost, _model.Sum(constan, _model.Prod(unitCost, _model.Sum(weight, -startWeight))))))));
                constan += unitCost * (pieceCost.StartWeight - startWeight);
                startWeight = pieceCost.StartWeight;
                unitCost = pieceCost.UnitCost;
            }
            _model.Add(_model.IfThen(activator, _model.IfThen(
                    _model.Ge(weight, startWeight),
                    _model.Eq(cost, _model.Sum(constan, _model.Prod(unitCost, _model.Sum(weight, -startWeight)))))));
        }

        private void Sample(int nDays, out double[] inWeights, out double[] inHoldingCosts)
        {
            inWeights = Enumerable.Repeat(0.0, nDays).ToArray();
            inHoldingCosts = Enumerable.Repeat(0.0, nDays).ToArray();
            foreach (var itemType in _scenario.ItemTypes)
            {
                var lambda = 1.0 / itemType.IAT_Expected.TotalDays;
                for (int i = 0; i < nDays; i++)
                {
                    var count = Poisson.Sample(_rs, lambda);
                    for (int j = 0; j < count; j++)
                    {
                        inWeights[i] += itemType.Weight_Mean + itemType.Weight_Offset * (_rs.NextDouble() - 0.5);
                        inHoldingCosts[i] += (itemType.Value_Mean + itemType.Value_Offset * (_rs.NextDouble() - 0.5)) * itemType.DailyInventoryCostRatio;
                    }
                }
            }
            
        }
    }
}
