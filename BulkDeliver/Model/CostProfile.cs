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
    }

    public class PieceCost
    {
        public int Id { get; set; }
        public double StartWeight { get; set; }
        public double UnitCost { get; set; }
    }
}
