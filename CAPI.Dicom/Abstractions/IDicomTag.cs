using System;

namespace VisTarsier.Dicom.Abstractions
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