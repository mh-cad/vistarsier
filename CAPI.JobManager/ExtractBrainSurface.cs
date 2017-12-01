﻿using CAPI.Dicom.Abstraction;
using CAPI.JobManager.Abstraction;
using System;

namespace CAPI.JobManager
{
    public class ExtractBrainSurface : IExtractBrainSurface
    {
        private string[] _parameters;

        public IntegratedProcessType Type
        {
            get => IntegratedProcessType.ExtractBrainSurface;
            set => value = IntegratedProcessType.ExtractBrainSurface;
        }
        public string Id { get; set; }
        public string Version { get; set; }
        public string[] Parameters { get; set; }
        public IDicomSeries DicomSeries { get; set; }
        public event EventHandler<ProcessEventArgument> OnComplete;

        // Constructor
        public ExtractBrainSurface(string[] parameters)
        {
            Id = "1";
            Version = "1";
            Init(parameters);
        }

        public void Init()
        {
            throw new System.NotImplementedException();
        }

        public void Init(params string[] parameters)
        {
            _parameters = parameters;
        }

        public void Run()
        {
            var handler = OnComplete;
            handler?.Invoke(this, new ProcessEventArgument("Brain Mask Extraction is completed"));
        }
    }
}