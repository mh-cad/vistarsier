namespace NiftiView
{
    partial class Form1
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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.axialToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.coronalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sagittalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.pictureBoxA = new System.Windows.Forms.PictureBox();
            this.menuStripA = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuFileOpenA = new System.Windows.Forms.ToolStripMenuItem();
            this.pictureBoxB = new System.Windows.Forms.PictureBox();
            this.menuStripB = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuFileOpenB = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.lblcurrentPoint = new System.Windows.Forms.Label();
            this.lblEor = new System.Windows.Forms.Label();
            this.lblSor = new System.Windows.Forms.Label();
            this.eor = new System.Windows.Forms.NumericUpDown();
            this.sor = new System.Windows.Forms.NumericUpDown();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxA)).BeginInit();
            this.menuStripA.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxB)).BeginInit();
            this.menuStripB.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.eor)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.sor)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.viewToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.menuStrip1.Size = new System.Drawing.Size(800, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.axialToolStripMenuItem,
            this.coronalToolStripMenuItem,
            this.sagittalToolStripMenuItem});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.viewToolStripMenuItem.Text = "View";
            // 
            // axialToolStripMenuItem
            // 
            this.axialToolStripMenuItem.Name = "axialToolStripMenuItem";
            this.axialToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.axialToolStripMenuItem.Text = "Axial";
            this.axialToolStripMenuItem.Click += new System.EventHandler(this.axialToolStripMenuItem_Click);
            // 
            // coronalToolStripMenuItem
            // 
            this.coronalToolStripMenuItem.Name = "coronalToolStripMenuItem";
            this.coronalToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.coronalToolStripMenuItem.Text = "Coronal";
            this.coronalToolStripMenuItem.Click += new System.EventHandler(this.coronalToolStripMenuItem_Click);
            // 
            // sagittalToolStripMenuItem
            // 
            this.sagittalToolStripMenuItem.Name = "sagittalToolStripMenuItem";
            this.sagittalToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.sagittalToolStripMenuItem.Text = "Sagittal";
            this.sagittalToolStripMenuItem.Click += new System.EventHandler(this.sagittalToolStripMenuItem_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 24);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer3);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(800, 525);
            this.splitContainer1.SplitterDistance = 349;
            this.splitContainer1.TabIndex = 3;
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.pictureBoxA);
            this.splitContainer3.Panel1.Controls.Add(this.menuStripA);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.pictureBoxB);
            this.splitContainer3.Panel2.Controls.Add(this.menuStripB);
            this.splitContainer3.Size = new System.Drawing.Size(800, 349);
            this.splitContainer3.SplitterDistance = 387;
            this.splitContainer3.TabIndex = 0;
            // 
            // pictureBoxA
            // 
            this.pictureBoxA.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.pictureBoxA.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBoxA.Location = new System.Drawing.Point(0, 24);
            this.pictureBoxA.Name = "pictureBoxA";
            this.pictureBoxA.Size = new System.Drawing.Size(387, 325);
            this.pictureBoxA.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxA.TabIndex = 3;
            this.pictureBoxA.TabStop = false;
            // 
            // menuStripA
            // 
            this.menuStripA.BackColor = System.Drawing.Color.Black;
            this.menuStripA.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem1});
            this.menuStripA.Location = new System.Drawing.Point(0, 0);
            this.menuStripA.Name = "menuStripA";
            this.menuStripA.Size = new System.Drawing.Size(387, 24);
            this.menuStripA.TabIndex = 4;
            this.menuStripA.Text = "menuStrip2";
            // 
            // fileToolStripMenuItem1
            // 
            this.fileToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuFileOpenA});
            this.fileToolStripMenuItem1.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.fileToolStripMenuItem1.Name = "fileToolStripMenuItem1";
            this.fileToolStripMenuItem1.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem1.Text = "File";
            // 
            // mnuFileOpenA
            // 
            this.mnuFileOpenA.Name = "mnuFileOpenA";
            this.mnuFileOpenA.ShortcutKeyDisplayString = "O";
            this.mnuFileOpenA.Size = new System.Drawing.Size(128, 22);
            this.mnuFileOpenA.Text = "Open...";
            this.mnuFileOpenA.Click += new System.EventHandler(this.mnuFileOpenA_Click);
            // 
            // pictureBoxB
            // 
            this.pictureBoxB.BackColor = System.Drawing.Color.Black;
            this.pictureBoxB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBoxB.Location = new System.Drawing.Point(0, 24);
            this.pictureBoxB.Name = "pictureBoxB";
            this.pictureBoxB.Size = new System.Drawing.Size(409, 325);
            this.pictureBoxB.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxB.TabIndex = 0;
            this.pictureBoxB.TabStop = false;
            // 
            // menuStripB
            // 
            this.menuStripB.BackColor = System.Drawing.Color.Black;
            this.menuStripB.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem2});
            this.menuStripB.Location = new System.Drawing.Point(0, 0);
            this.menuStripB.Name = "menuStripB";
            this.menuStripB.Size = new System.Drawing.Size(409, 24);
            this.menuStripB.TabIndex = 1;
            this.menuStripB.Text = "menuStrip3";
            // 
            // fileToolStripMenuItem2
            // 
            this.fileToolStripMenuItem2.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuFileOpenB});
            this.fileToolStripMenuItem2.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.fileToolStripMenuItem2.Name = "fileToolStripMenuItem2";
            this.fileToolStripMenuItem2.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem2.Text = "File";
            // 
            // mnuFileOpenB
            // 
            this.mnuFileOpenB.Name = "mnuFileOpenB";
            this.mnuFileOpenB.ShortcutKeyDisplayString = "O";
            this.mnuFileOpenB.Size = new System.Drawing.Size(128, 22);
            this.mnuFileOpenB.Text = "Open...";
            this.mnuFileOpenB.Click += new System.EventHandler(this.mnuFileOpenB_Click);
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.chart1);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.lblcurrentPoint);
            this.splitContainer2.Panel2.Controls.Add(this.lblEor);
            this.splitContainer2.Panel2.Controls.Add(this.lblSor);
            this.splitContainer2.Panel2.Controls.Add(this.eor);
            this.splitContainer2.Panel2.Controls.Add(this.sor);
            this.splitContainer2.Size = new System.Drawing.Size(800, 172);
            this.splitContainer2.SplitterDistance = 589;
            this.splitContainer2.TabIndex = 0;
            // 
            // chart1
            // 
            this.chart1.BorderlineWidth = 0;
            chartArea1.AxisX.ScaleBreakStyle.Spacing = 0D;
            chartArea1.AxisX2.ScaleBreakStyle.Spacing = 0D;
            chartArea1.BorderWidth = 0;
            chartArea1.Name = "ChartArea1";
            this.chart1.ChartAreas.Add(chartArea1);
            this.chart1.Dock = System.Windows.Forms.DockStyle.Fill;
            legend1.Name = "Legend1";
            this.chart1.Legends.Add(legend1);
            this.chart1.Location = new System.Drawing.Point(0, 0);
            this.chart1.Name = "chart1";
            series1.ChartArea = "ChartArea1";
            series1.IsVisibleInLegend = false;
            series1.Legend = "Legend1";
            series1.MarkerBorderWidth = 0;
            series1.Name = "Series1";
            this.chart1.Series.Add(series1);
            this.chart1.Size = new System.Drawing.Size(589, 172);
            this.chart1.TabIndex = 1;
            this.chart1.Text = "chart1";
            // 
            // lblcurrentPoint
            // 
            this.lblcurrentPoint.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblcurrentPoint.AutoSize = true;
            this.lblcurrentPoint.Location = new System.Drawing.Point(9, 53);
            this.lblcurrentPoint.Name = "lblcurrentPoint";
            this.lblcurrentPoint.Size = new System.Drawing.Size(39, 13);
            this.lblcurrentPoint.TabIndex = 4;
            this.lblcurrentPoint.Text = "Output";
            // 
            // lblEor
            // 
            this.lblEor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblEor.AutoSize = true;
            this.lblEor.Location = new System.Drawing.Point(11, 32);
            this.lblEor.Name = "lblEor";
            this.lblEor.Size = new System.Drawing.Size(67, 13);
            this.lblEor.TabIndex = 3;
            this.lblEor.Text = "end of range";
            // 
            // lblSor
            // 
            this.lblSor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSor.AutoSize = true;
            this.lblSor.Location = new System.Drawing.Point(9, 6);
            this.lblSor.Name = "lblSor";
            this.lblSor.Size = new System.Drawing.Size(69, 13);
            this.lblSor.TabIndex = 2;
            this.lblSor.Text = "start of range";
            // 
            // eor
            // 
            this.eor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.eor.Location = new System.Drawing.Point(84, 30);
            this.eor.Name = "eor";
            this.eor.Size = new System.Drawing.Size(120, 20);
            this.eor.TabIndex = 1;
            // 
            // sor
            // 
            this.sor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.sor.Location = new System.Drawing.Point(84, 4);
            this.sor.Name = "sor";
            this.sor.Size = new System.Drawing.Size(120, 20);
            this.sor.TabIndex = 0;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 549);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.menuStrip1);
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "DrtiNfti";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel1.PerformLayout();
            this.splitContainer3.Panel2.ResumeLayout(false);
            this.splitContainer3.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxA)).EndInit();
            this.menuStripA.ResumeLayout(false);
            this.menuStripA.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxB)).EndInit();
            this.menuStripB.ResumeLayout(false);
            this.menuStripB.PerformLayout();
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.eor)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.sor)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart1;
        private System.Windows.Forms.Label lblEor;
        private System.Windows.Forms.Label lblSor;
        private System.Windows.Forms.NumericUpDown eor;
        private System.Windows.Forms.NumericUpDown sor;
        private System.Windows.Forms.Label lblcurrentPoint;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem axialToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem coronalToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sagittalToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.PictureBox pictureBoxA;
        private System.Windows.Forms.MenuStrip menuStripA;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem1;
        private System.Windows.Forms.PictureBox pictureBoxB;
        private System.Windows.Forms.MenuStrip menuStripB;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem mnuFileOpenA;
        private System.Windows.Forms.ToolStripMenuItem mnuFileOpenB;
    }
}

