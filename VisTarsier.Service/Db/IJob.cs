﻿using System;

namespace VisTarsier.Service.Db
{
    public interface IJob
    {
        long Id { get; set; }
        string SourceAet { get; set; }
        string PatientId { get; set; }
        string PatientFullName { get; set; }
        string PatientBirthDate { get; set; }
        string CurrentAccession { get; set; }
        string PriorAccession { get; set; }
        string DefaultDestination { get; set; }
        bool ExtractBrain { get; set; }
        bool Register { get; set; }
        //string RegistrationData { get; set; }
        string ReferenceSeries { get; set; }
        bool BiasFieldCorrection { get; set; }
        string Status { get; set; }
        DateTime Start { get; set; }
        DateTime End { get; set; }
        string CurrentSeriesDicomFolder { get; set; }
        string PriorSeriesDicomFolder { get; set; }
        string ReferenceSeriesDicomFolder { get; set; }
        string ResultSeriesDicomFolder { get; set; }
        //IJobResult[] Results { get; set; }
        string PriorReslicedSeriesDicomFolder { get; set; }
        string ProcessingFolder { get; set; }

        void Process();
        string GetStudyIdFromReferenceSeries();
        string GetSeriesIdFromReferenceSeries();
        void WriteStudyAndSeriesIdsToReferenceSeries(string studyId, string seriesId);
    }
}