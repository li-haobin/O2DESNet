using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace O2DESNet.Warehouse
{
    internal class Dijkstra
    {
        private const string dll = @"dijkstra.dll";

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr dijkstra_algorithm(int num_nodes, int num_edges, ref int from_indices, ref int to_indices, ref double weights);

        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        private static extern int release_memory(IntPtr ptr);

        internal int NumNodes { get; private set; }
        internal Edge[] Edges { get; private set; }
        internal int[] Parents { get; private set; }
        private double?[] _shortestDistances;
        private List<int>[] _shortestPaths;

        /// <summary>
        /// Constract a shortest path problem with single source at index 0, and solve by Dijkstra algorithm
        /// </summary>
        internal Dijkstra(params Edge[] edges)
        {
            // intialize properties
            Edges = Standardize(edges);
            NumNodes = Math.Max(Edges.Max(e => e.FromIndex), Edges.Max(e => e.ToIndex)) + 1;

            // call C++ dll with boost implementation
            var fromIndices = Edges.Select(e => e.FromIndex).ToArray();
            var toIndices = Edges.Select(e => e.ToIndex).ToArray();
            var weights = Edges.Select(e => e.Distance).ToArray();
            var ptr = dijkstra_algorithm(
                NumNodes, Edges.Length, ref fromIndices[0], ref toIndices[0], ref weights[0]);

            //var DLL = Assembly.LoadFile(dll);

            //foreach (Type type in DLL.GetExportedTypes())
            //{
            //    var c = Activator.CreateInstance(type);
            //    type.InvokeMember("dijkstra_algorithm", BindingFlags.InvokeMethod, null, c, new object[] {NumNodes, Edges.Length,
            //        fromIndices, toIndices, weights });
            //}

            Parents = new int[NumNodes];
            Marshal.Copy(ptr, Parents, 0, NumNodes);
            release_memory(ptr);

            // initialize shortest paths & distances, for lazy calculation
            _shortestPaths = Enumerable.Range(0, NumNodes).Select(i => (List<int>)null).ToArray();
            _shortestPaths[0] = new List<int> { 0 };
            _shortestDistances = Enumerable.Repeat((double?)null, NumNodes).ToArray();
            _shortestDistances[0] = 0;
        }
        private Edge[] Standardize(Edge[] edges)
        {
            var min = edges.Min(e => e.Distance);
            if (min < 0) return edges.Select(e => new Edge(e.FromIndex, e.ToIndex, e.Distance - min)).ToArray();
            return edges.ToArray();
        }

        internal List<int> GetShortestPath(int index)
        {
            if (_shortestPaths[index] == null)
            {
                if (Parents[index] == index) _shortestPaths[index] = new List<int>();
                else
                {
                    _shortestPaths[index] = new List<int>(GetShortestPath(Parents[index]));
                    _shortestPaths[index].Add(index);
                }
            }
            return _shortestPaths[index];
        }

        internal double GetShortestDistance(int index)
        {
            if (_shortestDistances[index] == null)
            {
                if (Parents[index] == index) _shortestDistances[index] = double.PositiveInfinity;
                else
                    _shortestDistances[index] = GetShortestDistance(Parents[index]) +
                   Edges.Where(e => e.FromIndex == Parents[index] && e.ToIndex == index).First().Distance;
            }
            return _shortestDistances[index].Value;
        }

        internal class Edge
        {
            internal int FromIndex { get; private set; }
            internal int ToIndex { get; private set; }
            internal double Distance { get; private set; }
            internal Edge(int from, int to, double distance) { FromIndex = from; ToIndex = to; Distance = distance; }
        }

        /// <summary>
        /// The test program
        /// </summary>
        internal static void TestProgram()
        {
            var dijkstra = new Dijkstra(
                new Edge(0, 2, 1.9),
                new Edge(1, 1, 2.6),
                new Edge(1, 3, 1.3),
                new Edge(1, 4, 2.1),
                new Edge(2, 1, 7.2),
                new Edge(2, 3, 3.5),
                new Edge(3, 4, 1.5),
                new Edge(4, 0, 1.4),
                new Edge(4, 1, 1.2)
                );

            foreach (var i in Enumerable.Range(0, dijkstra.NumNodes))
            {
                Console.Write("{0}\t{1}\t", i, dijkstra.GetShortestDistance(i));
                foreach (var j in dijkstra.GetShortestPath(i)) Console.Write("{0},", j);
                Console.WriteLine();
            }

        }
    }
}
