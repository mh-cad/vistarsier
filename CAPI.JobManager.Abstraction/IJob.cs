using CAPI.Dicom.Abstraction;
using System;
using System.Collections.Generic;

namespace CAPI.JobManager.Abstraction
{
    public interface IJob<T>
    {
        IDicomStudy DicomStudyFixed { get; set; }
        IDicomSeries DicomSeriesFixed { get; set; }
        string DicomSeriesFixedFolderPath { get; set; }
        IDicomStudy DicomStudyFloating { get; set; }
        IDicomSeries DicomSeriesFloating { get; set; }
        string DicomSeriesFloatingFolderPath { get; set; }
        IList<IIntegratedProcess> IntegratedProcesses { get; set; }
        IList<IDestination> Destinations { get; set; }

        void Run();

        event EventHandler<ProcessEventArgument> OnEachProcessCompleted;
    }
}