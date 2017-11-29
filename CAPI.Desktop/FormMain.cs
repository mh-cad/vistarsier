using CAPI.DAL;
using CAPI.DAL.Abstraction;
using CAPI.Dicom;
using CAPI.Dicom.Abstraction;
using CAPI.Dicom.Model;
using CAPI.JobManager;
using CAPI.JobManager.Abstraction;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Unity;

namespace CAPI.Desktop
{
    public partial class FormMain : Form
    {
        private string _tempFileToSend;
        private const string SenderAet = "Home PC"; // TODO3: Hard-Coded PACS AET
        private readonly IDicomNode _localDicomNode;
        private IUnityContainer _unityContainer;
        private IDicomFactory _dicomFactory;
        private IDicomNodeRepository _dicomNodeRepo;
        private IJobManagerFactory _jobManagerFactory;
        private IRecipeRepositoryInMemory _recipeRepositoryInMemory;

        public FormMain()
        {
            InitializeComponent();

            InitializeUnity();

            _localDicomNode = _dicomNodeRepo.GetAll(_dicomFactory)
                .FirstOrDefault(n => string.Equals(n.LogicalName, SenderAet,
                StringComparison.CurrentCultureIgnoreCase));
        }

        private void InitializeUnity()
        {
            _unityContainer = new UnityContainer();
            RegisterClasses();
            _dicomNodeRepo = _unityContainer.Resolve<IDicomNodeRepositoryInMemory>();
            _dicomFactory = _unityContainer.Resolve<IDicomFactory>();
            _jobManagerFactory = _unityContainer.Resolve<IJobManagerFactory>();
            _recipeRepositoryInMemory = _unityContainer.Resolve<IRecipeRepositoryInMemory>();
        }

        private void RegisterClasses()
        {
            _unityContainer.RegisterType<IDicomNode, DicomNode>();
            _unityContainer.RegisterType<IDicomFactory, DicomFactory>();
            _unityContainer.RegisterType<IJobManagerFactory, JobManagerFactory>();
            _unityContainer.RegisterType<IRecipeRepositoryInMemory, RecipeRepositoryInMemory>();
            _unityContainer.RegisterType<IDicomNodeRepositoryInMemory, DicomNodeRepositoryInMemory>();
        }

        private void BindDicomHeaderModifiersEventHandlers()
        {
            foreach (Control control in PnlDcmHeaderModifiers.Controls)
            {
                if (control.GetType() == typeof(CheckBox)) ((CheckBox)control).CheckedChanged += CheckBoxes_CheckedChanged;
                if (control.GetType() == typeof(RadioButton)) ((RadioButton)control).CheckedChanged += CheckBoxes_CheckedChanged;
            }
        }

        private void LoadDicomNodes()
        {
            var dicomNodes = _dicomNodeRepo.GetAll(_dicomFactory).ToList();
            object[] items = dicomNodes.Select(n => n.LogicalName).ToArray();

            CbPacsList.Items.AddRange(items);
            CbSourcePacs.Items.AddRange(items);
            if (CbPacsList.Items.Count > 0) CbPacsList.SelectedIndex = 1;
            if (CbSourcePacs.Items.Count > 0) CbSourcePacs.SelectedIndex = CbSourcePacs.Items.Count - 2;
        }

        private IDicomNode GetSelectedDicomNode() => _dicomNodeRepo.GetAll(_dicomFactory)
            .FirstOrDefault(node => node.LogicalName == CbPacsList.SelectedItem.ToString());

        private void UpdateDicomTags()
        {
            var dicomTags = _dicomFactory.CreateDicomTagCollection();

            var dicomNewObjectType = DicomNewObjectType.NoChange;

            if (RbNewStudy.Checked) dicomNewObjectType = DicomNewObjectType.NewStudy;
            if (RbNewSeries.Checked) dicomNewObjectType = DicomNewObjectType.NewSeries;
            if (RbNewImage.Checked) dicomNewObjectType = DicomNewObjectType.NewImage;
            if (CbReidentify.Checked)
            {
                dicomNewObjectType = DicomNewObjectType.Anonymized;
                dicomTags.PatientName.Values = new[] { TxtPatientName.Text };
                dicomTags.PatientId.Values = new[] { TxtPatientIdFs.Text };
                dicomTags.PatientBirthDate.Values = new[] { TxtPatientBirthDate.Text };
                dicomTags.PatientSex.Values = new[] { TxtPatientSex.Text };
            }
            if (TxtAccessionNumber.Enabled) dicomTags.StudyAccessionNumber.Values = new[] { TxtAccessionNumber.Text };
            if (TxtSeriesDescription.Enabled) dicomTags.SeriesDescription.Values = new[] { TxtSeriesDescription.Text };

            var dicomServices = _dicomFactory.CreateDicomServices();
            dicomServices.UpdateDicomHeaders(_tempFileToSend, dicomTags, dicomNewObjectType);

            if (CbSiteRemoved.Checked)
                dicomServices.UpdateDicomHeaders(_tempFileToSend, dicomTags, DicomNewObjectType.SiteDetailsRemoved);

            if (CbRemoveCarerDetails.Checked)
                dicomServices.UpdateDicomHeaders(_tempFileToSend, dicomTags, DicomNewObjectType.CareProviderDetailsRemoved);
        }

        private void LoadTagsPopulateFields()
        {
            if (_tempFileToSend == null) return;
            var dicomServices = _dicomFactory.CreateDicomServices();
            var tags = dicomServices.GetDicomTags(_tempFileToSend);
            TxtPatientName.Text = tags.PatientName.Values.FirstOrDefault();
            TxtPatientIdFs.Text = tags.PatientId.Values.FirstOrDefault();
            TxtPatientBirthDate.Text = tags.PatientBirthDate.Values.FirstOrDefault();
            TxtPatientSex.Text = tags.PatientSex.Values.FirstOrDefault();
            TxtAccessionNumber.Text = tags.StudyAccessionNumber.Values.FirstOrDefault();
            TxtSeriesDescription.Text = tags.SeriesDescription.Values.FirstOrDefault();
        }

        #region "Event Handlers"
        private void FormMain_Load(object sender, EventArgs e)
        {
            LoadDicomNodes();
            BindDicomHeaderModifiersEventHandlers();
        }

        private void BtnSend_Click(object sender, EventArgs e)
        {
            UpdateDicomTags();
            var destinationNode = GetSelectedDicomNode();
            var dicomServices = _dicomFactory.CreateDicomServices();
            dicomServices.SendDicomFile(_tempFileToSend, SenderAet, destinationNode);
            if (File.Exists(_tempFileToSend)) File.Delete(_tempFileToSend);
            _tempFileToSend = null;
            ResetAllControls();
        }

        private void ResetAllControls()
        {
            PnlDcmHeaderModifiers.Controls.Cast<Control>()
                .ToList().ForEach(c =>
                    {
                        if (c.GetType() == typeof(CheckBox)) ((CheckBox)c).Checked = false;
                        if (c.GetType() == typeof(RadioButton)) ((RadioButton)c).Checked = false;
                        if (c.GetType() == typeof(TextBox)) ((TextBox)c).Text = "";
                    }
                );
            PnlPatientDetails.Controls.Cast<Control>()
                .ToList().ForEach(c => { if (c.GetType() == typeof(TextBox)) ((TextBox)c).Text = ""; });
            RbUnmodified.Checked = true;
        }

        private void CheckBoxes_CheckedChanged(object sender, EventArgs e)
        {
            if (sender.GetType() == typeof(CheckBox) && ((CheckBox)sender).Checked)
                RbUnmodified.Checked = false;
            if (sender.GetType() == typeof(RadioButton) && ((RadioButton)sender).Checked)
                RbUnmodified.Checked = false;

            foreach (Control control in PnlPatientDetails.Controls)
                control.Enabled = CbReidentify.Checked;

            TxtAccessionNumber.Enabled = RbNewStudy.Checked;
            TxtSeriesDescription.Enabled = RbNewSeries.Checked;

            if (!CbReidentify.Checked) LoadTagsPopulateFields();
            else
            {
                TxtAccessionNumber.Enabled = true;
                TxtSeriesDescription.Enabled = true;
            }

        }

        private void BtnBrowseDicomFile_Click(object sender, EventArgs e)
        {
            var result = FdLoadDicomFile.ShowDialog();
            if (result == DialogResult.OK) TxtFilePath.Text = FdLoadDicomFile.FileName;
            FdLoadDicomFile.FileName = "";
        }

        private void RbUnmodified_CheckedChanged(object sender, EventArgs e)
        {
            if (!RbUnmodified.Checked) return;
            foreach (Control control in PnlDcmHeaderModifiers.Controls)
            {
                if (control.GetType() == typeof(CheckBox)) ((CheckBox)control).Checked = false;
                if (control.GetType() == typeof(RadioButton)) ((RadioButton)control).Checked = false;
            }
        }

        private void BtnLoadImage_Click(object sender, EventArgs e)
        {
            if (!File.Exists(TxtFilePath.Text))
            {
                MessageBox.Show(@"File does not exist!", @"File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            _tempFileToSend = TxtFilePath.Text + "_modified";
            File.Copy(TxtFilePath.Text, _tempFileToSend, true);
            LoadTagsPopulateFields();
            BtnSend.Enabled = true;
        }
        #endregion

        private void BtnGetStudies_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(TxtPatientIdPacs.Text)) return;
            var dicomNode = _dicomNodeRepo.GetAll(_dicomFactory).ToList()
                .FirstOrDefault(n => string.Equals(n.LogicalName, CbSourcePacs.Text, StringComparison.CurrentCultureIgnoreCase));
            var dicomServices = _dicomFactory.CreateDicomServices();
            var accessions = dicomServices.GetStudiesForPatientId(TxtPatientIdPacs.Text, _localDicomNode, dicomNode);
            CbStudiesFromPacs.Items.Clear();
            if (accessions == null) return;
            CbStudiesFromPacs.Items.AddRange(accessions.ToArray());
            if (CbStudiesFromPacs.Items.Count > 0) CbStudiesFromPacs.SelectedIndex = 0;
        }

        private void BtnGetSeries_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(CbStudiesFromPacs.Text)) return;
            var dicomNode = _dicomNodeRepo.GetAll(_dicomFactory).ToList()
                .FirstOrDefault(n => string.Equals(n.LogicalName, CbSourcePacs.Text, StringComparison.CurrentCultureIgnoreCase));
            var accession = CbStudiesFromPacs.Text.Split('|').FirstOrDefault();
            var studyUid = CbStudiesFromPacs.Text.Split('|').LastOrDefault();
            var dicomServices = _dicomFactory.CreateDicomServices();
            var series = dicomServices.GetSeriesForStudy(studyUid, accession, _localDicomNode, dicomNode);
            CbSeriesForStudy.Items.Clear();
            if (series == null) return;
            CbSeriesForStudy.Items.AddRange(series.ToArray());
            if (CbSeriesForStudy.Items.Count > 0) CbSeriesForStudy.SelectedIndex = 0;
        }

        private void BtnGetImages_Click(object sender, EventArgs e)
        {
            var studyUid = CbStudiesFromPacs.Text.Split('|').LastOrDefault();
            var seriesUid = CbSeriesForStudy.Text.Split('|').LastOrDefault();
            var dicomNode = _dicomNodeRepo.GetAll(_dicomFactory).ToList()
                .FirstOrDefault(n => string.Equals(n.LogicalName, CbSourcePacs.Text, StringComparison.CurrentCultureIgnoreCase));
            var dicomServices = _dicomFactory.CreateDicomServices();
            var images = dicomServices.GetImagesForSeries(studyUid, seriesUid, _localDicomNode, dicomNode);
            TxtImagesList.Text = "";
            foreach (var image in images)
            {
                TxtImagesList.Text += image + Environment.NewLine;
            }
        }

        private void BtnTestProcess_Click(object sender, EventArgs e)
        {
            var recipes = _recipeRepositoryInMemory.GetAll(_jobManagerFactory);
        }
    }
}
