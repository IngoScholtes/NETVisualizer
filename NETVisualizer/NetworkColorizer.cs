using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace NETVisualizer
{

    /// <summary>
    /// A thread-safe storage for colors of nodes and edges
    /// </summary>
    public sealed class NetworkColorizer
    {
        private ConcurrentDictionary<string, Color> _customVertexColors;
        private ConcurrentDictionary<Tuple<string, string>, Color> _customEdgeColors;

        public Color DefaultVertexColor = Color.DarkSlateGray;
        public Color DefaultEdgeColor = Color.Gray;
        public Color DefaultBackgroundColor = Color.White;
        public Color DefaultSelectedVertexColor = Color.Red;

        /// <summary>
        /// Initializes a new instance of the <see cref="NETGen.Visualization.CustomColorIndexer"/> class.
        /// </summary>
        public NetworkColorizer()
        {
            _customEdgeColors = new ConcurrentDictionary<Tuple<string,string>, Color>(System.Environment.ProcessorCount, 0);
            _customVertexColors = new ConcurrentDictionary<string, Color>(System.Environment.ProcessorCount, 0);
        }

        /// <summary>
        /// Recomputes the colors of all previously customized vertices according to the specified lambda expression
        /// </summary>
        /// <param name='transform'>
        /// A lambda expression that asigns a Color to a vertex
        /// </param>
        public void RecomputeColors(Func<string, Color> transform)
        {
            lock(_customVertexColors)
                foreach (string v in _customVertexColors.Keys.ToArray())
                    _customVertexColors[v] = transform(v);
        }

        /// <summary>
        /// Recomputes the colors of all previously customized vertices according to the specified lambda expression
        /// </summary>
        /// <param name='transform'>
        /// A lambda expression that asigns a Color to a vertex
        /// </param>
        public void RecomputeColors(Func<Tuple<string, string>, Color> transform)
        {
            lock(_customEdgeColors)
                foreach (var e in _customEdgeColors.Keys.ToArray())
                    _customEdgeColors[e] = transform(e);
        }

        /// <summary>
        /// Removes all custom vertex and edge color assignments
        /// </summary>
        public void ClearAllColors()
        {
            lock(_customEdgeColors)
                lock (_customVertexColors)
                {
                    _customVertexColors.Clear();
                    _customEdgeColors.Clear();
                }
        }

        public void ClearEdgeColors()
        {
            lock(_customEdgeColors)
                _customEdgeColors.Clear();
        }

        public void ClearVertexColors()
        {
            lock(_customVertexColors)
                _customVertexColors.Clear();
        }

        /// <summary>
        /// Gets or sets the Color of the specified vertex
        /// </summary>
        /// <param name='v'>
        /// The vertex for which the color shall be set or returned
        /// </param>
        public Color this[string v]
        {
            get
            {
                lock (_customVertexColors)
                {
                    if (_customVertexColors.ContainsKey(v))
                        return _customVertexColors[v];
                    else
                        return DefaultVertexColor;
                }
            }
            set
            {
                lock (_customVertexColors)
                    _customVertexColors[v] = value;
            }
        }

        /// <summary>
        /// Gets or sets the Color of the specified edge
        /// </summary>
        /// <param name='e'>
        /// The edge for which the color shall be set or returned
        /// </param>
        public Color this[Tuple<string,string> e]
        {
            get
            {
                lock (_customEdgeColors)
                {
                    if (_customEdgeColors.ContainsKey(e))
                        return _customEdgeColors[e];
                    else
                        return DefaultEdgeColor;
                }
            }
            set
            {
                lock(_customEdgeColors)
                    _customEdgeColors[e] = value;
            }
        }
    }
}
