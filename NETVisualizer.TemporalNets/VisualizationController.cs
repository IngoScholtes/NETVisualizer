using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using NETVisualizer;
using System.Reflection;

namespace NETVisualizer.TemporalNets
{
    public partial class VisualizationController : Form
    {
        RenderableTempNet temp_net;
        Timer timer;

        public VisualizationController(RenderableTempNet network)
        {
            InitializeComponent();
            temp_net = network;
            timer = new Timer();
            timer.Tick += timer_Tick;
        }

        void timer_Tick(object sender, EventArgs e)
        {
            temp_net.MoveTime();
            timeBar.Value = temp_net.CurrentTime;
        }

        private void trackBar2_ValueChanged(object sender, EventArgs e)
        {
            Renderer.EdgeCurvatureGamma = (float)curvatureBar.Value / 1000000f;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            timer.Interval = (int)simulationDelay.Value;
            if (timer.Enabled)
            {
                playButton.Text= "Play";
                timer.Stop();
            }
            else
            {
                playButton.Text = "Stop";
                timer.Start();
            }
        }

        private void checkBox1_CheckStateChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                temp_net.RenderAggregate = true;
                timeBar.Enabled = false;
                simulationDelay.Enabled = false;
                playButton.Enabled = false;
            }
            else
            {
                temp_net.RenderAggregate = false;
                timeBar.Minimum = 0;
                timeBar.Maximum = temp_net.Network.Keys.Count;
                timeBar.TickFrequency = 1;
                timeBar.Enabled = true;
                simulationDelay.Enabled = true;
                playButton.Enabled = true;
            }
        }

        private void timeBar_ValueChanged(object sender, EventArgs e)
        {
            temp_net.MoveTime(timeBar.Value);
        }

        private void exportPDFBtn_Click(object sender, EventArgs e)
        {
            saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "PDF|*.pdf";
            if(saveFileDialog1.ShowDialog()==DialogResult.OK)
            {
                PDFExporter.CreatePDF(saveFileDialog1.FileName, temp_net, Renderer.Layout);
            }
        }
    }
}
