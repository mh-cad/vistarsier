using System;

namespace VisTarsier.Config
{
    public class ImagePaths
    {
        public ImagePaths()
        {
            ImageRepositoryPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../img/");
            ResultsDicomSeriesDescription = "VisTarsier";
        }

        public string ImageRepositoryPath { get; set; }
        public string ResultsDicomSeriesDescription { get; set; }
    }
}
