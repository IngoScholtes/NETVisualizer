using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NETVisualizer;

namespace NETVisualizer.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            SimpleNetwork net = new SimpleNetwork();
            Renderer.Start(net, new NETVisualizer.Layouts.FruchtermanReingold.FRLayout(10));
        }
    }
}
