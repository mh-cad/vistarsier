namespace CAPI.Desktop
{
    partial class FormMain
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
            this.TxtFilePath = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.CbRemoveCarerDetails = new System.Windows.Forms.CheckBox();
            this.CbSiteRemoved = new System.Windows.Forms.CheckBox();
            this.RbNewPatient = new System.Windows.Forms.RadioButton();
            this.RbNewStudy = new System.Windows.Forms.RadioButton();
            this.RbNewSeries = new System.Windows.Forms.RadioButton();
            this.RbNewImage = new System.Windows.Forms.RadioButton();
            this.BtnSend = new System.Windows.Forms.Button();
            this.CbPacsList = new System.Windows.Forms.ComboBox();
            this.CbAnonymize = new System.Windows.Forms.CheckBox();
            this.LblFilePath = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.FdLoadDicomFile = new System.Windows.Forms.OpenFileDialog();
            this.BtnBrowseDicomFile = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // TxtFilePath
            // 
            this.TxtFilePath.Font = new System.Drawing.Font("Verdana", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TxtFilePath.Location = new System.Drawing.Point(30, 63);
            this.TxtFilePath.Name = "TxtFilePath";
            this.TxtFilePath.Size = new System.Drawing.Size(233, 24);
            this.TxtFilePath.TabIndex = 0;
            this.TxtFilePath.Text = "C:\\temp\\test\\00000010";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.CbAnonymize);
            this.panel1.Controls.Add(this.CbRemoveCarerDetails);
            this.panel1.Controls.Add(this.CbSiteRemoved);
            this.panel1.Controls.Add(this.RbNewPatient);
            this.panel1.Controls.Add(this.RbNewStudy);
            this.panel1.Controls.Add(this.RbNewSeries);
            this.panel1.Controls.Add(this.RbNewImage);
            this.panel1.Font = new System.Drawing.Font("Marlett", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.panel1.Location = new System.Drawing.Point(16, 125);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(248, 255);
            this.panel1.TabIndex = 1;
            // 
            // CbRemoveCarerDetails
            // 
            this.CbRemoveCarerDetails.AutoSize = true;
            this.CbRemoveCarerDetails.Location = new System.Drawing.Point(16, 46);
            this.CbRemoveCarerDetails.Name = "CbRemoveCarerDetails";
            this.CbRemoveCarerDetails.Size = new System.Drawing.Size(192, 19);
            this.CbRemoveCarerDetails.TabIndex = 6;
            this.CbRemoveCarerDetails.Text = "Remove Care Provider Details";
            this.CbRemoveCarerDetails.UseVisualStyleBackColor = true;
            // 
            // CbSiteRemoved
            // 
            this.CbSiteRemoved.AutoSize = true;
            this.CbSiteRemoved.Location = new System.Drawing.Point(16, 14);
            this.CbSiteRemoved.Name = "CbSiteRemoved";
            this.CbSiteRemoved.Size = new System.Drawing.Size(138, 19);
            this.CbSiteRemoved.TabIndex = 5;
            this.CbSiteRemoved.Text = "Remove Site Details";
            this.CbSiteRemoved.UseVisualStyleBackColor = true;
            // 
            // RbNewPatient
            // 
            this.RbNewPatient.AutoSize = true;
            this.RbNewPatient.Location = new System.Drawing.Point(16, 118);
            this.RbNewPatient.Name = "RbNewPatient";
            this.RbNewPatient.Size = new System.Drawing.Size(91, 19);
            this.RbNewPatient.TabIndex = 3;
            this.RbNewPatient.TabStop = true;
            this.RbNewPatient.Text = "New Patient";
            this.RbNewPatient.UseVisualStyleBackColor = true;
            // 
            // RbNewStudy
            // 
            this.RbNewStudy.AutoSize = true;
            this.RbNewStudy.Location = new System.Drawing.Point(16, 155);
            this.RbNewStudy.Name = "RbNewStudy";
            this.RbNewStudy.Size = new System.Drawing.Size(83, 19);
            this.RbNewStudy.TabIndex = 2;
            this.RbNewStudy.TabStop = true;
            this.RbNewStudy.Text = "New Study";
            this.RbNewStudy.UseVisualStyleBackColor = true;
            // 
            // RbNewSeries
            // 
            this.RbNewSeries.AutoSize = true;
            this.RbNewSeries.Location = new System.Drawing.Point(16, 193);
            this.RbNewSeries.Name = "RbNewSeries";
            this.RbNewSeries.Size = new System.Drawing.Size(89, 19);
            this.RbNewSeries.TabIndex = 1;
            this.RbNewSeries.TabStop = true;
            this.RbNewSeries.Text = "New Series";
            this.RbNewSeries.UseVisualStyleBackColor = true;
            // 
            // RbNewImage
            // 
            this.RbNewImage.AutoSize = true;
            this.RbNewImage.Location = new System.Drawing.Point(16, 230);
            this.RbNewImage.Name = "RbNewImage";
            this.RbNewImage.Size = new System.Drawing.Size(88, 19);
            this.RbNewImage.TabIndex = 0;
            this.RbNewImage.TabStop = true;
            this.RbNewImage.Text = "New Image";
            this.RbNewImage.UseVisualStyleBackColor = true;
            // 
            // BtnSend
            // 
            this.BtnSend.Location = new System.Drawing.Point(30, 418);
            this.BtnSend.Name = "BtnSend";
            this.BtnSend.Size = new System.Drawing.Size(87, 27);
            this.BtnSend.TabIndex = 2;
            this.BtnSend.Text = "Send To PACS";
            this.BtnSend.UseVisualStyleBackColor = true;
            this.BtnSend.Click += new System.EventHandler(this.BtnSend_Click);
            // 
            // CbPacsList
            // 
            this.CbPacsList.FormattingEnabled = true;
            this.CbPacsList.Location = new System.Drawing.Point(30, 387);
            this.CbPacsList.Name = "CbPacsList";
            this.CbPacsList.Size = new System.Drawing.Size(233, 23);
            this.CbPacsList.TabIndex = 3;
            // 
            // CbAnonymize
            // 
            this.CbAnonymize.AutoSize = true;
            this.CbAnonymize.Location = new System.Drawing.Point(16, 81);
            this.CbAnonymize.Name = "CbAnonymize";
            this.CbAnonymize.Size = new System.Drawing.Size(87, 19);
            this.CbAnonymize.TabIndex = 6;
            this.CbAnonymize.Text = "Anonymise";
            this.CbAnonymize.UseVisualStyleBackColor = true;
            // 
            // LblFilePath
            // 
            this.LblFilePath.AutoSize = true;
            this.LblFilePath.Location = new System.Drawing.Point(30, 44);
            this.LblFilePath.Name = "LblFilePath";
            this.LblFilePath.Size = new System.Drawing.Size(56, 17);
            this.LblFilePath.TabIndex = 5;
            this.LblFilePath.Text = "File Path";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.BtnBrowseDicomFile);
            this.groupBox1.Controls.Add(this.LblFilePath);
            this.groupBox1.Controls.Add(this.TxtFilePath);
            this.groupBox1.Controls.Add(this.panel1);
            this.groupBox1.Controls.Add(this.CbPacsList);
            this.groupBox1.Controls.Add(this.BtnSend);
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(291, 453);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Dicom File";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.groupBox1);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(12, 12);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(701, 476);
            this.flowLayoutPanel1.TabIndex = 7;
            // 
            // FdLoadDicomFile
            // 
            this.FdLoadDicomFile.InitialDirectory = "C:\\temp\\test";
            // 
            // BtnBrowseDicomFile
            // 
            this.BtnBrowseDicomFile.Location = new System.Drawing.Point(176, 92);
            this.BtnBrowseDicomFile.Name = "BtnBrowseDicomFile";
            this.BtnBrowseDicomFile.Size = new System.Drawing.Size(87, 27);
            this.BtnBrowseDicomFile.TabIndex = 6;
            this.BtnBrowseDicomFile.Text = "Browse...";
            this.BtnBrowseDicomFile.UseVisualStyleBackColor = true;
            this.BtnBrowseDicomFile.Click += new System.EventHandler(this.BtnBrowseDicomFile_Click);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(719, 494);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Font = new System.Drawing.Font("Calibri", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "FormMain";
            this.Text = "CAPI";
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox TxtFilePath;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.RadioButton RbNewPatient;
        private System.Windows.Forms.RadioButton RbNewStudy;
        private System.Windows.Forms.RadioButton RbNewSeries;
        private System.Windows.Forms.RadioButton RbNewImage;
        private System.Windows.Forms.CheckBox CbRemoveCarerDetails;
        private System.Windows.Forms.CheckBox CbSiteRemoved;
        private System.Windows.Forms.Button BtnSend;
        private System.Windows.Forms.ComboBox CbPacsList;
        private System.Windows.Forms.CheckBox CbAnonymize;
        private System.Windows.Forms.Label LblFilePath;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button BtnBrowseDicomFile;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.OpenFileDialog FdLoadDicomFile;
    }
}

