using CAPI.Dicom.Abstraction;
using CAPI.JobManager.Abstraction;
using System;
using System.Collections.Generic;

namespace CAPI.JobManager
{
    public class Job<T> : IJob<T>
    {
        private readonly IJobManagerFactory _jobManagerFactory;
        public IDicomStudy DicomStudyUnderFocus { get; set; }
        public IDicomStudy DicomStudyBeingComparedTo { get; set; }
        public IList<IIntegratedProcess> IntegratedProcesses { get; set; }
        public IList<IDestination> Destinations { get; set; }

        public Job(IJobManagerFactory jobManagerFactory)
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
                        RunProcess(_jobManagerFactory
                            .CreateExtractBrinSurfaceIntegratedProcess(
                                integratedProcess.Version, integratedProcess.Parameters));
                        break;
                    case IntegratedProcessType.Registeration:
                        RunProcess(_jobManagerFactory
                            .CreateRegistrationIntegratedProcess(
                                integratedProcess.Version, integratedProcess.Parameters));
                        break;
                    case IntegratedProcessType.TakeDifference:
                        RunProcess(_jobManagerFactory
                            .CreateTakeDifferenceIntegratedProcess(
                                integratedProcess.Version, integratedProcess.Parameters));
                        break;
                    case IntegratedProcessType.ColorMap:
                        RunProcess(_jobManagerFactory
                            .CreateColorMapIntegratedProcess(
                                integratedProcess.Version, integratedProcess.Parameters));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void Process_OnComplete(object sender, ProcessEventArgument e)
        {
            OnEachProcessCompleted?.Invoke(sender, e);
        }

        private void RunProcess(IIntegratedProcess process)
        {
            process.OnComplete += Process_OnComplete;
            process.Run();
        }

        public event EventHandler<ProcessEventArgument> OnEachProcessCompleted;
    }
}