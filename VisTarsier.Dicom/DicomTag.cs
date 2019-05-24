using System;
using VisTarsier.Dicom.Abstractions;

namespace VisTarsier.Dicom
{
    public class DicomTag : IDicomTag
    {
        private string Name { get; }
        private uint TagValue { get; }
        public string[] Values { get; set; }
        private Type ValueType { get; }
        public TagType DicomTagType { get; }
        
        public DicomTag(string name, uint tagValue, TagType dicomTagType, Type valueType)
        {
            Name = name;
            TagValue = tagValue;
            DicomTagType = dicomTagType;
            ValueType = valueType;
        }

        public string GetName()
        {
            return Name;
        }
        public uint GetTagValue()
        {
            return TagValue;
        }
        public Type GetValueType()
        {
            return ValueType;
        }
    }
}