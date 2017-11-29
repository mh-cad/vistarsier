using System;

namespace CAPI.Dicom.Abstraction
{
    public interface IDicomTag
    {
        string[] Values { get; set; }
        TagType DicomTagType { get; }

        string GetName();
        uint GetTagValue();
        Type GetValueType();
    }
}