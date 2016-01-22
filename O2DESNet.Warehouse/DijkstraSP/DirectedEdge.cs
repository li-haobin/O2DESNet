using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace O2DESNet.Warehouse.DijkstraSP
{
    public class DirectedEdge
    {
        private readonly int _v;//The source vertex
        private readonly int _w;//The target vertex
        private readonly double _weight;//The weight to go from _v to _w

        //Create a directed edge from v to w with weight 'weight'
        public DirectedEdge(int v, int w, double weight)
        {
            this._v = v;
            this._w = w;
            this._weight = weight;
        }

        //Return the weight
        public double Weight()
        {
            return _weight;
        }

        //Return the source vertex
        public int From()
        {
            return _v;
        }

        //Return the target vertex
        public int To()
        {
            return _w;
        }

        //Return a string representation of the edge
        public override string ToString()
        {
            return String.Format("{0:d}->{1:d} {2:f}", _v, _w, _weight);
        }
    }


}
