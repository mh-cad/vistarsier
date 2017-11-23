namespace CAPI.Dicom.Model
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
