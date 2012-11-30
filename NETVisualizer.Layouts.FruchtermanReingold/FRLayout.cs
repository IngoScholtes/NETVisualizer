using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

using NETVisualizer;

using OpenTK;

namespace NETVisualizer.Layouts.FruchtermanReingold
{
    /// <summary>
    ///  A spring-based model according to (T. Fruchterman and E. Reingold 1991). 
    ///  Edges are thought to be elastic springs that lead to an attractive force between connected vertices. 
    ///  Furthermore, there is an antagonistic, repulsive force between every pair of vertices. 
    ///  Computation is being done in parallel on as many processing cores as available. 
    /// </summary>
    public class FRLayout : LayoutProvider
    {

        /// <summary>
        /// A random generator
        /// </summary>
        public static Random r = new Random();

        /// <summary>
        ///  The number of iterations to be used in the computation of vertex positions
        /// </summary>
        private int _iterations = 0;

        /// <summary>
        /// A concurrent dictionary that keeps the vertex positions
        /// </summary>
        private ConcurrentDictionary<string, Vector3> _vertexPositions;

        /// <summary>
        /// A concurrent set of vertices for which new positions shall be calculated (e.g. because they were added or edges were changed)
        /// </summary>
        private ConcurrentBag<string> _dirtyVertices;

        /// <summary>
        /// Creates a Fruchterman/Reingold layout using a given number of iterations for the computation of forces and positions. 
        /// A larger iterations value will enhance the layouting quality, but will require more computational resources
        /// </summary>
        /// <param name="iterations"></param>
        public FRLayout(int iterations)
        {
            _iterations = iterations;
            _vertexPositions = new ConcurrentDictionary<string, Vector3>();
            _dirtyVertices = new ConcurrentBag<string>();
        }

        /// <summary>
        /// Initializes the data structures needed to compute the layout
        /// </summary>
        /// <param name="width">the width of the layout area</param>
        /// <param name="height">the height of the layout area</param>
        /// <param name="network">The network to layout</param>
        public override void Init(double width, double height, IRenderableNet network)
        {
            base.Init(width, height, network);

            // Generate random initial positions and mark all vertices as "dirty"
            foreach (string v in network.GetVertexArray())
            {
                _vertexPositions[v] = new Vector3((float) (r.NextDouble() * width), (float) (r.NextDouble() * height), 1f);
                _dirtyVertices.Add(v);
            }

            network.OnVertexChanged += new VertexChangedHandler(network_OnVertexChanged);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        void network_OnVertexChanged(string vertex)
        {
            if(!_vertexPositions.ContainsKey(vertex))
                _vertexPositions[vertex] = new Vector3((float)(r.NextDouble() * Width), (float)(r.NextDouble() * Height), 1f);

            _dirtyVertices.Add(vertex);
        }

        /// <summary>
        /// Returns the layout position of a node v
        /// </summary>
        /// <param name="v">The node to return the position for</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public override Vector3 GetPositionOfNode(string v)
        {
            if (!_vertexPositions.ContainsKey(v))
                _vertexPositions[v] = new Vector3((float) (r.NextDouble() * Width), (float) (r.NextDouble() * Height), 1f);
            
            return  _vertexPositions[v];
        }

        /// <summary>
        /// Computes the position of all vertices of a network
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public override void DoLayout()
        {
            DateTime start = DateTime.Now;
            double _area = Width * Height;
            double _k = Math.Sqrt(_area / (double)Network.GetVertexCount());
            _k *= 0.75d;

            // The displacement calculated for each vertex in each step
            var disp = new ConcurrentDictionary<string, Vector3>(System.Environment.ProcessorCount, (int) Network.GetVertexCount());

            double t = Width / 10;
            double tempstep = t / (double)_iterations;

            var vertices = Network.GetVertexArray();
            var edges = Network.GetEdgeArray();

            for (int i = 0; i < _iterations; i++)
            {
                // parallely compute repulsive forces of nodes to every new node
#if DEBUG
                foreach(var v in _dirtyVertices)
#else
                Parallel.ForEach(_dirtyVertices, v =>
#endif
                {
                    disp[v] = new Vector3(0f, 0f, 1f);

                    // computation of repulsive forces
                    foreach (string u in vertices)
                    {
                        if (v != u)
                        {
                            Vector3 delta = _vertexPositions[v] - _vertexPositions[u];
                            disp[v] = disp[v] + Vector3.Multiply(Vector3.Divide(delta, delta.Length), (float) repulsion(delta.Length, _k));
                        }
                    }
                }
#if !DEBUG
                );
#endif

                // Parallely calculate attractive forces for all edges
#if DEBUG
                foreach(var e in edges)
#else
                Parallel.ForEach(edges, e =>
#endif
                {
                    string v = e.Item1;
                    string w = e.Item2;
                    if (_vertexPositions.ContainsKey(v) && _vertexPositions.ContainsKey(w) && v!=w)
                    {
                        Vector3 delta = _vertexPositions[v] - _vertexPositions[w];
                        if (_dirtyVertices.Contains(v))
                            disp[v] = disp[v] - Vector3.Multiply(Vector3.Divide(delta, delta.Length), (float) attraction(delta.Length, _k));
                        if (_dirtyVertices.Contains(w))
                            disp[w] = disp[w] + Vector3.Multiply(Vector3.Divide(delta, delta.Length), (float) attraction(delta.Length, _k));
                    }
                }
#if !DEBUG
                );
#endif
                // Limit to frame and include temperature cooling that reduces displacement step by step
#if DEBUG
                foreach(var v in _dirtyVertices)
#else
                Parallel.ForEach(_dirtyVertices, v =>
#endif
                {
                    Vector3 vPos = _vertexPositions[v] + Vector3.Multiply(Vector3.Divide(disp[v], disp[v].Length), (float) Math.Min(disp[v].Length, t));
                    vPos.X = (float) Math.Min(Width - 10, Math.Max(10, vPos.X));
                    vPos.Y = (float) Math.Min(Height - 10, Math.Max(10, vPos.Y));
                    _vertexPositions[v] = vPos;
                }
#if !DEBUG
                );
#endif
                t -= tempstep;
                start = DateTime.Now;
            }
            _dirtyVertices = new ConcurrentBag<string>();
        }

        /// <summary>
        /// A simple attractive force between connected vertices
        /// </summary>
        /// <param name="d"></param>
        /// <param name="_k"></param>
        /// <returns></returns>
        private double attraction(double d, double _k)
        {
            return Math.Pow(d, 2d) / _k;
        }

        /// <summary>
        /// A simple repulsive force between every pair of vertices
        /// </summary>
        /// <param name="d"></param>
        /// <param name="_k"></param>
        /// <returns></returns>
        double repulsion(double d, double _k)
        {
            return Math.Pow(_k, 2d) / d;
        }
    }
}

