namespace NETVisualizer.TemporalNets
{
    partial class VisualizationController
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.curvatureBar = new System.Windows.Forms.TrackBar();
            this.label2 = new System.Windows.Forms.Label();
            this.timeBar = new System.Windows.Forms.TrackBar();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.playButton = new System.Windows.Forms.Button();
            this.simulationDelay = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.exportPDFBtn = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.curvatureBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.timeBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.simulationDelay)).BeginInit();
            this.SuspendLayout();
            // 
            // curvatureBar
            // 
            this.curvatureBar.Location = new System.Drawing.Point(12, 47);
            this.curvatureBar.Maximum = 3141569;
            this.curvatureBar.Minimum = 100;
            this.curvatureBar.Name = "curvatureBar";
            this.curvatureBar.Size = new System.Drawing.Size(360, 45);
            this.curvatureBar.TabIndex = 2;
            this.curvatureBar.TickFrequency = 200000;
            this.curvatureBar.Value = 100;
            this.curvatureBar.ValueChanged += new System.EventHandler(this.trackBar2_ValueChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(81, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Edge Curvature";
            // 
            // timeBar
            // 
            this.timeBar.Cursor = System.Windows.Forms.Cursors.Default;
            this.timeBar.Enabled = false;
            this.timeBar.Location = new System.Drawing.Point(51, 188);
            this.timeBar.Name = "timeBar";
            this.timeBar.Size = new System.Drawing.Size(326, 45);
            this.timeBar.TabIndex = 4;
            this.timeBar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.timeBar.ValueChanged += new System.EventHandler(this.timeBar_ValueChanged);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Checked = true;
            this.checkBox1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox1.Location = new System.Drawing.Point(7, 156);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(168, 17);
            this.checkBox1.TabIndex = 5;
            this.checkBox1.Text = "Visualize Aggregated Network";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckStateChanged += new System.EventHandler(this.checkBox1_CheckStateChanged);
            // 
            // playButton
            // 
            this.playButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.playButton.Enabled = false;
            this.playButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.playButton.Location = new System.Drawing.Point(7, 188);
            this.playButton.Name = "playButton";
            this.playButton.Size = new System.Drawing.Size(38, 36);
            this.playButton.TabIndex = 6;
            this.playButton.Text = "Play";
            this.playButton.UseVisualStyleBackColor = true;
            this.playButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // simulationDelay
            // 
            this.simulationDelay.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.simulationDelay.Enabled = false;
            this.simulationDelay.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.simulationDelay.Location = new System.Drawing.Point(7, 230);
            this.simulationDelay.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.simulationDelay.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.simulationDelay.Name = "simulationDelay";
            this.simulationDelay.Size = new System.Drawing.Size(55, 20);
            this.simulationDelay.TabIndex = 7;
            this.simulationDelay.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(68, 230);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(83, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "ms per time step";
            // 
            // exportPDFBtn
            // 
            this.exportPDFBtn.Location = new System.Drawing.Point(12, 88);
            this.exportPDFBtn.Name = "exportPDFBtn";
            this.exportPDFBtn.Size = new System.Drawing.Size(75, 23);
            this.exportPDFBtn.TabIndex = 9;
            this.exportPDFBtn.Text = "Export PDF";
            this.exportPDFBtn.UseVisualStyleBackColor = true;
            this.exportPDFBtn.Click += new System.EventHandler(this.exportPDFBtn_Click);
            // 
            // VisualizationController
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(379, 262);
            this.Controls.Add(this.exportPDFBtn);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.simulationDelay);
            this.Controls.Add(this.playButton);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.timeBar);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.curvatureBar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "VisualizationController";
            this.Text = "Visualization Parameters";
            ((System.ComponentModel.ISupportInitialize)(this.curvatureBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.timeBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.simulationDelay)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TrackBar curvatureBar;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TrackBar timeBar;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.Button playButton;
        private System.Windows.Forms.NumericUpDown simulationDelay;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.Button exportPDFBtn;
    }
}