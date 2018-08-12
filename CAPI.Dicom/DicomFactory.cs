﻿using CAPI.Common.Abstractions.Services;
using CAPI.Dicom.Abstraction;
using CAPI.Dicom.Model;
using System;

namespace CAPI.Dicom
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public partial class DicomFactory : IDicomFactory
    {
        public IDicomNode CreateDicomNode()
        {
            return new DicomNode();
        }
        public IDicomNode CreateDicomNode(string logicalName, string aeTitle, string ipAddress, int port)
        {
            return new DicomNode(logicalName, aeTitle, ipAddress, port);
        }
        public IDicomTag CreateDicomTag(string name, uint tagValue, TagType dicomTagType, Type valueType)
        {
            return new DicomTag(name, tagValue, dicomTagType, valueType);
        }
        public IDicomTagCollection CreateDicomTagCollection()
        {
            return new DicomTagCollection();
        }
        public IDicomServices CreateDicomServices(IDicomConfig config, IFileSystem fileSystem, IProcessBuilder processBuilder)
        {
            return new DicomServices(config, fileSystem, processBuilder);
        }
        public IDicomStudy CreateStudy()
        {
            return new DicomStudy();
        }
        public IDicomSeries CreateDicomSeries()
        {
            return new DicomSeries();
        }
        public IDicomImage CreateDicomImage()
        {
            return new DicomImage();
        }
        public IDicomImage CreateDicomImage(string imageUid)
        {
            return new DicomImage { ImageUid = imageUid };
        }
        public IDicomConfig CreateDicomConfig()
        {
            return new DicomConfig();
        }
    }
}
