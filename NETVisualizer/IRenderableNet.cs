using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NETVisualizer
{
    /// <summary>
    /// A delegate that will be called whenever a vertex was changed
    /// </summary>
    /// <param name="vertex"></param>
    public delegate void VertexChangedHandler(string vertex);

    /// <summary>
    /// An interface that needs to be implemented by every type of network 
    /// that shall be renderable with NETVisualizer
    /// </summary>
    public interface IRenderableNet
    {
        /// <summary>
        /// Returns an array of vertices of the network
        /// </summary>
        /// <returns></returns>
        string[] GetVertexArray();

        /// <summary>
        /// Returns an array of edges of the network
        /// </summary>
        /// <returns></returns>
        Tuple<string, string>[] GetEdgeArray();

        /// <summary>
        /// Returns all successors of a particular node
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        string[] GetSuccessorArray(string v);

        /// <summary>
        /// Returns all predecessors of a particular node
        /// </summary>
        /// <param name="w"></param>
        /// <returns></returns>
        string[] GetPredecessorArray(string w);

        /// <summary>
        /// Returns the number of vertices in the network
        /// </summary>
        /// <returns></returns>
        int GetVertexCount();

        /// <summary>
        /// Returns the number of edges in the network
        /// </summary>
        /// <returns></returns>
        int GetEdgeCount();        

        /// <summary>
        /// An event that will be fired whenever a vertex was changed
        /// </summary>
        event VertexChangedHandler OnVertexChanged;
    }
}
