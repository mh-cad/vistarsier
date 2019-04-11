using MathNet.Numerics.Statistics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace NiftiView
{
    class HistogramController
    {
        private Chart _chart;
        private NiftiController _niftiControllerA;
        private NiftiController _niftiControllerB;
        private Histogram _histA;
        private Histogram _histB;
        private NumericUpDown _startOfRange;
        private NumericUpDown _endOfRange;
        private DataPoint _highlighted;
        private Color _oldColor;

        private const int HIST_BINS = 256;

        public HistogramController(
            Chart chart, NiftiController niftiControllerA, NiftiController niftiControllerB,
            NumericUpDown startOfRange, NumericUpDown endOfRange)
        {
            _chart = chart;
            _niftiControllerA = niftiControllerA;
            _niftiControllerB = niftiControllerB;
            _startOfRange = startOfRange;
            _endOfRange = endOfRange;

            _startOfRange.ValueChanged += RangeChanged;
            _endOfRange.ValueChanged += RangeChanged;
        }

        public void UpdateData()
        {
            int dataAlen = _niftiControllerA != null && _niftiControllerA.Nifti != null ? _niftiControllerA.Nifti.voxels.Length : 0;
            int dataBlen = _niftiControllerB != null && _niftiControllerB.Nifti != null ? _niftiControllerB.Nifti.voxels.Length : 0;

            double[] dataA = new double[dataAlen];
            double[] dataB = new double[dataBlen];

            if(dataAlen > 0)Array.Copy(_niftiControllerA.Nifti.voxels, dataA, dataA.Length);
            if(dataBlen > 0)Array.Copy(_niftiControllerB.Nifti.voxels, dataB, dataB.Length);

            double upper = 0;
            double lower = 0;

            if (dataAlen <= 0 || dataBlen <= 0)
            {
                if (dataAlen > 0)
                {
                    lower = dataA.Min();
                    upper = dataA.Max();
                }
                else if (dataBlen > 0)
                {
                    lower = dataB.Min();
                    upper = dataB.Max();
                }
            }
            else
            {
                lower = Math.Min(dataA.Min(), dataB.Min());
                upper = Math.Max(dataA.Max(), dataB.Max());
            }


            try { _histA = new Histogram(dataA, HIST_BINS, lower, upper); } catch (Exception) { }
            try { _histB = new Histogram(dataB, HIST_BINS, lower, upper); } catch (Exception) { }

            // Set range box values.
            _startOfRange.Minimum = 0;
            _startOfRange.Maximum = HIST_BINS;
            _startOfRange.Value = _startOfRange.Minimum;
            _endOfRange.Minimum = _startOfRange.Minimum;
            _endOfRange.Maximum = _startOfRange.Maximum;
            _endOfRange.Value = _endOfRange.Maximum;

            UpdateChart();
        }

        public void HighlightValue(NiftiController niftiController, double value)
        {
            int seriesIdx = 0;
            if (niftiController == _niftiControllerA) seriesIdx = 0;
            if (niftiController == _niftiControllerB) seriesIdx = 1;

            Series series = _chart.Series[seriesIdx]; 
            if (_highlighted != null && _oldColor != null) _highlighted.Color = _oldColor;
            DataPointCollection points = series.Points;

            var hist = seriesIdx == 0 ? _histA : _histB; //The bucket should be synced anyway.

            var index = hist.GetBucketIndexOf(value);
            if (index >= _startOfRange.Value && index < _endOfRange.Value)
            {
                _highlighted = points[index - (int)(_startOfRange.Value)];
                _oldColor = series.Color;
                _highlighted.Color = Color.Red;
            }
        }

        private void RangeChanged(object sender, EventArgs e)
        {
            UpdateChart();
        }

        private void UpdateChart()
        {
            _chart.Series.Clear();
            AddHistToSeries(_histA, _chart.Series.Add("A Distriution"));
            AddHistToSeries(_histB, _chart.Series.Add("B Distriution"));
        }

        private void AddHistToSeries(Histogram hist, Series series)
        {
            // Remove space between bars
            series["PointWidth"] = "1";
            // Remove legend label
            series.IsVisibleInLegend = false;

            if (hist == null) return;

            for (int i = (int)_startOfRange.Value; i <= (int)_endOfRange.Value && i < hist.BucketCount; ++i)
            {
                series.Points.Add(hist[i].Count);
                series.Points.Last().AxisLabel = hist[i].LowerBound.ToString();
            }
        }
    }
}
