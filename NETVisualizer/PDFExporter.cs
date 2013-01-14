using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using OpenTK;

namespace NETVisualizer
{

    /// <summary>
    /// This class can be used to export network visualizations to PDF files
    /// </summary>
    public class PDFExporter
    {

        private static float x_offset = 0;
        private static float y_offset = 0;

        /// <summary>
        /// Creates a PDF from a network visualization
        /// </summary>
        /// <param name='path'>
        /// The path where the PDF shall be saved to
        /// </param>
        /// <param name='n'>
        /// The network that shall be exported
        /// </param>
        /// <param name='presentationSettings'>
        /// The PresentationSettings that define the zooming, panning, default edge and vertex colors, etc. 
        /// </param>
        /// <param name='layout'>
        /// The layour provider that defines vertex positions
        /// </param>
        /// <param name='customColors'>
        /// Custom colors that change colors of vertices and edges individually
        /// </param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void CreatePDF(string path, IRenderableNet n, LayoutProvider layout, NetworkColorizer colorizer = null)
        {
            PdfSharp.Pdf.PdfDocument doc = new PdfDocument();
            doc.Info.Title = "Network";
            doc.Info.Subject = "Created by NETVisualizer";

            PdfPage page = doc.AddPage();

            // Apply the proper scaling
            Vector3 origin = Renderer.ScreenToWorld(new Vector3(0, 0, 0));
            Vector3 bottomright = Renderer.ScreenToWorld(new Vector3(Renderer.RenderWidth, Renderer.RenderHeight, 0));
            x_offset = origin.X;
            y_offset = origin.Y;
            page.Width = bottomright.X - origin.X;
            page.Height = bottomright.Y - origin.Y;


            if (colorizer != null)
                // Draw the network to the xgraphics object
                Draw(XGraphics.FromPdfPage(page), n, layout, colorizer);
            else
                Draw(XGraphics.FromPdfPage(page), n, layout, new NetworkColorizer());

            // Save the s_document...
            doc.Save(path);
        }

        private static void Draw(XGraphics g, IRenderableNet n, LayoutProvider layout, NetworkColorizer colorizer)
        {
            lock (n)
            {
                if (g == null)
                    return;
                g.SmoothingMode = PdfSharp.Drawing.XSmoothingMode.HighQuality;
                g.Clear(Color.White);
                foreach (var e in n.GetEdgeArray())
                    DrawEdge(g, e, layout, colorizer);
                foreach (string v in n.GetVertexArray())
                    DrawVertex(g, v, layout, colorizer);
            }

        }

        private static void DrawVertex(XGraphics g, string v, LayoutProvider layout, NetworkColorizer colorizer)
        {
            OpenTK.Vector3 p = layout.GetPositionOfNode(v);

            double size = Renderer.ComputeNodeSize(v);

            if (!double.IsNaN(p.X) &&
               !double.IsNaN(p.Y) &&
               !double.IsNaN(p.Z))
                g.DrawEllipse(new SolidBrush(colorizer[v]), p.X - size / 2d - x_offset, p.Y - size / 2d - y_offset, size, size);
        }

        private static void DrawEdge(XGraphics g, Tuple<string, string> e, LayoutProvider layout, NetworkColorizer colorizer)
        {
            string v = e.Item1;
            string w = e.Item2;
            float width = Renderer.ComputeEdgeThickness(e);
            XColor color = colorizer[e];

            List<Vector2> points = new List<Vector2>();

            // Draw curved edge
            if (Renderer.CurvedEdges)
            {
                // The two end points of the arc
                Vector2 pos_v = new Vector2(Renderer.Layout.GetPositionOfNode(v).X,Renderer.Layout.GetPositionOfNode(v).Y);
                Vector2 pos_w = new Vector2(Renderer.Layout.GetPositionOfNode(w).X, Renderer.Layout.GetPositionOfNode(w).Y);

                if (pos_v == pos_w)
                    return;

                // The vector vw representing one point of an equilateral triangle (v,w,x) with x being the center of the circle 
                Vector2 vw = Vector2.Subtract(pos_w, pos_v);

                // In an equilateral triangle, the two angles adjacent to vw are alpha
                float alpha = ((float)Math.PI - Renderer.EdgeCurvatureGamma) / 2f;

                // Compute the radius based on the sine rule ... 
                float radius = Math.Abs((float)(vw.Length * Math.Sin(alpha) / Math.Sin(Renderer.EdgeCurvatureGamma)));

                // Compute center point
                Vector2 center = vw;
                center.Normalize();
                float rotated_x = (float)(center.X * Math.Cos(alpha) - center.Y * Math.Sin(alpha));
                float rotated_y = (float)(center.X * Math.Sin(alpha) + center.Y * Math.Cos(alpha));
                center.X = rotated_x;
                center.Y = rotated_y;
                Vector2 pos_center = Vector2.Add(pos_v, Vector2.Multiply(center, radius));

                // The vector xv that will be rotatd in every step ... 
                Vector2 xv = Vector2.Subtract(pos_v, pos_center);

                // The point on the circle to use for the next line segment
                Vector2 circle_point = xv;                

                // Initiate the drawing ... 

                // Gradually rotate the vector circle_point to get the points on the circle to connect to a curved edge
                if (Renderer.orientation(pos_center, pos_v, pos_w) > 0)
                    for (int i = 0; i <= Renderer.EdgeCurvatureSegments; i++)
                    {
                        points.Add(Vector2.Add(pos_center, circle_point));
                        float new_x = (float)(circle_point.X * Math.Cos(Renderer.EdgeCurvatureGamma / Renderer.EdgeCurvatureSegments) - circle_point.Y * Math.Sin(Renderer.EdgeCurvatureGamma / Renderer.EdgeCurvatureSegments));
                        float new_y = (float)(circle_point.X * Math.Sin(Renderer.EdgeCurvatureGamma / Renderer.EdgeCurvatureSegments) + circle_point.Y * Math.Cos(Renderer.EdgeCurvatureGamma / Renderer.EdgeCurvatureSegments));
                        circle_point = new Vector2(new_x, new_y);
                    }
                else
                    for (int i = 0; i <= Renderer.EdgeCurvatureSegments; i++)
                    {
                        points.Add(Vector2.Add(pos_center, circle_point));
                        float new_x = (float)(circle_point.X * Math.Cos(-Renderer.EdgeCurvatureGamma / Renderer.EdgeCurvatureSegments) - circle_point.Y * Math.Sin(-Renderer.EdgeCurvatureGamma / Renderer.EdgeCurvatureSegments));
                        float new_y = (float)(circle_point.X * Math.Sin(-Renderer.EdgeCurvatureGamma / Renderer.EdgeCurvatureSegments) + circle_point.Y * Math.Cos(-Renderer.EdgeCurvatureGamma / Renderer.EdgeCurvatureSegments));
                        circle_point = new Vector2(new_x, new_y);
                    }
            }
            // Draw straight edge as a simple line between the vertices
            else
            {               
                points.Add(new Vector2(Renderer.Layout.GetPositionOfNode(v).X, Renderer.Layout.GetPositionOfNode(v).Y));
                points.Add(new Vector2(Renderer.Layout.GetPositionOfNode(w).X, Renderer.Layout.GetPositionOfNode(w).Y));
            }

            for (int i = 1; i < points.Count; i++)
                g.DrawLine(new XPen(color, width), points[i - 1].X-x_offset, points[i - 1].Y-y_offset, points[i].X-x_offset, points[i].Y-y_offset);
        }
    }
}
