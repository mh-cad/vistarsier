﻿using CAPI.ImageProcessing.Abstraction;
using MathNet.Numerics.Statistics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Unity;
using Unity.log4net;

namespace NiftiView
{
    class NiftiController
    {
        // Properties
        public SliceType SliceType
        {
            get { return _sliceType; }
            set { _sliceType = value; DrawSlice(_currentSlice, Nifti); }
        }
        public int CurrentSlice
        {
            get { return _currentSlice; }
            set { _currentSlice = value; DrawSlice(_currentSlice, Nifti); }
        }
        public HistogramController HistogramController { get; set; }

        public INifti Nifti { get; private set; }

        // Main contols
        private PictureBox _picBox;
        private SliceType _sliceType = SliceType.Axial;
        private int _currentSlice;

        // Scale data
        private decimal _scaleFactor = 1;
        private decimal _xPad = 0;
        private decimal _yPad = 0;

        // Outputs
        private Label _lblMouseOut;

        /// <summary>
        /// Constructor for NiftiController
        /// </summary>
        /// <param name="picBox">Picture box to display current slice</param>
        /// <param name="menu">Menu item to open file / change view</param>
        /// <param name="chartSeries">Chart series to display histogram</param>
        public NiftiController(PictureBox picBox, Label lblMouseOut)
        {
            _picBox = picBox;
            _lblMouseOut = lblMouseOut;
            SliceType = SliceType.Axial;
        }

        public void LoadNiftiFile(string fileName)
        {
            Nifti = InitNifti();
            Nifti.ReadNifti(fileName);

            Nifti.GetDimensions(_sliceType, out int width, out int height, out int nSlices);
            _currentSlice = nSlices / 2 + 1;

            DrawSlice(_currentSlice, Nifti);

            //_chartSeries.Points.Clear();
            ////chart1.Series.Remove(chart1.Series.First());
            //var s = chart1.Series.Add("Distribution");

            // Add picture listener
            _picBox.MouseMove += MouseOverImage;
            _picBox.Resize += PictureRezided;

            HistogramController?.UpdateData();
            _picBox.Refresh();
            RecalculateScale();
        }

        private void DrawSlice(int slice, INifti nifti)
        {
            if (nifti == null) return;

            Bitmap bmp = nifti.GetSlice(slice, _sliceType);

            _picBox.Image = bmp;
            _picBox.Refresh();
        }



        private void RecalculateScale()
        {
            var gu = GraphicsUnit.Pixel;
            var boundsPic = _picBox.Bounds;
            var boundsImg = _picBox.Image.GetBounds(ref gu);

            // We're assuming that the sizemode is on zoom. Otherwise our calculations will be wrong. 
            // This is only to stop code changes and shouldn't change at runtime.
            if (_picBox.SizeMode != PictureBoxSizeMode.Zoom)
            {
                throw new NotImplementedException("We can only translate points for Zoom SizeMode");
            }

            // Calculate the width/height ratios of both the image and the picbox
            decimal ratioPic = (decimal)boundsPic.Width / (decimal)boundsPic.Height;
            decimal ratioImg = (decimal)boundsImg.Width / (decimal)boundsImg.Height;

            // If the ratio is higher for the image we are constrained by height, otherwise we're constrained by width
            bool isHeightConstrained = ratioPic > ratioImg;

            // Get the true height and width of the image within the picbox
            decimal height = 0;
            decimal width = 0;

            // We also need to check on the x and y offsets since there is padding.


            if (isHeightConstrained)
            {
                height = boundsPic.Height;
                width = height * ratioImg;
            }
            else
            {
                width = boundsPic.Width;
                height = boundsPic.Width / ratioImg;
            }

            _xPad = (boundsPic.Width - width) / 2;
            _yPad = (boundsPic.Height - height) / 2;

            // Check that the ratios are correct.
            ratioPic = width / height;
            // We're just going to throw exceptions in lieu of tests for the moment.
            // TODO: Move this to a unit test?
            if (ratioPic != ratioImg) throw new Exception("Ratio calculation incorrect.");

            // Great now we know the scale.
            _scaleFactor = width / (decimal)boundsImg.Width;
        }

        private INifti InitNifti()
        {
            var container = (UnityContainer)new UnityContainer()
                .AddNewExtension<Log4NetExtension>();
            container.RegisterType<INifti, CAPI.ImageProcessing.Nifti>();

            return container.Resolve<INifti>();
            
        }

        private void MouseOverImage(object sender, EventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs)e;
            Point coordinates = me.Location;
            coordinates = TranslateMouseToImage(coordinates);
            var value = Nifti.GetValue(coordinates.X, coordinates.Y, _currentSlice, _sliceType);

            var outText = $@"Current voxel: ({coordinates.X}, {coordinates.Y}, {_currentSlice})" + Environment.NewLine;
            outText += $@"Value: {value}";
            _lblMouseOut.Text = outText;


            // Highlight chart.
            HistogramController?.HighlightValue(this, value);
        }

        private Point TranslateMouseToImage(Point coordinates)
        {
            var gu = GraphicsUnit.Pixel;
            var boundsImg = _picBox.Image.GetBounds(ref gu);

            // Translate the mouse (picbox) coordinates to the image coordinates. Making sure we're not outside the range.
            // Where 0 <= translated <= image_size
            var translatedX = Math.Max(0, Math.Min((coordinates.X - _xPad) / _scaleFactor, (decimal)boundsImg.Width));
            var translatedY = Math.Max(0, Math.Min((coordinates.Y - _yPad) / _scaleFactor, (decimal)boundsImg.Height));

            // Cast back to int point now that we've done all out calculations.
            return new Point((int)translatedX, (int)translatedY);
        }

        private void PictureRezided(object sender, EventArgs e)
        {
            RecalculateScale();
        }
    }
}
