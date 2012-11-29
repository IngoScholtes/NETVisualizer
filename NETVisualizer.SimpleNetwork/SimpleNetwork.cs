using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NETVisualizer
{
    public class SimpleNetwork : IRenderableNet
    {
        public string[] GetVertexArray()
        {
            throw new NotImplementedException();
        }

        public Tuple<string, string>[] GetEdgeArray()
        {
            throw new NotImplementedException();
        }

        public string[] GetSuccessorArray(string v)
        {
            throw new NotImplementedException();
        }

        public string[] GetPredecessorArray(string w)
        {
            throw new NotImplementedException();
        }

        public double GetVertexCount()
        {
            throw new NotImplementedException();
        }

        public double GetEdgeCount()
        {
            throw new NotImplementedException();
        }
    }
}
