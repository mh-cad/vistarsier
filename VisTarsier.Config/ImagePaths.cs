using System;

namespace VisTarsier.Config
{
    public class ImagePaths
    {
        public ImagePaths()
        {
            ImageRepositoryPath = AppDomain.CurrentDomain.BaseDirectory + "ImageRepository/";
            ResultsDicomSeriesDescription = "OVT Modified Signal";
            PriorReslicedDicomSeriesDescription = "OVT Prior Resliced";
        }

        public string ImageRepositoryPath { get; set; }
        public string ResultsDicomSeriesDescription { get; set; }
        public string PriorReslicedDicomSeriesDescription { get; set; }
    }
}
