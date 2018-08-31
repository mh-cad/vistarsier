using CAPI.ImageProcessing;
using Microsoft.Win32;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CAPI.LookUpTableGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly string _lutPath;

        public MainWindow()
        {
            InitializeComponent();
            _lutPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LookupTable.bmp");
            if (File.Exists(_lutPath)) File.Delete(_lutPath);
        }

        #region "Event Handlers"
        private void PriorImage_Drop(object sender, DragEventArgs e)
        {
            try
            {
                if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files?.Length != 1) throw new Exception("Only one bmp file allowed");
                var priorFilePath = files[0];
                if (!IsBmp(priorFilePath)) throw new Exception("File is not in bmp format");
                Prior.Source = BitmapImageFromFile(priorFilePath);
                Prior.DataContext = priorFilePath;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error occured!", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
        }
        private void CurrntImage_Drop(object sender, DragEventArgs e)
        {
            try
            {
                if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files?.Length != 1) throw new Exception("Only one bmp file allowed");
                var currentFilePath = files[0];
                if (!IsBmp(currentFilePath)) throw new Exception("File is not in bmp format");
                Current.Source = BitmapImageFromFile(currentFilePath);
                Current.DataContext = currentFilePath;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error occured!", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
        }
        private void ExpectedResult_Drop(object sender, DragEventArgs e)
        {
            try
            {
                if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files?.Length != 1) throw new Exception("Only one bmp file allowed"); ;
                var resultFilePath = files[0];
                if (!IsBmp(resultFilePath)) throw new Exception("File is not in bmp format");
                ExpectedResult.Source = BitmapImageFromFile(resultFilePath);
                ExpectedResult.DataContext = resultFilePath;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error occured!", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
        }

        /// <summary>
        /// Gets prior, current and expected result images to generate lookup table
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GenerateLut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!AllRequiredFilesExist()) return;

                var currentBmp = LoadBmpFromFilepath(Current.DataContext.ToString());
                var priorBmp = LoadBmpFromFilepath(Prior.DataContext.ToString());
                var resultBmp = LoadBmpFromFilepath(ExpectedResult.DataContext.ToString());

                var baseLut = File.Exists(_lutPath) ? LoadBmpFromFilepath(_lutPath) : null;

                var lut = new Nifti().GenerateLookupTable(currentBmp, priorBmp, resultBmp, baseLut);

                using (var fs = new FileStream(_lutPath, FileMode.Create))
                    lut.Save(fs, ImageFormat.Bmp);

                LookUpTable.Source = BitmapImageFromFile(_lutPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error occured!", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
        }
        /// <summary>
        /// Save Lookup Table to disk
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    DefaultExt = "bmp",
                    FileName = "LUT",
                    Filter = "Bmp File|*.bmp",
                    AddExtension = true
                };
                if (saveFileDialog.ShowDialog() == true)
                    File.Copy(_lutPath, saveFileDialog.FileName, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error occured!", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
        }
        private void ClearLut_OnClick_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (File.Exists(_lutPath)) File.Delete(_lutPath);
                LookUpTable.Source = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error occured!", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
        }
        #endregion

        #region "Private methods"
        private static Bitmap LoadBmpFromFilepath(string filepath)
        {
            using (var fs = new FileStream(filepath, FileMode.Open))
                return new Bitmap(fs);
        }
        private static ImageSource BitmapImageFromFile(string filepath)
        {
            using (var fs = new FileStream(filepath, FileMode.Open))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = fs;
                image.EndInit();
                return image;
            }
        }
        private bool AllRequiredFilesExist()
        {
            try
            {
                if (Prior.DataContext == null || !File.Exists(Prior.DataContext.ToString()))
                {
                    MessageBox.Show("Prior image not found", "File not found", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                    return false;
                }
                if (Current.DataContext == null || !File.Exists(Current.DataContext.ToString()))
                {
                    MessageBox.Show("Current image not found", "File not found", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                    return false;
                }
                if (ExpectedResult.DataContext != null && File.Exists(ExpectedResult.DataContext.ToString())) return true;
                MessageBox.Show("Expected result image not found", "File not found", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error occured!", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                return false;
            }
        }
        private static bool IsBmp(string filepath)
        {
            return (Path.HasExtension(filepath) && Path.GetExtension(filepath) == ".bmp");
        }
        #endregion
    }
}
