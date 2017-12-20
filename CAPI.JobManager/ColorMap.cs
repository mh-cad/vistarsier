﻿using CAPI.JobManager.Abstraction;
using System;

namespace CAPI.JobManager
{
    public class ColorMap : IColorMap
    {
        public IntegratedProcessType Type
        {
            get => IntegratedProcessType.ColorMap;
            set { }
        }
        public string Id { get; set; }
        public string Version { get; set; }
        public string[] Parameters { get; set; }

        public event EventHandler<IProcessEventArgument> OnComplete;

        // Constructor
        public ColorMap()
        {
            Id = "4";
            Version = "1";
        }

        public ColorMap(string[] parameters) : this()
        {
            Parameters = parameters;
        }

        public IJob<IRecipe> Run(IJob<IRecipe> jobToBeProcessed)
        {
            var handler = OnComplete;
            handler?.Invoke(this, new ProcessEventArgument(
                $"ColorMap process is completed [Version: {Version}] [Parameters: {string.Join(" ", Parameters)}]"));

            throw new NotImplementedException();
        }
    }
}