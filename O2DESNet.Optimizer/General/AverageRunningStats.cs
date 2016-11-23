using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Optimizer
{
    public class AverageRunningStats
    {
        public List<Tuple<double, double>>[] DataArray { get; private set; }
        public AverageRunningStats(int nRuns)
        {
            DataArray = Enumerable.Range(0, nRuns).Select(s => new List<Tuple<double, double>>()).ToArray();
        }
        public void Log(int runId, double time, double value) { DataArray[runId].Add(new Tuple<double, double>(time, value)); }
        public List<Tuple<double, double>> Output
        {
            get
            {
                var dataArray = DataArray.Where(l => l.Count > 0).ToArray();
                var maxT = dataArray.Max(data => data.Last().Item1);
                foreach (var data in dataArray) data.Add(new Tuple<double, double>(maxT, data.Last().Item2));

                var pointers = dataArray.Select(l => 0).ToArray();
                var indices = Enumerable.Range(0, pointers.Length).ToList();
                var output = new List<Tuple<double, double>>();
                var tCut = indices.Max(i => dataArray[i][pointers[i]].Item1);
                while (true)
                {
                    foreach (var i in indices)
                        while (pointers[i] < dataArray[i].Count - 1 && dataArray[i][pointers[i] + 1].Item1 <= tCut)
                            pointers[i]++;
                    output.Add(new Tuple<double, double>(tCut, indices.Average(i => dataArray[i][pointers[i]].Item2)));
                    indices = indices.Where(i => pointers[i] < dataArray[i].Count - 1).ToList();
                    if (indices.Count == 0) break;
                    //if (indices.Count(i => pointers[i] < dataArray[i].Count - 1) == 0) break;
                    tCut = indices.Min(i => dataArray[i][pointers[i] + 1].Item1);
                }
                return output;
            }
        }
    }
}
