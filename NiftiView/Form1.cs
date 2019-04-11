using CAPI.Common.Config;
using CAPI.ImageProcessing;
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
using System.Diagnostics;

namespace NiftiView
{
    public partial class Form1 : Form
    {
        //////////////////////
        private NiftiController _niftiControllerA;
        private NiftiController _niftiControllerB;
        private HistogramController _histogramController;
        private int _currentSlice;

        private TextBox _outText;

        public Form1()
        {
            InitializeComponent();
            lblMouseA.Parent = pictureBoxA;
            lblMouseB.Parent = pictureBoxB;
            


            // Setup chart
            chart1.ChartAreas[0].AxisX.MajorGrid.LineWidth = 0;
            chart1.ChartAreas[0].AxisY.MajorGrid.LineWidth = 0;
            chart1.Series.Remove(chart1.Series.First());

            _niftiControllerA = new NiftiController(pictureBoxA, lblMouseA);
            _niftiControllerB = new NiftiController(pictureBoxB, lblMouseB);

            _histogramController = new HistogramController(chart1, _niftiControllerA, _niftiControllerB, sor, eor);
            _niftiControllerA.HistogramController = _histogramController;
            _niftiControllerB.HistogramController = _histogramController;

            _niftiControllerA.FileLoad += (sender, e) => { lblAFileName.Text = ((NiftiController.FileLoadEventArgs)e).FileName; };
            _niftiControllerB.FileLoad += (sender, e) => { lblFileNameB.Text = ((NiftiController.FileLoadEventArgs)e).FileName; };

            this.KeyDown += KeyListener;
            this.MouseWheel += WheelListener;

        }

        private void WheelListener(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0) NextSlice();
            else if (e.Delta <= 0) PrevSlice();
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
            else if (keyE.KeyCode == Keys.W) NextSlice();
            else if (keyE.KeyCode == Keys.S) PrevSlice();
        }

        private void NextSlice()
        {
            _currentSlice = Math.Max(0, _currentSlice - 1);

            _niftiControllerA.CurrentSlice = _currentSlice;
            _niftiControllerB.CurrentSlice = _currentSlice;
        }

        private void PrevSlice()
        {
            int nSlicesA = 0;
            int nSlicesB = 0;

            _niftiControllerA.Nifti?.GetDimensions(_niftiControllerA.SliceType, out int widthA, out int heightA, out nSlicesA);
            _niftiControllerB.Nifti?.GetDimensions(_niftiControllerA.SliceType, out int widthB, out int heightB, out nSlicesB);
            var maxSlices = Math.Min(nSlicesA - 1, nSlicesB - 1);
            _currentSlice = Math.Min(_currentSlice + 1, maxSlices);

            _niftiControllerA.CurrentSlice = _currentSlice;
            _niftiControllerB.CurrentSlice = _currentSlice;
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


        delegate INifti ProcessNifti(INifti input, DataReceivedEventHandler handler);
        delegate INifti ProcessNiftis(INifti input, INifti input2, DataReceivedEventHandler handler);

        private void StartProcessing(NiftiController controller, string description, ProcessNifti func)
        {
            INifti nifti;

            Form outputForm = new Form();
            outputForm.Text = description;
            _outText = new TextBox
            {
                Parent = outputForm,
                Dock = DockStyle.Fill,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                MinimumSize = new Size(300, 300),
                Text = "Processing..." + Environment.NewLine,
                Multiline = true,
                BackColor = Color.Black,
                ForeColor = Color.LimeGreen,
            };


            outputForm.Show();

            var task1 = Task.Run(() =>
            {
                nifti =
                func(
                    controller.Nifti,
                    (s, e2) => {
                        AppendOutput(e2.Data);
                    });

                return nifti;
            }).ContinueWith(t => { controller.Nifti = t.Result; }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void StartProcessing(NiftiController floating, NiftiController reference, string description, ProcessNiftis func)
        {
            INifti nifti;

            Form outputForm = new Form();
            outputForm.Text = description;
            _outText = new TextBox
            {
                Parent = outputForm,
                Dock = DockStyle.Fill,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                MinimumSize = new Size(300, 300),
                Text = "Processing..." + Environment.NewLine,
                Multiline = true,
                BackColor = Color.Black,
                ForeColor = Color.LimeGreen,
            };


            outputForm.Show();

            var task1 = Task.Run(() =>
            {
                nifti =
                func(
                    floating.Nifti,
                    reference.Nifti,
                    (s, e2) => {
                        AppendOutput(e2.Data);
                    });

                return nifti;
            }).ContinueWith(t => { floating.Nifti = t.Result; }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        delegate void AppendOutputCallback(string text);

        private void AppendOutput(string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.lblcurrentPoint.InvokeRequired)
            {
                AppendOutputCallback d = new AppendOutputCallback(AppendOutput);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this._outText.AppendText(text + Environment.NewLine);
            }
        }

        private void aNTSN4BiasFieldCorrectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StartProcessing(_niftiControllerA, "N4BiasCorrection on A", BiasCorrection.AntsN4);
        }


        private void aNTSN4BiasFieldCorrectionToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            StartProcessing(_niftiControllerB, "N4BiasCorrection on B", BiasCorrection.AntsN4);
        }

        private void brainSuiteBrainExtractionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StartProcessing(_niftiControllerA, "Brain Extration on A", BrainExtraction.BrainSuiteBSE);
        }

        private void brainSuiteBrainExtractionToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            StartProcessing(_niftiControllerB, "Brain Extraction on B", BrainExtraction.BrainSuiteBSE);
        }

        private void cMTKRegistrationResliceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StartProcessing(_niftiControllerA, _niftiControllerB, "Registering A (floating) to B (fixed)", Registration.CMTKRegistration);
        }

        private void cMTKRegistrationResliceToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            StartProcessing(_niftiControllerB, _niftiControllerA, "Registering B (floating) to A (fixed)", Registration.CMTKRegistration);
        }

        private void subtractBToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var niftiA = _niftiControllerA.Nifti;
            var niftiB = _niftiControllerB.Nifti;

            float[] subMap = new float[niftiA.voxels.Length];

            // Create submap
            for(int i = 0; i < niftiA.voxels.Length && i < niftiB.voxels.Length; ++i)
            {
                subMap[i] = niftiA.voxels[i] - niftiB.voxels[i];
            }

            // Normalise to color range.
            var max = subMap.Max();
            var min = subMap.Min();
            var range = max - min;
            var scale = 255 / range;


            for (int i = 0; i < subMap.Length; ++i)
            {
                subMap[i] = subMap[i] * scale;
            }

            niftiA.voxels = subMap;
            niftiA.RecalcHeaderMinMax();
            // Apply
            _niftiControllerA.Nifti = niftiA;
        }

        private void compareBToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _niftiControllerA.Nifti = Compare.CompareMSLesionIncrease(_niftiControllerA.Nifti, _niftiControllerB.Nifti);
            _niftiControllerB.Overlay = _niftiControllerA.Nifti;
            //var niftiA = _niftiControllerA.Nifti;
            //var niftiB = _niftiControllerB.Nifti;
            //var niftiC = compare(niftiA, niftiB, 0, float.MaxValue);
            
            //niftiC.ColorMap = ColorMaps.RedScale();
            //_niftiControllerA.Nifti = niftiC;
        }

        private INifti compare(INifti niftiA, INifti niftiB, float lowerBound, float upperBound)
        {
            INifti niftiC = niftiA;

            if (niftiA.voxels.Length != niftiB.voxels.Length) throw new Exception("A and B don't match size");

            double[] vAd = new double[niftiA.voxels.Length];
            double[] vBd = new double[niftiA.voxels.Length];
            double[] sub = new double[niftiA.voxels.Length];


            for (int i = 0; i < niftiA.voxels.Length; ++i)
            {
                vAd[i] = (double)niftiA.voxels[i];
                vBd[i] = (double)niftiB.voxels[i];
                //if (sub[i] < 0) sub[i] = 0;
            }



            for (int i = 0; i < niftiA.voxels.Length; ++i)
            {
                sub[i] = vAd[i] - vBd[i];
                if (sub[i] < lowerBound) sub[i] = lowerBound;
                if (sub[i] > upperBound) sub[i] = upperBound;
                if (vAd[i] < 80) sub[i] = 0;
                if (vBd[i] < 80) sub[i] = 0;
            }

            for (int i = 0; i < vAd.Length; ++i)
            {
                
                niftiC.voxels[i] = (float)sub[i];
            }

            niftiC.RecalcHeaderMinMax();

            return niftiC;
        }

        public static void Normalize(float[] array, float mean, float stdDev)
        {
            var currentMean = array.Where(val => val > 10).Mean();
            var currentStdDev = array.Where(val => val > 10).StandardDeviation();

            if (Math.Abs(currentStdDev) < 0.000001) return;

            for (var i = 0; i < array.Length; i++)
            {
                array[i] = (float)((array[i] - currentMean) / currentStdDev) * stdDev + mean;
            }
                
        }

        private INifti setRange(INifti nifti, float start, float end)
        {
            var vox = nifti.voxels;
            var max = vox.Max();
            var min = vox.Min();
            var range = max - min;

            var b = start - min;
            float nurange = end - start;
            var scale = nurange / range;

            for (int i = 0; i < vox.Length; ++i)
            {
                vox[i] = scale * vox[i] + b;
                if (vox[i] < start) vox[i] = start;
                else if (vox[i] > end) vox[i] = end;
            }

            nifti.voxels = vox;
            nifti.RecalcHeaderMinMax();
            return nifti;
        }

        private void normalizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _niftiControllerA.Nifti = Normalization.ZNormalize(_niftiControllerA.Nifti, _niftiControllerB.Nifti);
        }

        private void compareDecreaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _niftiControllerA.Nifti = Compare.CompareMSLesionDecrease(_niftiControllerA.Nifti, _niftiControllerB.Nifti);
            _niftiControllerB.Overlay = _niftiControllerA.Nifti;
            //var niftiA = _niftiControllerA.Nifti;
            //var niftiB = _niftiControllerB.Nifti;
            //var niftiC = compare(niftiA, niftiB, -100, 0);

            //for (int i = 0; i < niftiC.voxels.Length; ++i)
            //{
            //    if (Math.Abs(niftiC.voxels[i]) < 30) niftiC.voxels[i] = 0;
            //}

            ////niftiC.ColorMap = ColorMaps.GreenMask();
            //niftiC.ColorMap = ColorMaps.ReverseGreenScale();
            //niftiC.voxels = niftiC.voxels;
            //_niftiControllerA.Nifti = niftiC;
        }
    }
}
