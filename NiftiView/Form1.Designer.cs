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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea4 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend4 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series4 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.axialToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.coronalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sagittalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.lblMouseA = new System.Windows.Forms.Label();
            this.lblAFileName = new System.Windows.Forms.Label();
            this.pictureBoxA = new System.Windows.Forms.PictureBox();
            this.menuStripA = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuFileOpenA = new System.Windows.Forms.ToolStripMenuItem();
            this.processToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aNTSN4BiasFieldCorrectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.brainSuiteBrainExtractionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cMTKRegistrationResliceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.subtractBToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lblMouseB = new System.Windows.Forms.Label();
            this.lblFileNameB = new System.Windows.Forms.Label();
            this.pictureBoxB = new System.Windows.Forms.PictureBox();
            this.menuStripB = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuFileOpenB = new System.Windows.Forms.ToolStripMenuItem();
            this.processToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.aNTSN4BiasFieldCorrectionToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.brainSuiteBrainExtractionToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.cMTKRegistrationResliceToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.lblcurrentPoint = new System.Windows.Forms.Label();
            this.lblEor = new System.Windows.Forms.Label();
            this.lblSor = new System.Windows.Forms.Label();
            this.eor = new System.Windows.Forms.NumericUpDown();
            this.sor = new System.Windows.Forms.NumericUpDown();
            this.compareBToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.normalizeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.compareDecreaseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
            this.splitContainer3.Panel1.Controls.Add(this.lblMouseA);
            this.splitContainer3.Panel1.Controls.Add(this.lblAFileName);
            this.splitContainer3.Panel1.Controls.Add(this.pictureBoxA);
            this.splitContainer3.Panel1.Controls.Add(this.menuStripA);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.lblMouseB);
            this.splitContainer3.Panel2.Controls.Add(this.lblFileNameB);
            this.splitContainer3.Panel2.Controls.Add(this.pictureBoxB);
            this.splitContainer3.Panel2.Controls.Add(this.menuStripB);
            this.splitContainer3.Size = new System.Drawing.Size(800, 349);
            this.splitContainer3.SplitterDistance = 387;
            this.splitContainer3.TabIndex = 0;
            // 
            // lblMouseA
            // 
            this.lblMouseA.AutoSize = true;
            this.lblMouseA.BackColor = System.Drawing.Color.Transparent;
            this.lblMouseA.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblMouseA.Location = new System.Drawing.Point(3, 24);
            this.lblMouseA.Name = "lblMouseA";
            this.lblMouseA.Size = new System.Drawing.Size(56, 13);
            this.lblMouseA.TabIndex = 6;
            this.lblMouseA.Text = "lblMouseA";
            this.lblMouseA.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblAFileName
            // 
            this.lblAFileName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblAFileName.AutoSize = true;
            this.lblAFileName.BackColor = System.Drawing.Color.Black;
            this.lblAFileName.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblAFileName.Location = new System.Drawing.Point(3, 336);
            this.lblAFileName.Name = "lblAFileName";
            this.lblAFileName.Size = new System.Drawing.Size(30, 13);
            this.lblAFileName.TabIndex = 5;
            this.lblAFileName.Text = "file A";
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
            this.fileToolStripMenuItem1,
            this.processToolStripMenuItem});
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
            // processToolStripMenuItem
            // 
            this.processToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aNTSN4BiasFieldCorrectionToolStripMenuItem,
            this.brainSuiteBrainExtractionToolStripMenuItem,
            this.cMTKRegistrationResliceToolStripMenuItem,
            this.subtractBToolStripMenuItem,
            this.normalizeToolStripMenuItem,
            this.compareBToolStripMenuItem,
            this.compareDecreaseToolStripMenuItem});
            this.processToolStripMenuItem.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.processToolStripMenuItem.Name = "processToolStripMenuItem";
            this.processToolStripMenuItem.Size = new System.Drawing.Size(59, 20);
            this.processToolStripMenuItem.Text = "Process";
            // 
            // aNTSN4BiasFieldCorrectionToolStripMenuItem
            // 
            this.aNTSN4BiasFieldCorrectionToolStripMenuItem.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.aNTSN4BiasFieldCorrectionToolStripMenuItem.Name = "aNTSN4BiasFieldCorrectionToolStripMenuItem";
            this.aNTSN4BiasFieldCorrectionToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.aNTSN4BiasFieldCorrectionToolStripMenuItem.Text = "(ANTS) N4BiasFieldCorrection";
            this.aNTSN4BiasFieldCorrectionToolStripMenuItem.Click += new System.EventHandler(this.aNTSN4BiasFieldCorrectionToolStripMenuItem_Click);
            // 
            // brainSuiteBrainExtractionToolStripMenuItem
            // 
            this.brainSuiteBrainExtractionToolStripMenuItem.Name = "brainSuiteBrainExtractionToolStripMenuItem";
            this.brainSuiteBrainExtractionToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.brainSuiteBrainExtractionToolStripMenuItem.Text = "(BrainSuite) BrainExtraction";
            this.brainSuiteBrainExtractionToolStripMenuItem.Click += new System.EventHandler(this.brainSuiteBrainExtractionToolStripMenuItem_Click);
            // 
            // cMTKRegistrationResliceToolStripMenuItem
            // 
            this.cMTKRegistrationResliceToolStripMenuItem.Name = "cMTKRegistrationResliceToolStripMenuItem";
            this.cMTKRegistrationResliceToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.cMTKRegistrationResliceToolStripMenuItem.Text = "(CMTK) Registration + Reslice";
            this.cMTKRegistrationResliceToolStripMenuItem.Click += new System.EventHandler(this.cMTKRegistrationResliceToolStripMenuItem_Click);
            // 
            // subtractBToolStripMenuItem
            // 
            this.subtractBToolStripMenuItem.Name = "subtractBToolStripMenuItem";
            this.subtractBToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.subtractBToolStripMenuItem.Text = "Subtract B";
            this.subtractBToolStripMenuItem.Click += new System.EventHandler(this.subtractBToolStripMenuItem_Click);
            // 
            // lblMouseB
            // 
            this.lblMouseB.AutoSize = true;
            this.lblMouseB.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblMouseB.Location = new System.Drawing.Point(3, 24);
            this.lblMouseB.Name = "lblMouseB";
            this.lblMouseB.Size = new System.Drawing.Size(45, 13);
            this.lblMouseB.TabIndex = 3;
            this.lblMouseB.Text = "mouseB";
            this.lblMouseB.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblFileNameB
            // 
            this.lblFileNameB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblFileNameB.AutoSize = true;
            this.lblFileNameB.BackColor = System.Drawing.Color.Black;
            this.lblFileNameB.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblFileNameB.Location = new System.Drawing.Point(2, 336);
            this.lblFileNameB.Name = "lblFileNameB";
            this.lblFileNameB.Size = new System.Drawing.Size(30, 13);
            this.lblFileNameB.TabIndex = 2;
            this.lblFileNameB.Text = "file B";
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
            this.fileToolStripMenuItem2,
            this.processToolStripMenuItem1});
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
            // processToolStripMenuItem1
            // 
            this.processToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aNTSN4BiasFieldCorrectionToolStripMenuItem1,
            this.brainSuiteBrainExtractionToolStripMenuItem1,
            this.cMTKRegistrationResliceToolStripMenuItem1});
            this.processToolStripMenuItem1.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.processToolStripMenuItem1.Name = "processToolStripMenuItem1";
            this.processToolStripMenuItem1.Size = new System.Drawing.Size(59, 20);
            this.processToolStripMenuItem1.Text = "Process";
            // 
            // aNTSN4BiasFieldCorrectionToolStripMenuItem1
            // 
            this.aNTSN4BiasFieldCorrectionToolStripMenuItem1.Name = "aNTSN4BiasFieldCorrectionToolStripMenuItem1";
            this.aNTSN4BiasFieldCorrectionToolStripMenuItem1.Size = new System.Drawing.Size(232, 22);
            this.aNTSN4BiasFieldCorrectionToolStripMenuItem1.Text = "(ANTS) N4BiasFieldCorrection";
            this.aNTSN4BiasFieldCorrectionToolStripMenuItem1.Click += new System.EventHandler(this.aNTSN4BiasFieldCorrectionToolStripMenuItem1_Click);
            // 
            // brainSuiteBrainExtractionToolStripMenuItem1
            // 
            this.brainSuiteBrainExtractionToolStripMenuItem1.Name = "brainSuiteBrainExtractionToolStripMenuItem1";
            this.brainSuiteBrainExtractionToolStripMenuItem1.Size = new System.Drawing.Size(232, 22);
            this.brainSuiteBrainExtractionToolStripMenuItem1.Text = "(BrainSuite) BrainExtraction";
            this.brainSuiteBrainExtractionToolStripMenuItem1.Click += new System.EventHandler(this.brainSuiteBrainExtractionToolStripMenuItem1_Click);
            // 
            // cMTKRegistrationResliceToolStripMenuItem1
            // 
            this.cMTKRegistrationResliceToolStripMenuItem1.Name = "cMTKRegistrationResliceToolStripMenuItem1";
            this.cMTKRegistrationResliceToolStripMenuItem1.Size = new System.Drawing.Size(232, 22);
            this.cMTKRegistrationResliceToolStripMenuItem1.Text = "(CMTK) Registration + Reslice";
            this.cMTKRegistrationResliceToolStripMenuItem1.Click += new System.EventHandler(this.cMTKRegistrationResliceToolStripMenuItem1_Click);
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
            chartArea4.AxisX.ScaleBreakStyle.Spacing = 0D;
            chartArea4.AxisX2.ScaleBreakStyle.Spacing = 0D;
            chartArea4.BorderWidth = 0;
            chartArea4.Name = "ChartArea1";
            this.chart1.ChartAreas.Add(chartArea4);
            this.chart1.Dock = System.Windows.Forms.DockStyle.Fill;
            legend4.Name = "Legend1";
            this.chart1.Legends.Add(legend4);
            this.chart1.Location = new System.Drawing.Point(0, 0);
            this.chart1.Name = "chart1";
            series4.ChartArea = "ChartArea1";
            series4.IsVisibleInLegend = false;
            series4.Legend = "Legend1";
            series4.MarkerBorderWidth = 0;
            series4.Name = "Series1";
            this.chart1.Series.Add(series4);
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
            // compareBToolStripMenuItem
            // 
            this.compareBToolStripMenuItem.ForeColor = System.Drawing.SystemColors.ControlText;
            this.compareBToolStripMenuItem.Name = "compareBToolStripMenuItem";
            this.compareBToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.compareBToolStripMenuItem.Text = "Compare Increase";
            this.compareBToolStripMenuItem.Click += new System.EventHandler(this.compareBToolStripMenuItem_Click);
            // 
            // normalizeToolStripMenuItem
            // 
            this.normalizeToolStripMenuItem.Name = "normalizeToolStripMenuItem";
            this.normalizeToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.normalizeToolStripMenuItem.Text = "Normalize";
            this.normalizeToolStripMenuItem.Click += new System.EventHandler(this.normalizeToolStripMenuItem_Click);
            // 
            // compareDecreaseToolStripMenuItem
            // 
            this.compareDecreaseToolStripMenuItem.Name = "compareDecreaseToolStripMenuItem";
            this.compareDecreaseToolStripMenuItem.Size = new System.Drawing.Size(232, 22);
            this.compareDecreaseToolStripMenuItem.Text = "Compare Decrease";
            this.compareDecreaseToolStripMenuItem.Click += new System.EventHandler(this.compareDecreaseToolStripMenuItem_Click);
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
        private System.Windows.Forms.Label lblAFileName;
        private System.Windows.Forms.Label lblFileNameB;
        private System.Windows.Forms.Label lblMouseA;
        private System.Windows.Forms.Label lblMouseB;
        private System.Windows.Forms.ToolStripMenuItem processToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aNTSN4BiasFieldCorrectionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem processToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem aNTSN4BiasFieldCorrectionToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem brainSuiteBrainExtractionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cMTKRegistrationResliceToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem brainSuiteBrainExtractionToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem cMTKRegistrationResliceToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem subtractBToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem compareBToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem normalizeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem compareDecreaseToolStripMenuItem;
    }
}

