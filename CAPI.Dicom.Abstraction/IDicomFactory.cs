using System;

namespace CAPI.Dicom.Abstraction
{
    public interface IDicomFactory
    {
        IDicomNode CreateDicomNode();
        IDicomNode CreateDicomNode(string logicalName, string aeTitle, string ipAddress, int port);
        IDicomTag CreateDicomTag(string name, uint tagValue, TagType dicomTagType, Type valueType);
        IDicomTagCollection CreateDicomTagCollection();
        IDicomServices CreateDicomServices(IDicomConfig config);
        IDicomStudy CreateStudy();
        IDicomSeries CreateDicomSeries();
        IDicomImage CreateDicomImage();
        IDicomImage CreateDicomImage(string imageUid);
    }
}
