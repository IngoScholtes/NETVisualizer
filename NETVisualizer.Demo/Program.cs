using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            SimpleNetwork net = SimpleNetwork.CreateRandomNetwork(300, 1000);

            // Fire up the visualizer window
            Renderer.Start(net, new NETVisualizer.Layouts.FruchtermanReingold.FRLayout(10));

            // Trigger the layout
            Renderer.Layout.DoLayoutAsync();

            // The rendering and layouting is done asynchronously in parallel, 
            // so you can modify the network while the visualization continues
            Console.Write("Press ANY KEY to add another edge");
            Console.ReadKey();
            net.AddEdge("a", "5");
            net.AddEdge("a", "7");

            // Trigger the layout again. Only changed nodes will be relayouted ... 
            Renderer.Layout.DoLayoutAsync();

            Console.WriteLine("Waiting for rendering window to be closed ...");
        }
    }
}
