using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NETVisualizer
{
    /// <summary>
    /// A simple interface that needs to be implemented by every renderable network
    /// </summary>
    public interface IRenderableNet
    {
        string[] GetVertexArray();
        Tuple<string, string>[] GetEdgeArray();

        string[] GetSuccessorArray(string v);
        string[] GetPredecessorArray(string w);

        double GetVertexCount();
        double GetEdgeCount();        
    }
}
