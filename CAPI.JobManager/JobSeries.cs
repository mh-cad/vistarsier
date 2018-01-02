﻿using CAPI.Dicom.Abstraction;
using CAPI.JobManager.Abstraction;
using System.Collections.Generic;

namespace CAPI.JobManager
{
    public class JobSeries : IJobSeries
    {
        public IDicomStudy ParentDicomStudy { get; set; }
        public string BmpFolderPath { get; set; }
        public string DicomFolderPath { get; set; }
        public string HdrFileFullPath { get; set; }
        public string NiiFileFullPath { get; set; }
        public ICollection<IntegratedProcessType> CompletedProcesses { get; set; }

        public JobSeries()
        {
            CompletedProcesses = new List<IntegratedProcessType>();
        }
    }
}