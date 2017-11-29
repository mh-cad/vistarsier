namespace CAPI.Dicom.Abstraction
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
