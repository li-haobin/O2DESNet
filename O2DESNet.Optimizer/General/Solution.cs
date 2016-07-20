using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Optimizer
{
    /// <summary>
    /// Solution with deterministic objective values
    /// </summary>
    public class Solution
    {
        public DenseVector Decisions { get; private set; }
        public DenseVector Objectives { get; protected set; }
        public Solution(DenseVector decisions, DenseVector objectives = null)
        {
            Decisions = decisions;
            Objectives = objectives;
        }
        public virtual void Evaluate(DenseVector objectives)
        {
            if (Objectives != null && Objectives.Count != objectives.Count) throw new Exception_InconsistentDimensions();
            Objectives = objectives;
        }        

        /// <summary>
        /// A unified gradient for multiple objectives
        /// </summary>
        public virtual DenseVector UniGradient { get; set; }

        public override string ToString()
        {
            return string.Format("{0} -> {1}", ToString(Decisions), ToString(Objectives));
        }

        private string ToString(Vector vector)
        {
            string str = "[";
            foreach (var d in vector) str += string.Format("{0},", d);
            str = str.Substring(0, str.Length - 1) + "]";
            return str;
        }
    }
}
