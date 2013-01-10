using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TemporalNetworks;

namespace NETVisualizer.TemporalNets
{
    public class RenderableTempNet : NETVisualizer.IRenderableNet
    {
        public TemporalNetwork Network;

        public bool RenderAggregate = true;
        public int CurrentTime = 0;

        public RenderableTempNet(TemporalNetwork temp_net)
        {
            Network = temp_net;
            CurrentTime = 0;
        }

        public void MoveTime(int time=-1)
        {
            if (time < 0 || time >= Network.Keys.Count)
            {
                CurrentTime++;
                if (CurrentTime > Network.Keys.Count)
                    CurrentTime = 0;
            }
            else
                CurrentTime = time;            
        }


        public Tuple<string, string>[] GetEdgeArray()
        {
            if (RenderAggregate)
                return Network.AggregateNetwork.Edges.ToArray();
            else
                return Network[Network.Keys.ElementAt(CurrentTime)].ToArray();
        }        

        public int GetEdgeCount()
        {
            if (RenderAggregate)
                return Network.AggregateNetwork.EdgeCount;
            else
                return Network[Network.Keys.ElementAt(CurrentTime)].Count;
        }

        public string[] GetPredecessorArray(string w)
        {
            return Network.AggregateNetwork.GetPredecessors(w).ToArray();
        }

        public string[] GetSuccessorArray(string v)
        {
            return Network.AggregateNetwork.GetSuccessors(v).ToArray();
        }

        public string[] GetVertexArray()
        {
            return Network.AggregateNetwork.Vertices.ToArray();
        }

        public int GetVertexCount()
        {
            return Network.AggregateNetwork.VertexCount;
        }

        public event NETVisualizer.VertexChangedHandler OnVertexChanged;
    }
}
