using CAPI.Dicom.Abstraction;
using CAPI.JobManager.Abstraction;
using System;
using System.Collections.Generic;

namespace CAPI.JobManager
{
    public class Job<T> : IJob<T>
    {
        private readonly IJobManagerFactory _jobManagerFactory;

        public IDicomStudy DicomStudyFixed { get; set; }
        public IDicomStudy DicomStudyFloating { get; set; }
        public IList<IIntegratedProcess> IntegratedProcesses { get; set; }
        public IList<IDestination> Destinations { get; set; }

        public Job(IJobManagerFactory jobManagerFactory, IDicomServices dicomServices)
        {
            _jobManagerFactory = jobManagerFactory;
        }

        public void Run()
        {
            foreach (var integratedProcess in IntegratedProcesses)
            {
                switch (integratedProcess.Type)
                {
                    case IntegratedProcessType.ExtractBrainSurface:
                        RunExtractBrainSurfaceProcess(integratedProcess);
                        break;
                    case IntegratedProcessType.Registeration:
                        //RunProcess(_jobManagerFactory
                        //    .CreateRegistrationIntegratedProcess(
                        //        integratedProcess.Version, integratedProcess.Parameters));
                        break;
                    case IntegratedProcessType.TakeDifference:
                        //RunProcess(_jobManagerFactory
                        //    .CreateTakeDifferenceIntegratedProcess(
                        //        integratedProcess.Version, integratedProcess.Parameters));
                        break;
                    case IntegratedProcessType.ColorMap:
                        //RunProcess(_jobManagerFactory
                        //    .CreateColorMapIntegratedProcess(
                        //        integratedProcess.Version, integratedProcess.Parameters));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

        }

        private void RunExtractBrainSurfaceProcess(IIntegratedProcess integratedProcess)
        {
            //DicomStudyFixedFolderPath = _dicomServices.get

            var extractBrainSurfaceProcess = _jobManagerFactory
                .CreateExtractBrinSurfaceIntegratedProcess(
                integratedProcess.Version, integratedProcess.Parameters);

            extractBrainSurfaceProcess.OnComplete += Process_OnComplete;

            //extractBrainSurfaceProcess.Run();
        }

        private void Process_OnComplete(object sender, ProcessEventArgument e)
        {
            OnEachProcessCompleted?.Invoke(sender, e);
        }

        public event EventHandler<ProcessEventArgument> OnEachProcessCompleted;
    }
}