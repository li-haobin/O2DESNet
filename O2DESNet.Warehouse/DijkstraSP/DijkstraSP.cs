using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Warehouse.DijkstraSP
{
    /*
    * An implementation of Dijkstra's shortest path algorithm based on Sedgewick's implmentation.
    * The inputs are an edge weighted directed graph and an individual vertex in the graph.  The output
    * will be the shortest paths from the vertex to all other vertices it is connected to.
    * */
    public class DijkstraSP
    {
        /*
        * an array to keep track of the distance from our starting vertex to
        * all connected vertices
        * */
        private double[] _distTo;
        /*
        * an array of edges in our shortest paths
        * it is interesting to not for all vertices in the shortest paths
        * there is only one edge to each vertex which is why we can use
        * a simple DirectedEdge array
        * */
        private DirectedEdge[] _edgeTo;
        /*
        * the index minimum priority que helps us keep track of vertices we
        * want to evaluate to add to our shortest paths
        * as we travel to a vertex we add vertices directly connected to it to
        * _pq and then examine vertices in order from least distance
        * to the source node
        * */
        private IndexMinPriorityQueue<Double> _pq;

        /*
        * The inputs are an edge weighted directed graph
        * and a starting vertex
        * */
        public DijkstraSP(EdgeWeightedDigraph G, int s)
        {
            /*
            * for this implementation we don't want to evaluate graphs
            * with negative weights
            * */
            foreach (DirectedEdge e in G.Edges())
            {
                if (e.Weight() < 0)
                    throw new Exception("edge " + e + " has negative weight");
            }

            /*
            * initialize our arrays and set the default values
            * for distances to infinity
            * */
            _distTo = new double[G.V()];
            _edgeTo = new DirectedEdge[G.V()];
            for (int v = 0; v < G.V(); v++)
                _distTo[v] = Double.PositiveInfinity;
            _distTo[s] = 0.0;

            //initialize the minimum priority queue
            _pq = new IndexMinPriorityQueue<Double>(G.V());
            _pq.Insert(s, _distTo[s]);
            while (!_pq.IsEmpty())
            {
                /*
                * get the vertex whose distance to the source is the smallest and examine
                * vertices directly connected to it
                * */
                int v = _pq.DeleteMin();
                foreach (DirectedEdge e in G.Adj(v))
                    //call the Relax method to see if any connected vetex contains a new shortest path
                    Relax(e);
            }
        }

        /*
        * Relax is the main method for helping us determin what
        * the minimum path is to a connected vertex.
        * Basically, we evaluate wether we can improve on the shortest path
        * to w(the target vertex of the edge e) by going through v(the source
        * vertex of the edge e)
        * */
        private void Relax(DirectedEdge e)
        {
            //get the source and target vertex of the edge
            int v = e.From(), w = e.To();
            /*
            * _distTo[w] contains the shortest path so far to the
            * vertex w so we can compare it to the weight of of going through
            * v, if it is less use the new path instead
            * */
            if (_distTo[w] > _distTo[v] + e.Weight())
            {
                //set the distance to w to the new(lower) weight
                _distTo[w] = _distTo[v] + e.Weight();
                //add the edge to the list of edges in our shortest paths
                _edgeTo[w] = e;
                if (_pq.Contains(w))
                    /*
                    * if w is already in the priority que update its minimum path and re-order the que
                    * */
                    _pq.DecreaseKey(w, _distTo[w]);
                else
                    _pq.Insert(w, _distTo[w]);
            }
        }

        //Return the shortest distance to any connected vertex v
        public double DistTo(int v)
        {
            return _distTo[v];
        }

        //Return whether the source vertex has a path to any vertex in the graph
        public bool HasPathTo(int v)
        {
            return _distTo[v] < Double.PositiveInfinity;
        }

        /*
        * Get the path (the series of edges/vertices) to get from
        * the source vertex to a given connected vertex v.
        */
        public IEnumerable<DirectedEdge> PathTo(int v)
        {
            if (!HasPathTo(v)) return null;
            Stack<DirectedEdge> path = new Stack<DirectedEdge>();
            for (DirectedEdge e = _edgeTo[v]; e != null; e = _edgeTo[e.From()])
            {
                path.Push(e);
            }
            return path;
        }
    }


}
