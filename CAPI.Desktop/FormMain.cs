using CAPI.Common.Services;
using CAPI.DAL;
using CAPI.DAL.Abstraction;
using CAPI.Dicom;
using CAPI.Dicom.Abstraction;
using CAPI.Dicom.Model;
using CAPI.ImageProcessing;
using CAPI.ImageProcessing.Abstraction;
using CAPI.JobManager;
using CAPI.JobManager.Abstraction;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Unity;

namespace CAPI.Desktop
{
    public partial class FormMain : Form
    {
        private string _tempFileToSend;
        private readonly IDicomNode _localDicomNode;
        private IUnityContainer _unityContainer;
        private IDicomFactory _dicomFactory;
        private IDicomNodeRepository _dicomNodeRepo;
        private IRecipeRepositoryInMemory<IRecipe> _recipeRepositoryInMemory;
        private IJobBuilder _jobBuilder;
        private IJobManagerFactory _jobManagerFactory;

        public FormMain()
        {
            InitializeComponent();

            InitializeUnity();

            _localDicomNode = _dicomNodeRepo.GetAll()
                .FirstOrDefault(n => string.Equals(n.AeTitle,
                Environment.GetEnvironmentVariable("DcmNodeAET_Local", EnvironmentVariableTarget.User),
                StringComparison.CurrentCultureIgnoreCase));
        }

        private void InitializeUnity()
        {
            _unityContainer = new UnityContainer();
            RegisterClasses();
            _dicomFactory = _unityContainer.Resolve<IDicomFactory>();
            _dicomNodeRepo = _unityContainer.Resolve<IDicomNodeRepository>();
            _jobManagerFactory = _unityContainer.Resolve<IJobManagerFactory>();
            _jobBuilder = _unityContainer.Resolve<IJobBuilder>();
            _recipeRepositoryInMemory = _unityContainer.Resolve<IRecipeRepositoryInMemory<IRecipe>>();
        }

        private void RegisterClasses()
        {
            _unityContainer.RegisterType<IDicomNode, DicomNode>();
            _unityContainer.RegisterType<IDicomFactory, DicomFactory>();
            _unityContainer.RegisterType<IDicomServices, DicomServices>();
            _unityContainer.RegisterType<IImageConverter, ImageConverter>();
            _unityContainer.RegisterType<IImageProcessor, ImageProcessor>();
            _unityContainer.RegisterType<IJobManagerFactory, JobManagerFactory>();
            _unityContainer.RegisterType<IRecipe, Recipe>();
            _unityContainer.RegisterType<IJob<IRecipe>, Job<IRecipe>>();
            _unityContainer.RegisterType<IJobBuilder, JobBuilder>();
            _unityContainer.RegisterType<ISeriesSelectionCriteria, SeriesSelectionCriteria>();
            _unityContainer.RegisterType<IIntegratedProcess, IntegratedProcess>();
            _unityContainer.RegisterType<IDestination, Destination>();
            _unityContainer.RegisterType<IRecipeRepositoryInMemory<IRecipe>, RecipeRepositoryInMemory<Recipe>>();
            _unityContainer.RegisterType<IDicomNodeRepository, DicomNodeRepositoryInMemory>();
            _unityContainer.RegisterType<IValueComparer, ValueComparer>();
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
            var dicomNodes = _dicomNodeRepo.GetAll().ToList();
            object[] items = dicomNodes.Select(n => n.LogicalName).ToArray();

            CbDestinationPacs.Items.AddRange(items);
            CbSourcePacs.Items.AddRange(items);
            if (CbDestinationPacs.Items.Count > 0) CbDestinationPacs.SelectedIndex = 1;
            if (CbSourcePacs.Items.Count > 0) CbSourcePacs.SelectedIndex = CbSourcePacs.Items.Count - 2;
        }

        private IDicomNode GetSelectedDestinationDicomNode() => _dicomNodeRepo.GetAll()
            .FirstOrDefault(node => node.LogicalName == CbDestinationPacs.SelectedItem.ToString());

        private IDicomNode GetSelectedSourceDicomNode() => _dicomNodeRepo.GetAll()
            .FirstOrDefault(node => node.LogicalName == CbSourcePacs.SelectedItem.ToString());

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
            LogToDataGridView("This is a test");
        }

        private void BtnSend_Click(object sender, EventArgs e)
        {
            UpdateDicomTags();
            var destinationNode = GetSelectedDestinationDicomNode();
            var dicomServices = _dicomFactory.CreateDicomServices();
            dicomServices.SendDicomFile(_tempFileToSend, _localDicomNode.AeTitle, destinationNode);
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
        private void BtnGetStudies_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(TxtPatientIdPacs.Text)) return;
            var dicomNode = _dicomNodeRepo.GetAll().ToList()
                .FirstOrDefault(n => string.Equals(n.LogicalName, CbSourcePacs.Text, StringComparison.CurrentCultureIgnoreCase));
            var dicomServices = _dicomFactory.CreateDicomServices();
            var accessions = dicomServices.GetStudiesForPatientId(TxtPatientIdPacs.Text, _localDicomNode, dicomNode)
                .Select(study => study.AccessionNumber + "|" + study.StudyInstanceUid);
            CbStudiesFromPacs.Items.Clear();
            var accessionNos = accessions as string[] ?? accessions.ToArray();
            if (!accessionNos.Any()) return;
            CbStudiesFromPacs.Items.AddRange(accessionNos.ToArray());
            if (CbStudiesFromPacs.Items.Count > 0) CbStudiesFromPacs.SelectedIndex = 0;
        }

        private void BtnGetSeries_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(CbStudiesFromPacs.Text)) return;
            var dicomNode = _dicomNodeRepo.GetAll().ToList()
                .FirstOrDefault(n => string.Equals(n.LogicalName, CbSourcePacs.Text, StringComparison.CurrentCultureIgnoreCase));
            var studyUid = CbStudiesFromPacs.Text.Split('|').LastOrDefault();
            var dicomServices = _dicomFactory.CreateDicomServices();
            var series = dicomServices.GetSeriesForStudy(studyUid, _localDicomNode, dicomNode)
                .Select(s => $"{s.SeriesDescription}|{s.SeriesInstanceUid}");
            CbSeriesForStudy.Items.Clear();
            if (!series.Any()) return;
            CbSeriesForStudy.Items.AddRange(series.ToArray());
            if (CbSeriesForStudy.Items.Count > 0) CbSeriesForStudy.SelectedIndex = 0;
        }

        private void BtnGetImages_Click(object sender, EventArgs e)
        {
            var studyUid = CbStudiesFromPacs.Text.Split('|').LastOrDefault();
            var seriesUid = CbSeriesForStudy.Text.Split('|').LastOrDefault();
            var dicomNode = _dicomNodeRepo.GetAll().ToList()
                .FirstOrDefault(n => string.Equals(n.LogicalName, CbSourcePacs.Text, StringComparison.CurrentCultureIgnoreCase));
            var dicomServices = _dicomFactory.CreateDicomServices();
            var series = dicomServices.GetSeriesForSeriesUid(studyUid, seriesUid, _localDicomNode, dicomNode);

            TxtImagesList.Text = "";
            foreach (var image in series.Images)
                TxtImagesList.Text += image.ImageUid + Environment.NewLine;
        }

        private void BtnSaveToDisk_Click(object sender, EventArgs e)
        {
            var series = _dicomFactory.CreateDicomSeries();
            var imageUidList = TxtImagesList.Lines
                .Where(l => !string.IsNullOrEmpty(l) && !string.IsNullOrWhiteSpace(l));

            series.Images = imageUidList.Select(s => _dicomFactory.CreateDicomImage(s));
            series.SeriesInstanceUid = CbSeriesForStudy.Text.Split('|').LastOrDefault();
            series.StudyInstanceUid = CbStudiesFromPacs.Text.Split('|').LastOrDefault();

            var dicomServices = _dicomFactory.CreateDicomServices();
            var dicomNode = _dicomNodeRepo.GetAll().ToList().FirstOrDefault(n =>
                string.Equals(n.LogicalName, CbSourcePacs.Text, StringComparison.CurrentCultureIgnoreCase));

            dicomServices.SaveSeriesToLocalDisk(series, TxtImageRepoDicom.Text, _localDicomNode, dicomNode);
        }

        private void BtnReadRecipeAndRun_Click(object sender, EventArgs e)
        {
            var recipe = _recipeRepositoryInMemory.GetAll().FirstOrDefault();
            var localNode = _dicomNodeRepo.GetAll().FirstOrDefault(n => n.AeTitle == _localDicomNode.AeTitle);
            if (string.IsNullOrEmpty(recipe.SourceAet) || string.IsNullOrWhiteSpace(recipe.SourceAet))
                throw new ArgumentNullException(nameof(recipe.SourceAet), "Source AE Title in recipe is not specified");
            var sourceNode = _dicomNodeRepo.GetAll().FirstOrDefault(n => n.AeTitle == recipe.SourceAet);

            recipe = GetDicomDesntinations(recipe);

            var job = _jobBuilder.Build(recipe, localNode, sourceNode);
            job.OnEachProcessCompleted += Process_Completed;

            LogToDataGridView($"Fixed: {job.DicomSeriesFixed.Original.ParentDicomStudy.AccessionNumber}");
            LogToDataGridView($"Floating: {job.DicomSeriesFloating.Original.ParentDicomStudy.AccessionNumber}");

            job.Run();
        }

        private IRecipe GetDicomDesntinations(IRecipe recipe)
        {
            var updatedRecipe = recipe;
            updatedRecipe.Destinations = new List<IDestination>();

            foreach (var destination in recipe.Destinations)
            {
                var updatedDestination = destination;

                if (string.IsNullOrEmpty(destination.AeTitle))
                    FileSystem.DirectoryExistsIfNotCreate(updatedDestination.FolderPath);
                else
                {
                    var dicomNode = _dicomNodeRepo.GetAll().FirstOrDefault(n => n.AeTitle == destination.AeTitle);
                    updatedDestination.DicomNode = dicomNode ??
                        throw new Exception($"Unable to find dicom node for AE Title [{destination.AeTitle}] in datasource {nameof(_dicomNodeRepo)}");
                }
                updatedRecipe.Destinations.Add(updatedDestination);
            }
            return updatedRecipe;
        }

        #endregion

        private void LogToDataGridView(string logContent)
        {
            var row = new DataGridViewRow();
            var cell1 = new DataGridViewTextBoxCell { Value = logContent };
            var cell2 = new DataGridViewTextBoxCell { Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
            row.Cells.AddRange(cell1, cell2);
            DgvLogs.Rows.Insert(0, row);
        }

        private void Process_Completed(object sender, IProcessEventArgument e)
        {
            LogToDataGridView(e.LogContent);
        }
    }
}