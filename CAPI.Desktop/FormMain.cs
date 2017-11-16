using System.IO;
using System.Linq;
using System.Windows.Forms;
using CAPI.DAL;
using CAPI.Dicom;
using CAPI.Dicom.Model;

namespace CAPI.Desktop
{
    public partial class FormMain : Form
    {
        private readonly DicomNodeRepository _dicomNodeRepo;

        public FormMain()
        {
            InitializeComponent();
            _dicomNodeRepo = new DicomNodeRepository();
        }

        private void FormMain_Load(object sender, System.EventArgs e)
        {
            LoadDicomNodes();
        }

        private void LoadDicomNodes()
        {
            var dicomNodes = _dicomNodeRepo.GetAll().ToList();
            object[] items = dicomNodes.Select(n => n.AeTitle).ToArray();
            CbPacsList.Items.AddRange(items);
            if (CbPacsList.Items.Count > 0) CbPacsList.SelectedIndex = 0;
        }

        private DicomNode GetSelectedDicomNode() => _dicomNodeRepo.GetAll()
                .FirstOrDefault(node => node.AeTitle == CbPacsList.SelectedItem.ToString());

        private void BtnSend_Click(object sender, System.EventArgs e)
        {
            var filepath = TxtFilePath.Text + "_modified";
            File.Copy(TxtFilePath.Text, filepath, true);

            var destinationNode = GetSelectedDicomNode();

            var dicomNewObjectType = DicomNewObjectType.NoChange;
            if (CbAnonymize.Checked) dicomNewObjectType = DicomNewObjectType.Anonymized;
            if (RbNewPatient.Checked) dicomNewObjectType = DicomNewObjectType.NewPatient;
            if (RbNewStudy.Checked) dicomNewObjectType = DicomNewObjectType.NewStudy;
            if (RbNewSeries.Checked) dicomNewObjectType = DicomNewObjectType.NewSeries;
            if (RbNewImage.Checked) dicomNewObjectType = DicomNewObjectType.NewImage;
            DicomServices.UpdateDicomHeaders(TxtFilePath.Text, new DicomTagCollection(), dicomNewObjectType);

            if (CbSiteRemoved.Checked)
                DicomServices.UpdateDicomHeaders(TxtFilePath.Text, new DicomTagCollection(), DicomNewObjectType.SiteDetailsRemoved);

            if (CbRemoveCarerDetails.Checked)
                DicomServices.UpdateDicomHeaders(TxtFilePath.Text, new DicomTagCollection(), DicomNewObjectType.CareProviderDetailsRemoved);

            DicomServices.SendDicomFile(TxtFilePath.Text, "ORTHANC", destinationNode);
        }

        private void BtnBrowseDicomFile_Click(object sender, System.EventArgs e)
        {
            var result = FdLoadDicomFile.ShowDialog();
            if (result == DialogResult.OK) TxtFilePath.Text = FdLoadDicomFile.FileName;
        }
    }
}
