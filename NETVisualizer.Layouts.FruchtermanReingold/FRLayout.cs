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
    ///  A spring-based model according to (T. Fruchterman and E. Reingold 1991). Edges are thought to be elastic springs that lead to an attractive force between connected vertices. Furthermore, there is an antagonistic, repulsive force between every pair of vertices. Computation is being done in parallel on as many processing cores as available. 
    /// </summary>
    public class FRLayout : LayoutProvider
    {

        public static Random r = new Random();

        /// <summary>
        ///  The number of iterations to be used in the computation of vertex positions
        /// </summary>
        private int _iterations = 0;

        private ConcurrentDictionary<string, Vector3> _vertexPositions;

        private ConcurrentBag<string> _newVertices;

        /// <summary>
        /// Creates a Fruchterman/Reingold layout using a given number of iterations for the computation of forces and positions. A larger iterations value will enhance the layouting quality, but will require more computation
        /// </summary>
        /// <param name="iterations"></param>
        public FRLayout(int iterations)
        {
            _iterations = iterations;
            _vertexPositions = new ConcurrentDictionary<string, Vector3>();
            _newVertices = new ConcurrentBag<string>();
        }

        public override void Init(double width, double height, IRenderableNet network)
        {
            base.Init(width, height, network);

            foreach (string v in network.GetVertexArray())
            {
                _vertexPositions[v] = new Vector3((float) (r.NextDouble() * width), (float) (r.NextDouble() * height), 1f);
                _newVertices.Add(v);
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public override void TouchVertex(string v)
        {
            _newVertices.Add(v);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public override void TouchEdge(string v, string w)
        {
            _newVertices.Add(v);
            _newVertices.Add(w);
        }

        /// <summary>
        /// Computes the position of all vertices of a network
        /// </summary>
        /// <param name="width">width of the frame</param>
        /// <param name="height">height of the frame</param>
        /// <param name="n">The network to compute the layout for</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public override void DoLayout()
        {
            DateTime start = DateTime.Now;
            double _area = Width * Height;
            double _k = Math.Sqrt(_area / (double)Network.GetVertexCount());
            _k *= 0.75d;

            // The displacement calculated for each vertex in each step
            ConcurrentDictionary<string, Vector3> disp = new ConcurrentDictionary<string, Vector3>(System.Environment.ProcessorCount, (int) Network.GetVertexCount());

            double t = Width / 10;
            double tempstep = t / (double)_iterations;

            var vertices = Network.GetVertexArray();
            var edges = Network.GetEdgeArray();

            for (int i = 0; i < _iterations; i++)
            {
                // parallely Calculate repulsive forces of nodes to every new node
                Parallel.ForEach(_newVertices, v =>
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
                });

                // Parallely calculate attractive forces for all pairs of connected nodes
                Parallel.ForEach(edges, e =>
                {
                    string v = e.Item1;
                    string w = e.Item2;
                    if (_vertexPositions.ContainsKey(v) && _vertexPositions.ContainsKey(w))
                    {
                        Vector3 delta = _vertexPositions[v] - _vertexPositions[w];
                        if (_newVertices.Contains(v))
                            disp[v] = disp[v] - Vector3.Multiply(Vector3.Divide(delta, delta.Length), (float) attraction(delta.Length, _k));
                        if (_newVertices.Contains(w))
                            disp[w] = disp[w] + Vector3.Multiply(Vector3.Divide(delta, delta.Length), (float)attraction(delta.Length, _k));
                    }
                });

                // Limit to frame and include temperature cooling that reduces displacement step by step
                Parallel.ForEach(_newVertices, v =>
                {
                    Vector3 vPos = _vertexPositions[v] + Vector3.Multiply(Vector3.Divide(disp[v], disp[v].Length), (float) Math.Min(disp[v].Length, t));
                    vPos.X = (float) Math.Min(Width - 10, Math.Max(10, vPos.X));
                    vPos.Y = (float) Math.Min(Height - 10, Math.Max(10, vPos.Y));
                    _vertexPositions[v] = vPos;
                });
                t -= tempstep;

                //Logger.AddMessage(LogEntryType.Info, string.Format("Layout step {0} computed in {1} ms", i, (DateTime.Now - start).TotalMilliseconds.ToString()));
                start = DateTime.Now;
            }
            _newVertices = new ConcurrentBag<string>();
            //Logger.AddMessage(LogEntryType.Info, "Layout completed");
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

        /// <summary>
        /// Returns the layout position of a node v
        /// </summary>
        /// <param name="v">The node to return the position for</param>
        /// <returns></returns>
        public override Vector3 GetPositionOfNode(string v)
        {
            if (_vertexPositions.ContainsKey(v))
                return _vertexPositions[v];
            else return new Vector3();
        }
    }
}

