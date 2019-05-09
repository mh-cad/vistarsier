using System;

namespace CAPI.Config
{
    public class ImagePaths
    {
        public ImagePaths()
        {
            ImageRepositoryPath = AppDomain.CurrentDomain.BaseDirectory + "ImageRepository/";
            ResultsDicomSeriesDescription = "CAPI Modified Signal";
            PriorReslicedDicomSeriesDescription = "CAPI Prior Resliced";
        }

        public string ImageRepositoryPath { get; set; }
        public string ResultsDicomSeriesDescription { get; set; }
        public string PriorReslicedDicomSeriesDescription { get; set; }
    }
}
