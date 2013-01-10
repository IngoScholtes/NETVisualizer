using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NETVisualizer.Layouts.FruchtermanReingold;
using System.Windows.Forms;
using TemporalNetworks;
using System.Drawing;

namespace NETVisualizer.TemporalNets
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("\nUsage: Visualize [network-file] [iterations=50]\n");
                return;
            }

            string network_file = args[0];
            int iterations = args.Length >= 2 ? int.Parse(args[1]) : 50;

            // Load the temporal and aggregate network
            Console.Write("Loading temporal network ...");
            TemporalNetwork net = TemporalNetwork.ReadFromFile(network_file);
            Console.WriteLine("done");
            Console.Write("Computing aggregate network ...");
            WeightedNetwork agg = net.AggregateNetwork;
            Console.WriteLine("done");

            // Change the colors
            NetworkColorizer colors = new NetworkColorizer();
            colors.DefaultBackgroundColor = Color.White;
            colors.DefaultVertexColor = Color.DarkBlue;
            colors.DefaultEdgeColor = Color.Black;

            Renderer.CurvedEdges = true;

            RenderableTempNet temp_net = new RenderableTempNet(net);

            // Start rendering ...
            Renderer.Start(temp_net, new FRLayout(iterations), colors);


            // Asynchronously compute the layout ...
            Renderer.Layout.DoLayout();

            Application.Run(new VisualizationController(temp_net));

        }
    }
}
