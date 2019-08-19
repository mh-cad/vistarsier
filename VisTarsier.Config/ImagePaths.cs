using System;

namespace VisTarsier.Config
{
    public class ImagePaths
    {
        public ImagePaths()
        {
            ImageRepositoryPath = AppDomain.CurrentDomain.BaseDirectory + "ImageRepository/";
            ResultsDicomSeriesDescription = "VisTarsier";
        }

        public string ImageRepositoryPath { get; set; }
        public string ResultsDicomSeriesDescription { get; set; }
    }
}
