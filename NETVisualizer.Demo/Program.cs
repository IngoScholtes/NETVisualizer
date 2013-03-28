using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using NETVisualizer;

namespace NETVisualizer.Demo
{
    /// <summary>
    /// A simple demonstration of how to use the network visualizer in your code ...
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            // Create a random network
            SimpleNetwork net = SimpleNetwork.ReadFromEdgeList("demo.edges");

            // We use a custom coloring
            NetworkColorizer colors = new NetworkColorizer();
            colors.DefaultBackgroundColor = Color.Black;
            colors.DefaultEdgeColor = Color.WhiteSmoke;
            colors.DefaultVertexColor = Color.SteelBlue;

            // Let's use curved edges instead of the default straight ones
            Renderer.CurvedEdges = true;

            // Fire up the visualizer window
            Renderer.Start(net, new NETVisualizer.Layouts.FruchtermanReingold.FRLayout(10), colors);

            // Trigger the layout
            Renderer.Layout.DoLayoutAsync();            

            // The rendering and layouting is done asynchronously in parallel, 
            // so you can modify the network while the visualization and the layout continues
            Console.Write("Press ANY KEY to add another edge");
            Console.ReadKey();
            net.AddEdge("a", "b");
            net.AddEdge("b", "c");

            // Trigger the layout again. Only changed nodes will be relayouted ... 
            Renderer.Layout.DoLayoutAsync();

            Console.WriteLine("Waiting for rendering window to be closed ...");           
        }
    }
}
