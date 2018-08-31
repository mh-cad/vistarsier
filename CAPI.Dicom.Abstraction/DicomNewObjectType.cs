namespace CAPI.Dicom.Abstractions
{
    public enum DicomNewObjectType
    {
        NoChange,
        NewStudy,
        NewSeries,
        NewImage,
        NewPatient,
        Anonymized,
        SiteDetailsRemoved,
        CareProviderDetailsRemoved
    }
}
