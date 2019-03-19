using CAPI.Common.Config;
using CAPI.ImageProcessing.Abstraction;
using Unity;
using Unity.log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MathNet.Numerics.Statistics;
using System.Windows.Forms.DataVisualization.Charting;

namespace NiftiView
{
    public partial class Form1 : Form
    {
        //////////////////////
        private NiftiController _niftiControllerA;
        private NiftiController _niftiControllerB;
        private HistogramController _histogramController;
        private int _currentSlice;

        public Form1()
        {
            InitializeComponent();
            
            // Setup chart
            chart1.ChartAreas[0].AxisX.MajorGrid.LineWidth = 0;
            chart1.ChartAreas[0].AxisY.MajorGrid.LineWidth = 0;
            chart1.Series.Remove(chart1.Series.First());

            _niftiControllerA = new NiftiController(pictureBoxA, lblcurrentPoint);
            _niftiControllerB = new NiftiController(pictureBoxB, lblcurrentPoint);

            _histogramController = new HistogramController(chart1, _niftiControllerA, _niftiControllerB, sor, eor);
            _niftiControllerA.HistogramController = _histogramController;
            _niftiControllerB.HistogramController = _histogramController;

            this.KeyDown += KeyListener;
        }

        // EVENTS

        private void KeyListener(object sender, EventArgs e)
        {
            //var mouseE = (MouseEventArgs)e;

            //lblcurrentPoint.Text = $@"Mouse scroll = {mouseE.Delta}";
            var keyE = (KeyEventArgs)e;

            if (keyE.KeyCode == Keys.Q) sor.UpButton();
            else if (keyE.KeyCode == Keys.A) sor.DownButton();
            else if (keyE.KeyCode == Keys.E) eor.UpButton();
            else if (keyE.KeyCode == Keys.D) eor.DownButton();
            try
            {

                if (keyE.KeyCode == Keys.W)
                {
                    _currentSlice = Math.Max(0, _currentSlice - 1);
                }
        
                else if (keyE.KeyCode == Keys.S)
                {
                    _niftiControllerA.Nifti.GetDimensions(_niftiControllerA.SliceType, out int widthA, out int heightA, out int nSlicesA);
                    _niftiControllerB.Nifti.GetDimensions(_niftiControllerA.SliceType, out int widthB, out int heightB, out int nSlicesB);
                    var maxSlices = Math.Min(nSlicesA - 1, nSlicesB - 1);
                    _currentSlice = Math.Min(_currentSlice + 1, maxSlices);
                }

                _niftiControllerA.CurrentSlice = _currentSlice;
                _niftiControllerB.CurrentSlice = _currentSlice;

            } catch (Exception ex)
            {

            }
        }

        private void axialToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _niftiControllerA.SliceType = SliceType.Axial;
            _niftiControllerB.SliceType = SliceType.Axial;
        }

        private void coronalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _niftiControllerA.SliceType = SliceType.Coronal;
            _niftiControllerB.SliceType = SliceType.Coronal;
        }

        private void sagittalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _niftiControllerA.SliceType = SliceType.Sagittal;
            _niftiControllerB.SliceType = SliceType.Sagittal;
        }

        private void mnuFileOpenA_Click(object sender, EventArgs e)
        {
            var fileDialog = new OpenFileDialog();
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                _niftiControllerA.LoadNiftiFile(fileDialog.FileName);
            }
        }

        private void mnuFileOpenB_Click(object sender, EventArgs e)
        {
            var fileDialog = new OpenFileDialog();
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                _niftiControllerB.LoadNiftiFile(fileDialog.FileName);
            }
        }
    }
}
