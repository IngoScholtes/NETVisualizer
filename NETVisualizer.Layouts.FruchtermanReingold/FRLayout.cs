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

        public double AreaMultiplicator
        {
            get;
            set;
        }

        public double SpeedDivisor
        {
            get;
            set;
        }

        public double Speed
        {
            get;
            set;
        }

        public double Gravity
        {
            get;
            set;
        }

        public double RepulsionFactor
        {
            get;
            set;
        }

        public int Iterations
        {
            get;
            set;
        } 


        /// <summary>
        /// A concurrent dictionary that keeps the vertex positions
        /// </summary>
        private ConcurrentDictionary<string, Vector3> _vertexPositions;

        /// <summary>
        /// Creates a Fruchterman/Reingold layout using a given number of iterations for the computation of forces and positions. 
        /// A larger iterations value will enhance the layouting quality, but will require more computational resources
        /// </summary>
        /// <param name="iterations"></param>
        public FRLayout(int iterations)
        {
            Iterations = iterations;
            RepulsionFactor = 1d;
            AreaMultiplicator = 10000d;
            Speed = 1d;
            SpeedDivisor = 800d;
            Gravity = 10d;

            _vertexPositions = new ConcurrentDictionary<string, Vector3>();
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

            CreateRandomState();

            network.OnVertexChanged += new VertexChangedHandler(network_OnVertexChanged);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        void network_OnVertexChanged(string vertex)
        {
            if(!_vertexPositions.ContainsKey(vertex))
                _vertexPositions[vertex] = new Vector3((float)(r.NextDouble() * Width), (float)(r.NextDouble() * Height), 1f);

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

        private void CreateRandomState()
        {
            foreach (string v in base.Network.GetVertexArray())
            {
                _vertexPositions[v] = new Vector3((float)(r.NextDouble() * base.Width - base.Width / 2d), (float)(r.NextDouble() * base.Height - base.Height / 2d), 1f);
            }
        }   

        /// <summary>
        /// Computes the position of all vertices of a network
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public override void DoLayout()
        {
            DateTime start = DateTime.Now;
            double area = Width * Height;

            // The displacement calculated for each vertex in each step
            var disp = new ConcurrentDictionary<string, Vector3>(System.Environment.ProcessorCount, (int)Network.GetVertexCount());

            // Some area dependent parameters
            double maxDist = (Math.Sqrt(area * AreaMultiplicator) / 10d);
            double k = Math.Sqrt(AreaMultiplicator * area) / (1d + Network.GetVertexCount());

            var vertices = Network.GetVertexArray();
            var edges = Network.GetEdgeArray();

            foreach (string v in vertices)
                disp[v] = new Vector3(0f, 0f, 1f);

            for (int i = 0; i < Iterations; i++)
            {
                // parallely compute repulsive forces of nodes to every new node
#if DEBUG
                foreach (var v in vertices)//
#else
                Parallel.ForEach(vertices, v =>
#endif
                    {
                        foreach (string u in vertices)
                        {
                            if (v != u)
                            {
                                // Compute repulsive force

                                Vector3 delta = _vertexPositions[v] - _vertexPositions[u];
                                disp[v] = disp[v] + Vector3.Multiply(Vector3.Divide(delta, delta.Length), (float)(RepulsionFactor * k * k / delta.Length));

                                // Compute attractive force
                                disp[v] = disp[v] - Vector3.Multiply(Vector3.Divide(delta, delta.Length), (float)attraction(delta.Length, k, v, u));
                                disp[u] = disp[u] + Vector3.Multiply(Vector3.Divide(delta, delta.Length), (float)attraction(delta.Length, k, v, u));
                            }
                        }
                    }
#if !DEBUG
                );
#endif
                foreach (string v in vertices)
                {
                        double dist = disp[v].Length;
                        double new_x = disp[v].X - (0.01d * k * Gravity * _vertexPositions[v].X);
                        double new_y = disp[v].Y - (0.01d * k * Gravity * _vertexPositions[v].Y);
                        disp[v] = new Vector3((float)new_x, (float)new_y, 1f);
                }
#if DEBUG
                foreach (var v in vertices)
#else
                Parallel.ForEach(vertices, v =>
#endif
                    {
                        Vector3 vPos = _vertexPositions[v] + Vector3.Multiply(Vector3.Divide(disp[v], disp[v].Length),
                            (float)Math.Min(disp[v].Length, maxDist * (Speed / SpeedDivisor)));
                        // We skip the limitation to a certain frame, since we can still pan and zoom ... 
                        //vPos.X = (float)Math.Min(Width - 10, Math.Max(10, vPos.X));
                        //vPos.Y = (float)Math.Min(Height - 10, Math.Max(10, vPos.Y));
                        _vertexPositions[v] = vPos;
                        disp[v] = new Vector3(0f, 0f, 1f);
                    }
#if !DEBUG
                );
#endif
            }
        }

        /// <summary>
        /// A simple attractive force between connected vertices
        /// </summary>
        /// <param name="d"></param>
        /// <param name="_k"></param>
        /// <returns></returns>
        private double attraction(double distance, double k, string v, string w)
        {
            double attraction = this.Network.Edge(v,w) ? distance * distance / k : 0d;
            return attraction * AreaMultiplicator;
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

