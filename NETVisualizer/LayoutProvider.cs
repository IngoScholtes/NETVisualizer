using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;

using OpenTK;

namespace NETVisualizer
{
    /// <summary>
    /// An abstract class which custom layout alkgorithms need to inherit. 
    /// This is the point of extensibility for any custom NETVisualizer.Layouts modules
    /// </summary>
    public abstract class LayoutProvider
    {
        /// <summary>
        /// The width of the layout area
        /// </summary>
        protected double Width;

        /// <summary>
        /// The height of the layout area
        /// </summary>
        protected double Height;

        /// <summary>
        /// The network to render
        /// </summary>
        protected IRenderableNet Network;

        /// <summary>
        /// Returns the position of a vertex in the network
        /// </summary>
        /// <returns>
        /// The position of vertex v
        /// </returns>
        /// <param name='v'>
        /// The vertex for which the position shall be returned
        /// </param>
        public abstract Vector3 GetPositionOfNode(string v);

        /// <summary>
        /// Initializes the layout area and the network to render
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="network"></param>
        public virtual void Init(double width, double height, IRenderableNet network)
        {
            Width = width;
            Height = height;
            Network = network;
        }

        /// <summary>
        /// Computes all vertex positions of a network. 
        /// This will be called whenever a layout has to be computed for 
        /// the first time or whenever the recomputation of the layout is enforced.
        /// </summary>
        public abstract void DoLayout();

        /// <summary>
        /// Asynchronously computes the layout.
        /// </summary>
        /// <param name='layoutCompleted'>
        /// An optional lambda expression that will be executed after the layout has been completed. 
        /// </param>
        public void DoLayoutAsync(Action layoutCompleted = null)
        {
            System.Threading.ThreadPool.QueueUserWorkItem(delegate(object o)
            {
                DoLayout();
                if (layoutCompleted != null)
                    layoutCompleted();
            });
        }
    }
}
