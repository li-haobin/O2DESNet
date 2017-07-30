using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Traffic
{
    internal class Dijkstra
    {
        public Dictionary<int, Dictionary<int, double>> Vertices { get; private set; }

        public Dijkstra() { Vertices = new Dictionary<int, Dictionary<int, double>>(); }
        public Dijkstra(List<Tuple<int, int, double>> edges)
        {
            Vertices = edges.Select(e => e.Item1).Concat(edges.Select(e => e.Item2)).Distinct().ToDictionary(i => i, i => new Dictionary<int, double>());
            foreach (var edge in edges)
            {
                int from = edge.Item1, to = edge.Item2;
                double distance = edge.Item3;
                Vertices[from].Add(to, distance);
            }
        }

        public void AddVertex(int id, Dictionary<int, double> edges)
        {
            Vertices[id] = edges;
        }
        public List<int> ShortestPath(int start, int finish)
        {
            Dictionary<int, int> prev;
            Dictionary<int, double> dist;
            return ShortestPath(start, finish, out prev, out dist);
        }
        public List<int> ShortestPath(int start, int finish, out Dictionary<int, int> previous, out Dictionary<int, double> distances)
        {
            var prev = new Dictionary<int, int>();
            var dist = new Dictionary<int, double>();
            var nodes = new List<int>();

            List<int> path = null;

            foreach (var vertex in Vertices)
            {
                if (vertex.Key == start)
                {
                    dist[vertex.Key] = 0;
                }
                else
                {
                    dist[vertex.Key] = double.PositiveInfinity;
                }

                nodes.Add(vertex.Key);
            }

            path = new List<int>();
            while (nodes.Count != 0)
            {
                nodes.Sort((x, y) => dist[x].CompareTo(dist[y]));

                var smallest = nodes[0];
                nodes.Remove(smallest);

                if (smallest == finish)
                {
                    //path = new List<int>();
                    while (prev.ContainsKey(smallest))
                    {
                        path.Add(smallest);
                        smallest = prev[smallest];
                    }

                    break;
                }

                if (dist[smallest] == double.PositiveInfinity)
                {
                    break;
                }

                foreach (var neighbor in Vertices[smallest])
                {
                    var alt = dist[smallest] + neighbor.Value;
                    if (alt < dist[neighbor.Key])
                    {
                        dist[neighbor.Key] = alt;
                        prev[neighbor.Key] = smallest;
                    }
                }
            }

            previous = prev;
            distances = dist;
            return path;
        }

        public static void Test()
        {
            Dijkstra g = new Dijkstra();
            g.AddVertex('A', new Dictionary<int, double>() { { 'B', 7 }, { 'C', 8 } });
            g.AddVertex('B', new Dictionary<int, double>() { { 'A', 7 }, { 'F', 2 } });
            g.AddVertex('C', new Dictionary<int, double>() { { 'A', 8 }, { 'F', 6 }, { 'G', 4 } });
            g.AddVertex('D', new Dictionary<int, double>() { { 'F', 8 } });
            g.AddVertex('E', new Dictionary<int, double>() { { 'H', 1 } });
            g.AddVertex('F', new Dictionary<int, double>() { { 'B', 2 }, { 'C', 6 }, { 'D', 8 }, { 'G', 9 }, { 'H', 3 } });
            g.AddVertex('G', new Dictionary<int, double>() { { 'C', 4 }, { 'F', 9 } });
            g.AddVertex('H', new Dictionary<int, double>() { { 'E', 1 }, { 'F', 3 } });

            g.ShortestPath('B', 'G').ForEach(x => Console.WriteLine((char)x));
        }
    }
}
