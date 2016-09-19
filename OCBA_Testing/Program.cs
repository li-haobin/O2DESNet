using O2DESNet.Optimizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCBA_Testing
{
    class Program
    {
        static void Main(string[] args)
        {
            var rns_ocba = new Example_3_5();
            var rns_eq = new Example_3_5();

            for (int i = 0; i < 5; i++)
            {
                rns_ocba.Evaluate(i, 10);
                rns_eq.Evaluate(i, 10);
            }

            var ocba = new OCBA();
            var eq = new EqualAlloc();

            int totalBudget = 0;
            while (true)
            {
                totalBudget += 10;

                var alloc_ocba = ocba.Alloc(
                    budget: Math.Max(0, totalBudget - rns_ocba.Solutions.Sum(s => s.Observations.Count)),
                    solutions: rns_ocba.Solutions
                    );
                var alloc_eq = eq.Alloc(
                    budget: Math.Max(0, totalBudget - rns_eq.Solutions.Sum(s => s.Observations.Count)),
                    solutions: rns_eq.Solutions
                    );
                foreach (var a in alloc_ocba) rns_ocba.Evaluate(index: (int)a.Key[0], budget: a.Value);
                foreach (var a in alloc_eq) rns_eq.Evaluate(index: (int)a.Key[0], budget: a.Value);

                Console.WriteLine("Target Budget:{0}\tPCS (OCBA): {1:F4}\tPCS (EA): {2:F4}", totalBudget, rns_ocba.PCS, rns_eq.PCS);
                Console.ReadKey();
            }
        }
    }
}
