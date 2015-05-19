using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkDeliver.Model
{
    public class CostProfile
    {
        public int Id { get; set; }
        public double Constan { get; set; }
        public ICollection<PieceCost> Pieces { get; set; }
        public double Calculate(double weight)
        {
            double cost = Constan;
            var current = new PieceCost{ StartWeight=0, UnitCost=0};
            foreach (var next in Pieces)
            {
                if (weight < next.StartWeight) break;
                cost += current.UnitCost * (next.StartWeight - current.StartWeight);
                current = next;
            }
            cost += current.UnitCost * (weight - current.StartWeight);
            return cost;
        }
        public static CostProfile GetCostProfile(double constan, IEnumerable<double[]> pieces)
        {
            return new CostProfile
            {
                Constan = constan,
                Pieces = pieces.Select(p => new PieceCost
                {
                    StartWeight = p[0],
                    UnitCost = p[1]
                }).OrderBy(p => p.StartWeight).ToList()
            };
        }
        public static CostProfile ExampleAirfreight
        {
            get
            {
                return GetCostProfile(1000, new List<double[]>
                {
                    new double[] { 200, 5 },
                    new double[] { 1000, 4 },
                    new double[] { 2000, 3.5 },
                    new double[] { 3000, 3 }
                });
            }
        }
        public static CostProfile ExampleContainer
        {
            get
            {
                return GetCostProfile(0, new List<double[]>
                {
                    new double[] { 0, 8000000 },
                    new double[] { 0.001, 0 },
                    new double[] { 5000, 8000000 },
                    new double[] { 5000.001, 0 }
                });
            }
        }
        public static CostProfile ExamplePallete
        {
            get
            {
                return GetCostProfile(2000, new List<double[]>
                {
                    new double[] { 0, 1840000 },
                    new double[] { 0.001, 0 },
                    new double[] { 1000, 1840000 },
                    new double[] { 1000.001, 0 },
                    new double[] { 2000, 1840000 },
                    new double[] { 2000.001, 0 },
                    new double[] { 3000, 1840000 },
                    new double[] { 3000.001, 0 },
                    new double[] { 4000, 1840000 },
                    new double[] { 4000.001, 0 },
                    new double[] { 5000, 1840000 },
                    new double[] { 5000.001, 0 },
                    new double[] { 6000, 1840000 },
                    new double[] { 6000.001, 0 },
                    new double[] { 7000, 1840000 },
                    new double[] { 7000.001, 0 },
                    new double[] { 8000, 1840000 },
                    new double[] { 8000.001, 0 },
                    new double[] { 9000, 1840000 },
                    new double[] { 9000.001, 0 },
                    new double[] { 10000, 1840000 },
                    new double[] { 10000.001, 0 },
                    new double[] { 11000, 1840000 }
                });
            }
        }
    }

    public class PieceCost
    {
        public int Id { get; set; }
        public double StartWeight { get; set; }
        public double UnitCost { get; set; }
    }
}
