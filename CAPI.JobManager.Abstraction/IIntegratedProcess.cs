﻿using System;

namespace CAPI.JobManager.Abstraction
{
    public interface IIntegratedProcess
    {
        IntegratedProcessType Type { get; set; }
        string Id { get; set; }
        string Version { get; set; }
        string[] Parameters { get; set; }

        event EventHandler<ProcessEventArgument> OnComplete;
    }
}