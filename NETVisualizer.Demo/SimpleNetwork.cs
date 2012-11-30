using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NETVisualizer.Demo
{
    /// <summary>
    /// A simple undirected network implementation used for demonstration purposes
    /// </summary>
    public class SimpleNetwork : IRenderableNet
    {
        /// <summary>
        /// The list of vertices
        /// </summary>
        List<string> _vertices = new List<string>();

        /// <summary>
        /// The list of edges
        /// </summary>
        List<Tuple<string, string>> _edges = new List<Tuple<string, string>>();

        /// <summary>
        /// Adds a vertex to the network
        /// </summary>
        /// <param name="v"></param>
        public void AddVertex(string v)
        {
            _vertices.Add(v);
        }

        /// <summary>
        /// Adds an undirected edge to the network
        /// </summary>
        /// <param name="v">source node</param>
        /// <param name="w">target node</param>
        public void AddEdge(string v, string w)
        {
            if (!_vertices.Contains(v))
                _vertices.Add(v);
            if (!_vertices.Contains(w))
                _vertices.Add(w);
            _edges.Add(new Tuple<string, string>(v, w));

            if (OnVertexChanged != null)
            {
                OnVertexChanged(v);
                OnVertexChanged(w);
            }            
        }

        /// <summary>
        /// Generates a simple random network with n nodes and m edges
        /// </summary>
        /// <param name="n">The number of nodes</param>
        /// <param name="m">The number of edges</param>
        /// <returns>A random network</returns>
        public static SimpleNetwork CreateRandomNetwork(int n, int m)
        {
            Random r = new Random();
            SimpleNetwork net = new SimpleNetwork();

            for(int i = 0; i<n; i++)
                net.AddVertex(i.ToString());                

            for (int j = 0; j < m; j++)
                {
                    string x = net.GetVertexArray()[r.Next(net.GetVertexCount())];
                    string y = net.GetVertexArray()[r.Next(net.GetVertexCount())];
                    net.AddEdge(x, y);
                }
            return net;
        }

        /// <summary>
        /// Returns an array of vertices
        /// </summary>
        /// <returns></returns>
        public string[] GetVertexArray()
        {
            return _vertices.ToArray();
        }

        /// <summary>
        /// Returns an array of edges
        /// </summary>
        /// <returns></returns>
        public Tuple<string, string>[] GetEdgeArray()
        {
            return _edges.ToArray();
        }

        /// <summary>
        /// Returns all neighbors of a node v
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public string[] GetSuccessorArray(string v)
        {
            return (from w in _edges where w.Item1 == v select w.Item2).Concat(from w in _edges where w.Item2 == v select w.Item1).ToArray();            
        }

        /// <summary>
        /// Returns all neighbors of a node w
        /// </summary>
        /// <param name="w"></param>
        /// <returns></returns>
        public string[] GetPredecessorArray(string w)
        {
            return GetSuccessorArray(w);
        }

        /// <summary>
        /// Returns the number of vertices
        /// </summary>
        /// <returns></returns>
        public int GetVertexCount()
        {
            return _vertices.Count;
        }

        /// <summary>
        /// Returns the number of edges
        /// </summary>
        /// <returns></returns>
        public int GetEdgeCount()
        {
            return _edges.Count;
        }

        public event VertexChangedHandler OnVertexChanged;

    }
}
