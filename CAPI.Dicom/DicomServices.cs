using CAPI.Dicom.Abstractions;
using CAPI.Dicom.Model;
using CAPI.General.Abstractions.Services;
using ClearCanvas.Dicom;
using ClearCanvas.Dicom.Iod;
using ClearCanvas.Dicom.Iod.Iods;
using ClearCanvas.Dicom.Network.Scu;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using StorageScp = ClearCanvas.Dicom.Samples.StorageScp;

namespace CAPI.Dicom
{
    public class DicomServices : IDicomServices
    {
        private readonly IFileSystem _fileSystem;
        private readonly IProcessBuilder _processBuilder;
        private readonly ILog _log;
        private readonly IDicomConfig _config;

        public DicomServices(IDicomConfig config, IFileSystem fileSystem, IProcessBuilder processBuilder, ILog log)
        {
            _fileSystem = fileSystem;
            _processBuilder = processBuilder;
            _log = log;
            _config = config;
        }

        public void SendDicomFile(string filepath, string localAe, IDicomNode destinationDicomNode)
        {
            using (var scu = new StorageScu(localAe, destinationDicomNode.AeTitle, destinationDicomNode.IpAddress, destinationDicomNode.Port))
            {
                scu.ImageStoreCompleted += ScuOnImageStorageCompleted;
                scu.AddFile(filepath);
                scu.Send();
            }
        }

        private static void ScuOnImageStorageCompleted(object sender, ImageStoreEventArgs eventArgs)
        {
        }

        public void UpdateDicomHeaders(
            string filepath, IDicomTagCollection tags, DicomNewObjectType dicomNewObjectType)
        {
            var dcmFile = new DicomFile(filepath);
            dcmFile.Load(filepath);

            switch (dicomNewObjectType)
            {
                case DicomNewObjectType.Anonymized:
                    tags = UpdateUidsForNewStudy(tags);
                    dcmFile = UpdateTags(dcmFile, tags, TagType.Patient, true);
                    dcmFile = UpdateTags(dcmFile, tags, TagType.Study);
                    dcmFile = UpdateTags(dcmFile, tags, TagType.Series);
                    dcmFile = UpdateTags(dcmFile, tags, TagType.Image);
                    break;
                case DicomNewObjectType.SiteDetailsRemoved:
                    tags = UpdateUidsForNewStudy(tags);
                    dcmFile = UpdateTags(dcmFile, tags, TagType.Site, true);
                    break;
                case DicomNewObjectType.CareProviderDetailsRemoved:
                    tags = UpdateUidsForNewStudy(tags);
                    dcmFile = UpdateTags(dcmFile, tags, TagType.CareProvider, true);
                    break;
                case DicomNewObjectType.NewPatient:
                    tags = UpdateUidsForNewStudy(tags);
                    dcmFile = UpdateTags(dcmFile, tags, TagType.Patient);
                    dcmFile = UpdateTags(dcmFile, tags, TagType.Study);
                    dcmFile = UpdateTags(dcmFile, tags, TagType.Series);
                    dcmFile = UpdateTags(dcmFile, tags, TagType.Image);
                    break;
                case DicomNewObjectType.NewStudy:
                    tags = UpdateUidsForNewStudy(tags);
                    dcmFile = UpdateTags(dcmFile, tags, TagType.Study);
                    dcmFile = UpdateTags(dcmFile, tags, TagType.Series);
                    dcmFile = UpdateTags(dcmFile, tags, TagType.Image);
                    break;
                case DicomNewObjectType.NewSeries:
                    tags = UpdateUidsForNewSeries(tags);
                    dcmFile = UpdateTags(dcmFile, tags, TagType.Series);
                    dcmFile = UpdateTags(dcmFile, tags, TagType.Image);
                    break;
                case DicomNewObjectType.NewImage:
                    tags = UpdateUidsForNewImage(tags);
                    dcmFile = UpdateTags(dcmFile, tags, TagType.Image);
                    break;
                case DicomNewObjectType.NoChange:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dicomNewObjectType), dicomNewObjectType, null);
            }
            dcmFile.Save(filepath);
        }

        public void UpdateSeriesHeadersForAllFiles(string[] filesPath, IDicomTagCollection tags)
        {
            tags.SeriesInstanceUid.Values = new[] { GenerateNewSeriesUid() };
            tags.ImageUid.Values = new[] { GenerateNewImageUid() };
            foreach (var filepath in filesPath)
            {
                var dcmFile = new DicomFile(filepath);
                dcmFile.Load(filepath);
                dcmFile = UpdateTags(dcmFile, tags, TagType.Series);
                dcmFile.Save(filepath);
            }
        }

        private static DicomFile UpdateTags(
            DicomFile dcmFile, IDicomTagCollection newTags, TagType tagType, bool overwriteIfNotProvided = false)
        {
            if (newTags == null) return dcmFile;
            newTags.ToList().ForEach(tag =>
            {
                if (tag.DicomTagType == tagType)
                    dcmFile = UpdateTag(dcmFile, tag, overwriteIfNotProvided);
            });
            return dcmFile;
        }
        private static DicomFile UpdateTag(
            DicomFile dcmFile, IDicomTag newTag, bool overwriteIfNotProvided = false)
        {
            if (newTag.Values == null && !overwriteIfNotProvided) return dcmFile;
            var value = newTag.Values != null ? newTag.Values[0] : "";
            return UpdateTag(dcmFile, newTag, value);
        }
        private static DicomFile UpdateTag(
            DicomFile dcmFile, IDicomTag newTag, string value)
        {
            if (newTag.GetValueType() == typeof(string[])) dcmFile.DataSet[newTag.GetTagValue()].Values = new[] { value };
            else if (newTag.GetValueType() == typeof(string)) dcmFile.DataSet[newTag.GetTagValue()].Values = value;
            return dcmFile;
        }

        private IDicomTagCollection UpdateUidsForNewStudy(IDicomTagCollection tags)
        {
            if (tags == null) return null;
            tags.StudyInstanceUid.Values = new[] { GenerateNewStudyUid() };
            tags = UpdateUidsForNewSeries(tags);
            tags = UpdateUidsForNewImage(tags);
            return tags;
        }
        private IDicomTagCollection UpdateUidsForNewSeries(IDicomTagCollection tags)
        {
            if (tags == null) return null;
            tags.SeriesInstanceUid.Values = new[] { GenerateNewSeriesUid() };
            tags = UpdateUidsForNewImage(tags);
            return tags;
        }
        private IDicomTagCollection UpdateUidsForNewImage(IDicomTagCollection tags)
        {
            if (tags == null) return null;
            tags.ImageUid.Values = new[] { GenerateNewImageUid() };
            return tags;
        }

        public IDicomTagCollection GetDicomTags(string filePath)
        {
            var dcmFile = new DicomFile(filePath);
            dcmFile.Load(filePath);
            var tags = new DicomTagCollection();
            var updatedTags = new DicomTagCollection();
            foreach (var tag in tags.ToList())
                updatedTags.SetTagValue(tag.GetTagValue(), dcmFile.DataSet[tag.GetTagValue()].Values);
            return updatedTags;
        }

        public IEnumerable<IDicomStudy> GetStudiesForPatientId(
            string patientId, IDicomNode localNode, IDicomNode remoteNode)
        {
            var query = new StudyQueryIod();
            query.SetCommonTags();
            query.PatientId = patientId;

            return GetStudies(query, localNode, remoteNode);
        }

        public IEnumerable<IDicomStudy> GetStudiesForPatient(
            string patientFullName, string patientBirthDate,
            IDicomNode localNode, IDicomNode sourceNode)
        {
            var query = new StudyQueryIod();
            query.SetCommonTags();
            query.PatientsName = new PersonName(patientFullName);
            query.PatientsBirthDate = DateTime.ParseExact(patientBirthDate, "yyyyMMdd", new DateTimeFormatInfo());

            return GetStudies(query, localNode, sourceNode);
        }

        public IDicomPatient GetPatientIdFromPatientDetails(
            string patientFullname, string patientBirthDate,
            IDicomNode localNode, IDicomNode sourceNode)
        {
            var query = new PatientQueryIod();
            query.SetCommonTags();
            query.PatientsName = new PersonName(patientFullname);
            query.PatientsBirthDate = DateTime.ParseExact(patientBirthDate, "yyyyMMdd", CultureInfo.CurrentCulture);
            var findScu = new PatientRootFindScu();
            var patient = findScu.Find(localNode.AeTitle, sourceNode.AeTitle,
                sourceNode.IpAddress, sourceNode.Port, query).ToList();
            if (!patient.Any()) throw new Exception($"No patient found with name [{patientFullname}] " +
                                                    $"and birth date [{patientBirthDate}]");
            if (patient.Count > 1) throw new Exception($"{patient.Count} patients were found for name [{patientFullname}] " +
                                                    $"and birth date [{patientBirthDate}]");
            return patient.Select(MapToDicomPatient).FirstOrDefault();
        }

        public string GenerateNewStudyUid()
        {
            return $"1.2.826.0.1.3680043.9.7303.1.1.{DateTime.Now:yyyyMMddHHmmssfff}.1";
        }
        public string GenerateNewSeriesUid()
        {
            return $"1.2.826.0.1.3680043.9.7303.1.2.{DateTime.Now:yyyyMMddHHmmssfff}.1";
        }
        public string GenerateNewImageUid()
        {
            return $"1.2.826.0.1.3680043.9.7303.1.3.{DateTime.Now:yyyyMMddHHmmssfff}.1";
        }

        public void ConvertBmpsToDicom(string bmpFolder, string dicomFolder, SliceType sliceType, string dicomHeadersFolder = "", bool matchByFilename = false)
        {
            _fileSystem.DirectoryExistsIfNotCreate(dicomFolder);


            var orderedDicomFiles = new List<string>();
            if (!string.IsNullOrEmpty(dicomHeadersFolder))
            {
                orderedDicomFiles = GetFilesOrderedByInstanceNumber(Directory.GetFiles(dicomHeadersFolder)).ToList();
                if (!DicomFilesInRightOrder(dicomHeadersFolder, sliceType)) // TODO1: Testing
                    orderedDicomFiles = orderedDicomFiles.ToArray().Reverse().ToList();
            }

            var bmpFiles = Directory.GetFiles(bmpFolder);

            if (matchByFilename)
                bmpFiles = MatchBmpFilesWithDicomFiles(bmpFiles, orderedDicomFiles);

            // check if number of dicom and bmp files match
            if (bmpFiles.Length != orderedDicomFiles.Count)
                throw new Exception($"Number of Bmp files and dicom files to read header from don't match {bmpFiles.Length} != {orderedDicomFiles.Count}");

            for (var i = 0; i < bmpFiles.Length; i++)
            {

                var filenameNoExt = Path.GetFileNameWithoutExtension(bmpFiles[i]);
                var dicomFilePath = Path.Combine(dicomFolder, filenameNoExt ?? throw new InvalidOperationException("Failed to get bmp file name during converting bmp files to dicom"));

                ConvertBmpToDicom(bmpFiles[i], dicomFilePath, orderedDicomFiles[i]);
            }
        }

        private static string[] MatchBmpFilesWithDicomFiles(string[] bmpFiles, IEnumerable<string> orderedDicomFiles)
        {
            var orderedBmpFiles = new List<string>();
            foreach (var dicomFile in orderedDicomFiles)
            {
                var dicomFileName = Path.GetFileNameWithoutExtension(dicomFile);
                if (dicomFileName == null) throw new Exception($"Failed to get filename for following path: [{dicomFile}]");
                var matchingBmpFile = bmpFiles
                    .FirstOrDefault(f =>
                    {
                        var bmpFileName = Path.GetFileNameWithoutExtension(f);
                        if (bmpFileName == null) throw new Exception($"Failed to get filename for following path: [{f}]");
                        return bmpFileName.Equals(dicomFileName, StringComparison.CurrentCultureIgnoreCase);
                    });
                orderedBmpFiles.Add(matchingBmpFile);
            }
            return orderedBmpFiles.ToArray();
        }


        private bool DicomFilesInRightOrder(string dicomFolderPath, SliceType sliceType)
        {
            var dicomFileWithLowestInstanceNo = GetDicomFileWithLowestInstanceNumber(dicomFolderPath);
            if (dicomFileWithLowestInstanceNo == null || string.IsNullOrEmpty(dicomFileWithLowestInstanceNo))
                throw new Exception($"Failed to get the dicom file with lowest instance number in folder [{dicomFolderPath}]");
            var headers = GetDicomTags(dicomFileWithLowestInstanceNo);
            if (headers == null) throw new Exception($"Failed to get dicom headers for dicom file [{dicomFileWithLowestInstanceNo}]");
            var imagePosition = headers.ImagePositionPatient.Values;
            if (imagePosition.Length < 3) return true; // disregard

            var slicesFromLeftToRight = float.Parse(imagePosition[0]) > 0;
            var slicesFromBackToFront = float.Parse(imagePosition[1]) > 0;
            var slicesFromBottomToTop = float.Parse(imagePosition[2]) < 0;

            switch (sliceType)
            {
                case SliceType.Sagittal when !slicesFromLeftToRight:
                case SliceType.Coronal when !slicesFromBackToFront:
                case SliceType.Axial when !slicesFromBottomToTop:
                    return false; // So later we reverse the order of files
                default:
                    return true;
            }
        }

        public void ConvertBmpToDicom(string bmpFilepath, string dicomFilePath, string dicomHeadersFilePath = "")
        {
            var arguments = string.Empty;
            if (!string.IsNullOrEmpty(dicomHeadersFilePath))
                arguments = $@"-df {dicomHeadersFilePath} "; // Copy dicom headers from dicom file: -df = dataset file

            arguments += $"-i BMP {bmpFilepath} {dicomFilePath}";

            _processBuilder.CallExecutableFile(_config.Img2DcmFilePath, arguments, "", OutputDataReceivedInProcess, ErrorOccuredInProcess);
        }

        public void ConvertBmpToDicomAndAddToExistingFolder(string bmpFilePath, string dicomFolderPath, string newFileName = "")
        {
            if (string.IsNullOrEmpty(newFileName)) newFileName = Path.GetFileNameWithoutExtension(bmpFilePath);
            if (Directory.GetFiles(dicomFolderPath)
                .Any(f => Path.GetFileName(f).ToLower().Contains(newFileName ?? throw new ArgumentNullException(nameof(newFileName)))))
                newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");

            var dicomFileHighestInstanceNo = GetDicomFileWithHighestInstanceNumber(dicomFolderPath);
            var headers = GetDicomTags(dicomFileHighestInstanceNo);
            var newFilePath = Path.Combine(dicomFolderPath, newFileName ?? throw new ArgumentNullException(nameof(newFileName)));

            ConvertBmpToDicom(bmpFilePath, newFilePath, dicomFileHighestInstanceNo);
            var newFileInstanceNumber = headers.InstanceNumber.Values == null ? 1 : int.Parse(headers.InstanceNumber.Values[0]) + 1;
            headers.InstanceNumber.Values = new[] { newFileInstanceNumber.ToString() };
            UpdateDicomHeaders(newFilePath, headers, DicomNewObjectType.NewImage);
        }

        private string GetDicomFileWithHighestInstanceNumber(string dicomFolderPath)
        {
            var dicomFiles = Directory.GetFiles(dicomFolderPath);
            return dicomFiles.OrderByDescending(f =>
                    GetDicomTags(f).InstanceNumber.Values != null ?
                        int.Parse(GetDicomTags(f).InstanceNumber.Values[0])
                        : 0)
                    .FirstOrDefault();
        }
        private string GetDicomFileWithLowestInstanceNumber(string dicomFolderPath)
        {
            var dicomFiles = Directory.GetFiles(dicomFolderPath);
            return dicomFiles.OrderBy(f =>
                    GetDicomTags(f).InstanceNumber.Values != null ?
                        int.Parse(GetDicomTags(f).InstanceNumber.Values[0])
                        : 0)
                .FirstOrDefault();
        }

        private IEnumerable<string> GetFilesOrderedByInstanceNumber(IEnumerable<string> files)
        {
            return files.OrderBy(f => Convert.ToInt32(GetDicomTags(f).InstanceNumber.Values[0])).ToList();
        }

        private static IDicomPatient MapToDicomPatient(PatientQueryIod patientQueryIod)
        {
            return new DicomPatient
            {
                PatientId = patientQueryIod.PatientId,
                PatientFullName = patientQueryIod.PatientsName,
                PatientBirthDate = patientQueryIod.PatientsBirthDate.Date.ToString("yyyyMMdd")
            };
        }

        private IEnumerable<IDicomStudy> GetStudies(StudyQueryIod query,
            IDicomNode localNode, IDicomNode remoteNode)
        {
            CheckRemoteNodeAvailability(localNode, remoteNode);
            var findScu = new StudyRootFindScu();
            return findScu
                .Find(localNode.AeTitle, remoteNode.AeTitle, remoteNode.IpAddress, remoteNode.Port, query)
                .Select(MapToDicomStudy);
        }

        private static IDicomStudy MapToDicomStudy(StudyQueryIod study)
        {
            return new DicomStudy
            {
                AccessionNumber = study.AccessionNumber,
                StudyDescription = study.StudyDescription,
                StudyInstanceUid = study.StudyInstanceUid,
                StudyDate = study.StudyDate,
                PatientBirthDate = study.PatientsBirthDate,
                PatientId = study.PatientId,
                PatientsName = study.PatientsName,
                PatientsSex = study.PatientsSex
            };
        }

        public IEnumerable<IDicomSeries> GetSeriesForStudy(
            string studyUid, IDicomNode localNode, IDicomNode remoteNode)
        {
            CheckRemoteNodeAvailability(localNode, remoteNode);

            var seriesQuery = new SeriesQueryIod();
            seriesQuery.SetCommonTags();
            seriesQuery.StudyInstanceUid = studyUid;
            var find = new StudyRootFindScu();

            return find.Find(localNode.AeTitle, remoteNode.AeTitle, remoteNode.IpAddress, remoteNode.Port, seriesQuery)
                .Select(s => new DicomSeries
                {
                    SeriesInstanceUid = s.SeriesInstanceUid,
                    SeriesDescription = s.SeriesDescription,
                    StudyInstanceUid = s.StudyInstanceUid
                });
        }

        public string GetStudyUidForAccession(string accession,
            IDicomNode localNode, IDicomNode remoteNode)
        {
            CheckRemoteNodeAvailability(localNode, remoteNode);

            var studyQuery = new StudyQueryIod();
            studyQuery.SetCommonTags();
            studyQuery.AccessionNumber = accession;

            var findScu = new StudyRootFindScu();
            var studies = findScu.Find(localNode.AeTitle, remoteNode.AeTitle,
                remoteNode.IpAddress, remoteNode.Port, studyQuery);

            if (studies.Count == 0) throw new DicomException($"No study was found for accession: {accession}");
            return studies.Count == 0 ? "" : studies.FirstOrDefault()?.StudyInstanceUid;
        }

        public void CheckRemoteNodeAvailability(IDicomNode localNode, IDicomNode remoteNode)
        {
            var verificationScu = new VerificationScu();
            //var result = verificationScu.Verify(localNode.AeTitle, remoteNode.AeTitle, remoteNode.IpAddress, remoteNode.Port);
            var result = verificationScu.Verify("KPSB", "***REMOVED***", "***REMOVED***", 104);
            if (result != VerificationResult.Success)
                throw new Exception($"Remote Dicom node not reachable. AET: [{remoteNode.AeTitle}] IP: [{remoteNode.IpAddress}]");
        }

        public IDicomSeries GetSeriesForSeriesUid(
            string studyUid, string seriesUid, IDicomNode localNode, IDicomNode remoteNode)
        {
            CheckRemoteNodeAvailability(localNode, remoteNode);

            var seriesQueryIod = new SeriesQueryIod();
            seriesQueryIod.SetCommonTags();
            seriesQueryIod.StudyInstanceUid = studyUid;
            seriesQueryIod.SeriesInstanceUid = seriesUid;

            var findScu = new StudyRootFindScu();
            var series = findScu.Find(localNode.AeTitle, remoteNode.AeTitle,
                remoteNode.IpAddress, remoteNode.Port, seriesQueryIod);
            var seriesDescription = series.Count == 0 ? "" : series.FirstOrDefault()?.SeriesDescription;

            var imageQueryIods = GetImageIodsForSeries(studyUid, seriesUid, localNode, remoteNode) as IList<ImageQueryIod>;

            if (imageQueryIods?.ToList().Count == 0) return null;

            return new DicomSeries
            {
                StudyInstanceUid = studyUid,
                SeriesInstanceUid = seriesUid,
                SeriesDescription = seriesDescription,
                Images = imageQueryIods?
                    .Select(r => new DicomImage { ImageUid = r.SopInstanceUid })
                    .ToList()
            };
        }

        private static IEnumerable<ImageQueryIod> GetImageIodsForSeries(
            string studyUid, string seriesUid, IDicomNode localNode, IDicomNode remoteNode)
        {
            var imageQuery = new ImageQueryIod();

            imageQuery.SetCommonTags();
            imageQuery.StudyInstanceUid = studyUid;
            imageQuery.SeriesInstanceUid = seriesUid;

            var find = new StudyRootFindScu();
            var results = find.Find(localNode.AeTitle, remoteNode.AeTitle, remoteNode.IpAddress,
                remoteNode.Port, imageQuery);

            if (!string.IsNullOrEmpty(find.FailureDescription))
                throw new DicomException($"Series query failed: {find.FailureDescription}{Environment.NewLine}" +
                                         $"AET: {remoteNode.AeTitle}{Environment.NewLine}" +
                                         $"Study Uid: {studyUid}{Environment.NewLine}" +
                                         $"Series Uid: {seriesUid}");
            return results;
        }

        public void SaveSeriesToLocalDisk(
            IDicomSeries dicomSeries, string folderPath, IDicomNode localNode, IDicomNode remoteNode)
        {
            var moveScu = new StudyRootMoveScu(localNode.AeTitle, remoteNode.AeTitle,
                remoteNode.IpAddress, remoteNode.Port, localNode.AeTitle);

            moveScu.AddStudyInstanceUid(dicomSeries.StudyInstanceUid);
            moveScu.AddSeriesInstanceUid(dicomSeries.SeriesInstanceUid);

            StorageScp.StorageLocation = folderPath;

            try
            {
                StorageScp.StartListening(localNode.AeTitle, localNode.Port);

                moveScu.Move();

                if (!string.IsNullOrEmpty(moveScu.FailureDescription))
                    throw new DicomException(moveScu.FailureDescription);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex); // TODO3: Implement proper logging
                throw;
            }
            finally
            {
                StorageScp.StopListening(localNode.Port);
            }

        }

        public IDicomStudy GetStudyForAccession(string accessionNumber,
            IDicomNode localNode, IDicomNode remoteNode)
        {
            var query = new StudyQueryIod();
            query.SetCommonTags();
            query.AccessionNumber = accessionNumber;
            var findScu = new StudyRootFindScu();
            var studies = findScu
                .Find(localNode.AeTitle, remoteNode.AeTitle, remoteNode.IpAddress, remoteNode.Port, query)
                .Select(MapToDicomStudy).ToList();
            if (studies.Count > 1)
                throw new Exception($"{studies.Count} studies returned for accession {accessionNumber}");
            return studies.FirstOrDefault();
        }

        private void OutputDataReceivedInProcess(object sender, DataReceivedEventArgs e)
        {
            var consoleColor = Console.ForegroundColor;
            try
            {
                Console.ForegroundColor = ConsoleColor.Green;
                if (!string.IsNullOrEmpty(e.Data) && !string.IsNullOrWhiteSpace(e.Data))
                    _log.Info($"Process stdout:{Environment.NewLine}{e.Data}");
            }
            finally
            {
                Console.ForegroundColor = consoleColor;
            }
        }
        private void ErrorOccuredInProcess(object sender, DataReceivedEventArgs e)
        {
            // Swallow log4j initialisation warnings
            if (e?.Data == null || string.IsNullOrEmpty(e.Data) || string.IsNullOrWhiteSpace(e.Data) || e.Data.ToLower().Contains("log4j")) return;

            var consoleColor = Console.ForegroundColor;
            try
            {
                Console.ForegroundColor = ConsoleColor.Red;
                if (!string.IsNullOrEmpty(e.Data) && !string.IsNullOrWhiteSpace(e.Data))
                    _log.Error($"Process error:{Environment.NewLine}{e.Data}");
            }
            finally
            {
                Console.ForegroundColor = consoleColor;
            }
        }
    }
}