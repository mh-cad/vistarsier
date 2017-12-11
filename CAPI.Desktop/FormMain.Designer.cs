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
            this.PnlDcmHeaderModifiers = new System.Windows.Forms.Panel();
            this.TxtAccessionNumber = new System.Windows.Forms.TextBox();
            this.PnlPatientDetails = new System.Windows.Forms.Panel();
            this.TxtPatientSex = new System.Windows.Forms.TextBox();
            this.LblPatientSex = new System.Windows.Forms.Label();
            this.TxtPatientBirthDate = new System.Windows.Forms.TextBox();
            this.LblPatientBirthDate = new System.Windows.Forms.Label();
            this.TxtPatientIdFs = new System.Windows.Forms.TextBox();
            this.LblPatientIdFs = new System.Windows.Forms.Label();
            this.TxtPatientName = new System.Windows.Forms.TextBox();
            this.LblPatientName = new System.Windows.Forms.Label();
            this.TxtSeriesDescription = new System.Windows.Forms.TextBox();
            this.CbReidentify = new System.Windows.Forms.CheckBox();
            this.LblSeriesDescription = new System.Windows.Forms.Label();
            this.CbRemoveCarerDetails = new System.Windows.Forms.CheckBox();
            this.LblAccession = new System.Windows.Forms.Label();
            this.CbSiteRemoved = new System.Windows.Forms.CheckBox();
            this.RbOverwriteImage = new System.Windows.Forms.RadioButton();
            this.RbNewStudy = new System.Windows.Forms.RadioButton();
            this.RbNewSeries = new System.Windows.Forms.RadioButton();
            this.RbNewImage = new System.Windows.Forms.RadioButton();
            this.BtnSend = new System.Windows.Forms.Button();
            this.CbDestinationPacs = new System.Windows.Forms.ComboBox();
            this.LblFilePath = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.BtnLoadImage = new System.Windows.Forms.Button();
            this.RbUnmodified = new System.Windows.Forms.RadioButton();
            this.BtnBrowseDicomFile = new System.Windows.Forms.Button();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.TxtImageRepoDicom = new System.Windows.Forms.TextBox();
            this.BtnSaveToDisk = new System.Windows.Forms.Button();
            this.GbArchive = new System.Windows.Forms.GroupBox();
            this.TxtImagesList = new System.Windows.Forms.TextBox();
            this.BtnGetImages = new System.Windows.Forms.Button();
            this.CbSeriesForStudy = new System.Windows.Forms.ComboBox();
            this.BtnGetSeries = new System.Windows.Forms.Button();
            this.CbStudiesFromPacs = new System.Windows.Forms.ComboBox();
            this.LblPatientIdPacs = new System.Windows.Forms.Label();
            this.TxtPatientIdPacs = new System.Windows.Forms.TextBox();
            this.CbSourcePacs = new System.Windows.Forms.ComboBox();
            this.BtnGetStudies = new System.Windows.Forms.Button();
            this.LblSourcePacs = new System.Windows.Forms.Label();
            this.BtnTestProcess = new System.Windows.Forms.Button();
            this.FdLoadDicomFile = new System.Windows.Forms.OpenFileDialog();
            this.DgvLogs = new System.Windows.Forms.DataGridView();
            this.LogText = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.LogTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panel2 = new System.Windows.Forms.Panel();
            this.PnlDcmHeaderModifiers.SuspendLayout();
            this.PnlPatientDetails.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.GbArchive.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DgvLogs)).BeginInit();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // TxtFilePath
            // 
            this.TxtFilePath.Font = new System.Drawing.Font("Verdana", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TxtFilePath.Location = new System.Drawing.Point(33, 55);
            this.TxtFilePath.Name = "TxtFilePath";
            this.TxtFilePath.Size = new System.Drawing.Size(266, 24);
            this.TxtFilePath.TabIndex = 0;
            this.TxtFilePath.Text = "C:\\temp\\test\\00000010";
            // 
            // PnlDcmHeaderModifiers
            // 
            this.PnlDcmHeaderModifiers.Controls.Add(this.TxtAccessionNumber);
            this.PnlDcmHeaderModifiers.Controls.Add(this.PnlPatientDetails);
            this.PnlDcmHeaderModifiers.Controls.Add(this.TxtSeriesDescription);
            this.PnlDcmHeaderModifiers.Controls.Add(this.CbReidentify);
            this.PnlDcmHeaderModifiers.Controls.Add(this.LblSeriesDescription);
            this.PnlDcmHeaderModifiers.Controls.Add(this.CbRemoveCarerDetails);
            this.PnlDcmHeaderModifiers.Controls.Add(this.LblAccession);
            this.PnlDcmHeaderModifiers.Controls.Add(this.CbSiteRemoved);
            this.PnlDcmHeaderModifiers.Controls.Add(this.RbOverwriteImage);
            this.PnlDcmHeaderModifiers.Controls.Add(this.RbNewStudy);
            this.PnlDcmHeaderModifiers.Controls.Add(this.RbNewSeries);
            this.PnlDcmHeaderModifiers.Controls.Add(this.RbNewImage);
            this.PnlDcmHeaderModifiers.Font = new System.Drawing.Font("Marlett", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PnlDcmHeaderModifiers.Location = new System.Drawing.Point(7, 153);
            this.PnlDcmHeaderModifiers.Name = "PnlDcmHeaderModifiers";
            this.PnlDcmHeaderModifiers.Size = new System.Drawing.Size(376, 399);
            this.PnlDcmHeaderModifiers.TabIndex = 1;
            // 
            // TxtAccessionNumber
            // 
            this.TxtAccessionNumber.Enabled = false;
            this.TxtAccessionNumber.Font = new System.Drawing.Font("Verdana", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TxtAccessionNumber.Location = new System.Drawing.Point(144, 241);
            this.TxtAccessionNumber.Name = "TxtAccessionNumber";
            this.TxtAccessionNumber.Size = new System.Drawing.Size(223, 24);
            this.TxtAccessionNumber.TabIndex = 16;
            // 
            // PnlPatientDetails
            // 
            this.PnlPatientDetails.Controls.Add(this.TxtPatientSex);
            this.PnlPatientDetails.Controls.Add(this.LblPatientSex);
            this.PnlPatientDetails.Controls.Add(this.TxtPatientBirthDate);
            this.PnlPatientDetails.Controls.Add(this.LblPatientBirthDate);
            this.PnlPatientDetails.Controls.Add(this.TxtPatientIdFs);
            this.PnlPatientDetails.Controls.Add(this.LblPatientIdFs);
            this.PnlPatientDetails.Controls.Add(this.TxtPatientName);
            this.PnlPatientDetails.Controls.Add(this.LblPatientName);
            this.PnlPatientDetails.Location = new System.Drawing.Point(17, 75);
            this.PnlPatientDetails.Name = "PnlPatientDetails";
            this.PnlPatientDetails.Size = new System.Drawing.Size(354, 134);
            this.PnlPatientDetails.TabIndex = 7;
            // 
            // TxtPatientSex
            // 
            this.TxtPatientSex.Enabled = false;
            this.TxtPatientSex.Font = new System.Drawing.Font("Verdana", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TxtPatientSex.Location = new System.Drawing.Point(127, 96);
            this.TxtPatientSex.Name = "TxtPatientSex";
            this.TxtPatientSex.Size = new System.Drawing.Size(223, 24);
            this.TxtPatientSex.TabIndex = 14;
            // 
            // LblPatientSex
            // 
            this.LblPatientSex.AutoSize = true;
            this.LblPatientSex.Location = new System.Drawing.Point(10, 99);
            this.LblPatientSex.Name = "LblPatientSex";
            this.LblPatientSex.Size = new System.Drawing.Size(71, 15);
            this.LblPatientSex.TabIndex = 13;
            this.LblPatientSex.Text = "Patient Sex:";
            // 
            // TxtPatientBirthDate
            // 
            this.TxtPatientBirthDate.Enabled = false;
            this.TxtPatientBirthDate.Font = new System.Drawing.Font("Verdana", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TxtPatientBirthDate.Location = new System.Drawing.Point(127, 68);
            this.TxtPatientBirthDate.Name = "TxtPatientBirthDate";
            this.TxtPatientBirthDate.Size = new System.Drawing.Size(223, 24);
            this.TxtPatientBirthDate.TabIndex = 12;
            // 
            // LblPatientBirthDate
            // 
            this.LblPatientBirthDate.AutoSize = true;
            this.LblPatientBirthDate.Location = new System.Drawing.Point(10, 71);
            this.LblPatientBirthDate.Name = "LblPatientBirthDate";
            this.LblPatientBirthDate.Size = new System.Drawing.Size(105, 15);
            this.LblPatientBirthDate.TabIndex = 11;
            this.LblPatientBirthDate.Text = "Patient Birth Date:";
            // 
            // TxtPatientIdFs
            // 
            this.TxtPatientIdFs.Enabled = false;
            this.TxtPatientIdFs.Font = new System.Drawing.Font("Verdana", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TxtPatientIdFs.Location = new System.Drawing.Point(127, 41);
            this.TxtPatientIdFs.Name = "TxtPatientIdFs";
            this.TxtPatientIdFs.Size = new System.Drawing.Size(223, 24);
            this.TxtPatientIdFs.TabIndex = 10;
            // 
            // LblPatientIdFs
            // 
            this.LblPatientIdFs.AutoSize = true;
            this.LblPatientIdFs.Location = new System.Drawing.Point(10, 44);
            this.LblPatientIdFs.Name = "LblPatientIdFs";
            this.LblPatientIdFs.Size = new System.Drawing.Size(61, 15);
            this.LblPatientIdFs.TabIndex = 9;
            this.LblPatientIdFs.Text = "Patient Id:";
            // 
            // TxtPatientName
            // 
            this.TxtPatientName.Enabled = false;
            this.TxtPatientName.Font = new System.Drawing.Font("Verdana", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TxtPatientName.Location = new System.Drawing.Point(127, 12);
            this.TxtPatientName.Name = "TxtPatientName";
            this.TxtPatientName.Size = new System.Drawing.Size(223, 24);
            this.TxtPatientName.TabIndex = 8;
            // 
            // LblPatientName
            // 
            this.LblPatientName.AutoSize = true;
            this.LblPatientName.Location = new System.Drawing.Point(10, 15);
            this.LblPatientName.Name = "LblPatientName";
            this.LblPatientName.Size = new System.Drawing.Size(114, 15);
            this.LblPatientName.TabIndex = 0;
            this.LblPatientName.Text = "Patient Description:";
            // 
            // TxtSeriesDescription
            // 
            this.TxtSeriesDescription.Enabled = false;
            this.TxtSeriesDescription.Font = new System.Drawing.Font("Verdana", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TxtSeriesDescription.Location = new System.Drawing.Point(144, 295);
            this.TxtSeriesDescription.Name = "TxtSeriesDescription";
            this.TxtSeriesDescription.Size = new System.Drawing.Size(223, 24);
            this.TxtSeriesDescription.TabIndex = 18;
            // 
            // CbReidentify
            // 
            this.CbReidentify.AutoSize = true;
            this.CbReidentify.Location = new System.Drawing.Point(8, 53);
            this.CbReidentify.Name = "CbReidentify";
            this.CbReidentify.Size = new System.Drawing.Size(84, 19);
            this.CbReidentify.TabIndex = 6;
            this.CbReidentify.Text = "Re-identify";
            this.CbReidentify.UseVisualStyleBackColor = true;
            // 
            // LblSeriesDescription
            // 
            this.LblSeriesDescription.AutoSize = true;
            this.LblSeriesDescription.Location = new System.Drawing.Point(26, 300);
            this.LblSeriesDescription.Name = "LblSeriesDescription";
            this.LblSeriesDescription.Size = new System.Drawing.Size(114, 15);
            this.LblSeriesDescription.TabIndex = 17;
            this.LblSeriesDescription.Text = "DicomSeries Desc:";
            // 
            // CbRemoveCarerDetails
            // 
            this.CbRemoveCarerDetails.AutoSize = true;
            this.CbRemoveCarerDetails.Location = new System.Drawing.Point(8, 30);
            this.CbRemoveCarerDetails.Name = "CbRemoveCarerDetails";
            this.CbRemoveCarerDetails.Size = new System.Drawing.Size(192, 19);
            this.CbRemoveCarerDetails.TabIndex = 6;
            this.CbRemoveCarerDetails.Text = "Remove Care Provider Details";
            this.CbRemoveCarerDetails.UseVisualStyleBackColor = true;
            // 
            // LblAccession
            // 
            this.LblAccession.AutoSize = true;
            this.LblAccession.Location = new System.Drawing.Point(27, 242);
            this.LblAccession.Name = "LblAccession";
            this.LblAccession.Size = new System.Drawing.Size(77, 15);
            this.LblAccession.TabIndex = 15;
            this.LblAccession.Text = "Accession #:";
            // 
            // CbSiteRemoved
            // 
            this.CbSiteRemoved.AutoSize = true;
            this.CbSiteRemoved.Location = new System.Drawing.Point(8, 5);
            this.CbSiteRemoved.Name = "CbSiteRemoved";
            this.CbSiteRemoved.Size = new System.Drawing.Size(138, 19);
            this.CbSiteRemoved.TabIndex = 5;
            this.CbSiteRemoved.Text = "Remove Site Details";
            this.CbSiteRemoved.UseVisualStyleBackColor = true;
            // 
            // RbOverwriteImage
            // 
            this.RbOverwriteImage.AutoSize = true;
            this.RbOverwriteImage.Enabled = false;
            this.RbOverwriteImage.Location = new System.Drawing.Point(8, 357);
            this.RbOverwriteImage.Name = "RbOverwriteImage";
            this.RbOverwriteImage.Size = new System.Drawing.Size(114, 19);
            this.RbOverwriteImage.TabIndex = 3;
            this.RbOverwriteImage.Text = "Overwrite Image";
            this.RbOverwriteImage.UseVisualStyleBackColor = true;
            // 
            // RbNewStudy
            // 
            this.RbNewStudy.AutoSize = true;
            this.RbNewStudy.Location = new System.Drawing.Point(8, 214);
            this.RbNewStudy.Name = "RbNewStudy";
            this.RbNewStudy.Size = new System.Drawing.Size(83, 19);
            this.RbNewStudy.TabIndex = 2;
            this.RbNewStudy.Text = "New Study";
            this.RbNewStudy.UseVisualStyleBackColor = true;
            // 
            // RbNewSeries
            // 
            this.RbNewSeries.AutoSize = true;
            this.RbNewSeries.Location = new System.Drawing.Point(8, 269);
            this.RbNewSeries.Name = "RbNewSeries";
            this.RbNewSeries.Size = new System.Drawing.Size(125, 19);
            this.RbNewSeries.TabIndex = 1;
            this.RbNewSeries.Text = "New DicomSeries";
            this.RbNewSeries.UseVisualStyleBackColor = true;
            // 
            // RbNewImage
            // 
            this.RbNewImage.AutoSize = true;
            this.RbNewImage.Checked = true;
            this.RbNewImage.Location = new System.Drawing.Point(8, 331);
            this.RbNewImage.Name = "RbNewImage";
            this.RbNewImage.Size = new System.Drawing.Size(88, 19);
            this.RbNewImage.TabIndex = 0;
            this.RbNewImage.TabStop = true;
            this.RbNewImage.Text = "New Image";
            this.RbNewImage.UseVisualStyleBackColor = true;
            // 
            // BtnSend
            // 
            this.BtnSend.Enabled = false;
            this.BtnSend.Location = new System.Drawing.Point(24, 556);
            this.BtnSend.Name = "BtnSend";
            this.BtnSend.Size = new System.Drawing.Size(99, 29);
            this.BtnSend.TabIndex = 2;
            this.BtnSend.Text = "Send To PACS";
            this.BtnSend.UseVisualStyleBackColor = true;
            this.BtnSend.Click += new System.EventHandler(this.BtnSend_Click);
            // 
            // CbDestinationPacs
            // 
            this.CbDestinationPacs.FormattingEnabled = true;
            this.CbDestinationPacs.Location = new System.Drawing.Point(130, 558);
            this.CbDestinationPacs.Name = "CbDestinationPacs";
            this.CbDestinationPacs.Size = new System.Drawing.Size(243, 24);
            this.CbDestinationPacs.TabIndex = 3;
            // 
            // LblFilePath
            // 
            this.LblFilePath.AutoSize = true;
            this.LblFilePath.Location = new System.Drawing.Point(33, 35);
            this.LblFilePath.Name = "LblFilePath";
            this.LblFilePath.Size = new System.Drawing.Size(65, 16);
            this.LblFilePath.TabIndex = 5;
            this.LblFilePath.Text = "File Path";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.BtnLoadImage);
            this.groupBox1.Controls.Add(this.RbUnmodified);
            this.groupBox1.Controls.Add(this.BtnBrowseDicomFile);
            this.groupBox1.Controls.Add(this.LblFilePath);
            this.groupBox1.Controls.Add(this.TxtFilePath);
            this.groupBox1.Controls.Add(this.PnlDcmHeaderModifiers);
            this.groupBox1.Controls.Add(this.CbDestinationPacs);
            this.groupBox1.Controls.Add(this.BtnSend);
            this.groupBox1.Location = new System.Drawing.Point(437, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(390, 595);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Dicom File";
            // 
            // BtnLoadImage
            // 
            this.BtnLoadImage.Location = new System.Drawing.Point(33, 87);
            this.BtnLoadImage.Name = "BtnLoadImage";
            this.BtnLoadImage.Size = new System.Drawing.Size(99, 29);
            this.BtnLoadImage.TabIndex = 8;
            this.BtnLoadImage.Text = "Load Image";
            this.BtnLoadImage.UseVisualStyleBackColor = true;
            this.BtnLoadImage.Click += new System.EventHandler(this.BtnLoadImage_Click);
            // 
            // RbUnmodified
            // 
            this.RbUnmodified.AutoSize = true;
            this.RbUnmodified.Checked = true;
            this.RbUnmodified.Location = new System.Drawing.Point(34, 127);
            this.RbUnmodified.Name = "RbUnmodified";
            this.RbUnmodified.Size = new System.Drawing.Size(97, 20);
            this.RbUnmodified.TabIndex = 7;
            this.RbUnmodified.TabStop = true;
            this.RbUnmodified.Text = "Unmodified";
            this.RbUnmodified.UseVisualStyleBackColor = true;
            this.RbUnmodified.CheckedChanged += new System.EventHandler(this.RbUnmodified_CheckedChanged);
            // 
            // BtnBrowseDicomFile
            // 
            this.BtnBrowseDicomFile.Location = new System.Drawing.Point(200, 87);
            this.BtnBrowseDicomFile.Name = "BtnBrowseDicomFile";
            this.BtnBrowseDicomFile.Size = new System.Drawing.Size(99, 29);
            this.BtnBrowseDicomFile.TabIndex = 6;
            this.BtnBrowseDicomFile.Text = "Browse...";
            this.BtnBrowseDicomFile.UseVisualStyleBackColor = true;
            this.BtnBrowseDicomFile.Click += new System.EventHandler(this.BtnBrowseDicomFile_Click);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.panel1);
            this.flowLayoutPanel1.Controls.Add(this.groupBox1);
            this.flowLayoutPanel1.Controls.Add(this.BtnTestProcess);
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(14, 13);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(1121, 641);
            this.flowLayoutPanel1.TabIndex = 7;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.TxtImageRepoDicom);
            this.panel1.Controls.Add(this.BtnSaveToDisk);
            this.panel1.Controls.Add(this.GbArchive);
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(428, 626);
            this.panel1.TabIndex = 7;
            // 
            // TxtImageRepoDicom
            // 
            this.TxtImageRepoDicom.Font = new System.Drawing.Font("Verdana", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TxtImageRepoDicom.Location = new System.Drawing.Point(13, 588);
            this.TxtImageRepoDicom.Name = "TxtImageRepoDicom";
            this.TxtImageRepoDicom.Size = new System.Drawing.Size(284, 24);
            this.TxtImageRepoDicom.TabIndex = 21;
            this.TxtImageRepoDicom.Text = "C:\\temp\\test\\CC-Storage";
            // 
            // BtnSaveToDisk
            // 
            this.BtnSaveToDisk.Location = new System.Drawing.Point(303, 588);
            this.BtnSaveToDisk.Name = "BtnSaveToDisk";
            this.BtnSaveToDisk.Size = new System.Drawing.Size(116, 26);
            this.BtnSaveToDisk.TabIndex = 21;
            this.BtnSaveToDisk.Text = "Save To Disk";
            this.BtnSaveToDisk.UseVisualStyleBackColor = true;
            this.BtnSaveToDisk.Click += new System.EventHandler(this.BtnSaveToDisk_Click);
            // 
            // GbArchive
            // 
            this.GbArchive.Controls.Add(this.TxtImagesList);
            this.GbArchive.Controls.Add(this.BtnGetImages);
            this.GbArchive.Controls.Add(this.CbSeriesForStudy);
            this.GbArchive.Controls.Add(this.BtnGetSeries);
            this.GbArchive.Controls.Add(this.CbStudiesFromPacs);
            this.GbArchive.Controls.Add(this.LblPatientIdPacs);
            this.GbArchive.Controls.Add(this.TxtPatientIdPacs);
            this.GbArchive.Controls.Add(this.CbSourcePacs);
            this.GbArchive.Controls.Add(this.BtnGetStudies);
            this.GbArchive.Controls.Add(this.LblSourcePacs);
            this.GbArchive.Location = new System.Drawing.Point(3, 0);
            this.GbArchive.Name = "GbArchive";
            this.GbArchive.Size = new System.Drawing.Size(422, 582);
            this.GbArchive.TabIndex = 9;
            this.GbArchive.TabStop = false;
            this.GbArchive.Text = "Archive";
            // 
            // TxtImagesList
            // 
            this.TxtImagesList.Font = new System.Drawing.Font("Verdana", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TxtImagesList.Location = new System.Drawing.Point(10, 276);
            this.TxtImagesList.Multiline = true;
            this.TxtImagesList.Name = "TxtImagesList";
            this.TxtImagesList.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.TxtImagesList.Size = new System.Drawing.Size(406, 300);
            this.TxtImagesList.TabIndex = 20;
            // 
            // BtnGetImages
            // 
            this.BtnGetImages.Location = new System.Drawing.Point(317, 244);
            this.BtnGetImages.Name = "BtnGetImages";
            this.BtnGetImages.Size = new System.Drawing.Size(99, 26);
            this.BtnGetImages.TabIndex = 19;
            this.BtnGetImages.Text = "Get Images";
            this.BtnGetImages.UseVisualStyleBackColor = true;
            this.BtnGetImages.Click += new System.EventHandler(this.BtnGetImages_Click);
            // 
            // CbSeriesForStudy
            // 
            this.CbSeriesForStudy.Font = new System.Drawing.Font("Verdana", 7.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CbSeriesForStudy.FormattingEnabled = true;
            this.CbSeriesForStudy.Location = new System.Drawing.Point(10, 212);
            this.CbSeriesForStudy.Name = "CbSeriesForStudy";
            this.CbSeriesForStudy.Size = new System.Drawing.Size(406, 20);
            this.CbSeriesForStudy.TabIndex = 18;
            // 
            // BtnGetSeries
            // 
            this.BtnGetSeries.Location = new System.Drawing.Point(284, 179);
            this.BtnGetSeries.Name = "BtnGetSeries";
            this.BtnGetSeries.Size = new System.Drawing.Size(132, 26);
            this.BtnGetSeries.TabIndex = 17;
            this.BtnGetSeries.Text = "Get DicomSeries for Study";
            this.BtnGetSeries.UseVisualStyleBackColor = true;
            this.BtnGetSeries.Click += new System.EventHandler(this.BtnGetSeries_Click);
            // 
            // CbStudiesFromPacs
            // 
            this.CbStudiesFromPacs.Font = new System.Drawing.Font("Verdana", 7.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CbStudiesFromPacs.FormattingEnabled = true;
            this.CbStudiesFromPacs.Location = new System.Drawing.Point(10, 149);
            this.CbStudiesFromPacs.Name = "CbStudiesFromPacs";
            this.CbStudiesFromPacs.Size = new System.Drawing.Size(406, 20);
            this.CbStudiesFromPacs.TabIndex = 16;
            // 
            // LblPatientIdPacs
            // 
            this.LblPatientIdPacs.AutoSize = true;
            this.LblPatientIdPacs.Location = new System.Drawing.Point(7, 94);
            this.LblPatientIdPacs.Name = "LblPatientIdPacs";
            this.LblPatientIdPacs.Size = new System.Drawing.Size(79, 16);
            this.LblPatientIdPacs.TabIndex = 15;
            this.LblPatientIdPacs.Text = "Patient Id:";
            // 
            // TxtPatientIdPacs
            // 
            this.TxtPatientIdPacs.Font = new System.Drawing.Font("Verdana", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TxtPatientIdPacs.Location = new System.Drawing.Point(10, 115);
            this.TxtPatientIdPacs.Name = "TxtPatientIdPacs";
            this.TxtPatientIdPacs.Size = new System.Drawing.Size(223, 24);
            this.TxtPatientIdPacs.TabIndex = 15;
            this.TxtPatientIdPacs.Text = "1200633";
            // 
            // CbSourcePacs
            // 
            this.CbSourcePacs.FormattingEnabled = true;
            this.CbSourcePacs.Location = new System.Drawing.Point(10, 57);
            this.CbSourcePacs.Name = "CbSourcePacs";
            this.CbSourcePacs.Size = new System.Drawing.Size(223, 24);
            this.CbSourcePacs.TabIndex = 9;
            // 
            // BtnGetStudies
            // 
            this.BtnGetStudies.Location = new System.Drawing.Point(239, 115);
            this.BtnGetStudies.Name = "BtnGetStudies";
            this.BtnGetStudies.Size = new System.Drawing.Size(99, 26);
            this.BtnGetStudies.TabIndex = 8;
            this.BtnGetStudies.Text = "Get Studies for Patient";
            this.BtnGetStudies.UseVisualStyleBackColor = true;
            this.BtnGetStudies.Click += new System.EventHandler(this.BtnGetStudies_Click);
            // 
            // LblSourcePacs
            // 
            this.LblSourcePacs.AutoSize = true;
            this.LblSourcePacs.Location = new System.Drawing.Point(7, 35);
            this.LblSourcePacs.Name = "LblSourcePacs";
            this.LblSourcePacs.Size = new System.Drawing.Size(150, 16);
            this.LblSourcePacs.TabIndex = 5;
            this.LblSourcePacs.Text = "Dicom Node To Query";
            // 
            // BtnTestProcess
            // 
            this.BtnTestProcess.Location = new System.Drawing.Point(833, 3);
            this.BtnTestProcess.Name = "BtnTestProcess";
            this.BtnTestProcess.Size = new System.Drawing.Size(234, 39);
            this.BtnTestProcess.TabIndex = 8;
            this.BtnTestProcess.Text = "Test Process";
            this.BtnTestProcess.UseVisualStyleBackColor = true;
            this.BtnTestProcess.Click += new System.EventHandler(this.BtnTestProcess_Click);
            // 
            // FdLoadDicomFile
            // 
            this.FdLoadDicomFile.InitialDirectory = "C:\\temp\\test";
            // 
            // DgvLogs
            // 
            this.DgvLogs.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DgvLogs.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.LogText,
            this.LogTime});
            this.DgvLogs.Location = new System.Drawing.Point(3, 3);
            this.DgvLogs.Name = "DgvLogs";
            this.DgvLogs.Size = new System.Drawing.Size(1112, 237);
            this.DgvLogs.TabIndex = 9;
            // 
            // LogText
            // 
            this.LogText.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.LogText.FillWeight = 194.9239F;
            this.LogText.HeaderText = "Log Text";
            this.LogText.Name = "LogText";
            this.LogText.ReadOnly = true;
            // 
            // LogTime
            // 
            this.LogTime.FillWeight = 5.076141F;
            this.LogTime.HeaderText = "Time";
            this.LogTime.Name = "LogTime";
            this.LogTime.ReadOnly = true;
            this.LogTime.Width = 200;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.DgvLogs);
            this.panel2.Location = new System.Drawing.Point(14, 660);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1121, 212);
            this.panel2.TabIndex = 8;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1141, 878);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.Name = "FormMain";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "CAPI";
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.PnlDcmHeaderModifiers.ResumeLayout(false);
            this.PnlDcmHeaderModifiers.PerformLayout();
            this.PnlPatientDetails.ResumeLayout(false);
            this.PnlPatientDetails.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.GbArchive.ResumeLayout(false);
            this.GbArchive.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DgvLogs)).EndInit();
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox TxtFilePath;
        private System.Windows.Forms.Panel PnlDcmHeaderModifiers;
        private System.Windows.Forms.RadioButton RbNewStudy;
        private System.Windows.Forms.RadioButton RbNewSeries;
        private System.Windows.Forms.RadioButton RbNewImage;
        private System.Windows.Forms.CheckBox CbRemoveCarerDetails;
        private System.Windows.Forms.CheckBox CbSiteRemoved;
        private System.Windows.Forms.Button BtnSend;
        private System.Windows.Forms.ComboBox CbDestinationPacs;
        private System.Windows.Forms.CheckBox CbReidentify;
        private System.Windows.Forms.Label LblFilePath;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button BtnBrowseDicomFile;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.OpenFileDialog FdLoadDicomFile;
        private System.Windows.Forms.RadioButton RbUnmodified;
        private System.Windows.Forms.Panel PnlPatientDetails;
        private System.Windows.Forms.TextBox TxtPatientSex;
        private System.Windows.Forms.Label LblPatientSex;
        private System.Windows.Forms.TextBox TxtPatientBirthDate;
        private System.Windows.Forms.Label LblPatientBirthDate;
        private System.Windows.Forms.TextBox TxtPatientIdFs;
        private System.Windows.Forms.Label LblPatientIdFs;
        private System.Windows.Forms.TextBox TxtPatientName;
        private System.Windows.Forms.Label LblPatientName;
        private System.Windows.Forms.Button BtnLoadImage;
        private System.Windows.Forms.TextBox TxtSeriesDescription;
        private System.Windows.Forms.Label LblSeriesDescription;
        private System.Windows.Forms.TextBox TxtAccessionNumber;
        private System.Windows.Forms.Label LblAccession;
        private System.Windows.Forms.RadioButton RbOverwriteImage;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.GroupBox GbArchive;
        private System.Windows.Forms.ComboBox CbSourcePacs;
        private System.Windows.Forms.Button BtnGetStudies;
        private System.Windows.Forms.Label LblSourcePacs;
        private System.Windows.Forms.Label LblPatientIdPacs;
        private System.Windows.Forms.TextBox TxtPatientIdPacs;
        private System.Windows.Forms.ComboBox CbStudiesFromPacs;
        private System.Windows.Forms.ComboBox CbSeriesForStudy;
        private System.Windows.Forms.Button BtnGetSeries;
        private System.Windows.Forms.TextBox TxtImagesList;
        private System.Windows.Forms.Button BtnGetImages;
        private System.Windows.Forms.Button BtnTestProcess;
        private System.Windows.Forms.DataGridView DgvLogs;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.DataGridViewTextBoxColumn LogText;
        private System.Windows.Forms.DataGridViewTextBoxColumn LogTime;
        private System.Windows.Forms.Button BtnSaveToDisk;
        private System.Windows.Forms.TextBox TxtImageRepoDicom;
    }
}

