﻿using System.IO;

namespace CAPI.Domain.Model
{
    public class SeriesNii : ISeriesNii
    {
        public string Description { get; set; }
        public string FileFullPath { get; set; }
        public string FolderPath { get; set; }
        public int NumberOfImages { get; set; }

        public SeriesNii(string name, string fileFullPath, int numberOfImages)
        {
            Description = name;
            FileFullPath = fileFullPath;
            NumberOfImages = numberOfImages;
            GetFolderPath();
        }

        public void GetFolderPath()
        {
            FolderPath = Path.GetDirectoryName(FileFullPath);
        }
    }
}
