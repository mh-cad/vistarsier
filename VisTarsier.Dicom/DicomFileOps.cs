using VisTarsier.Common;
using VisTarsier.Dicom.Abstractions;
using VisTarsier.Dicom.Model;
using ClearCanvas.Dicom;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VisTarsier.Config;
using System.Threading;

namespace VisTarsier.Dicom
{
    public static class DicomFileOps
    {
        ///// <summary>
        ///// Update tags in dicom file. 
        ///// </summary>
        ///// <param name="filepath">The path for the local dicom file.</param>
        ///// <param name="tags">The set of tags to be updated</param>
        ///// <param name="dicomNewObjectType">Essentially a mask to stop updating the wrong tags?</param>
        //public static void UpdateDicomHeaders(
        //   string filepath, DicomTagCollection tags, DicomNewObjectType dicomNewObjectType)
        //{
        //    var dcmFile = new ClearCanvas.Dicom.DicomFile(filepath);
        //    dcmFile.Load(filepath);

        //    switch (dicomNewObjectType)
        //    {
        //        case DicomNewObjectType.Anonymized:
        //            tags = UpdateUidsForNewStudy(tags);
        //            dcmFile = UpdateTags(dcmFile, tags, TagType.Patient, true);
        //            dcmFile = UpdateTags(dcmFile, tags, TagType.Study);
        //            dcmFile = UpdateTags(dcmFile, tags, TagType.Series);
        //            dcmFile = UpdateTags(dcmFile, tags, TagType.Image);
        //            break;
        //        case DicomNewObjectType.SiteDetailsRemoved:
        //            tags = UpdateUidsForNewStudy(tags);
        //            dcmFile = UpdateTags(dcmFile, tags, TagType.Site, true);
        //            break;
        //        case DicomNewObjectType.CareProviderDetailsRemoved:
        //            tags = UpdateUidsForNewStudy(tags);
        //            dcmFile = UpdateTags(dcmFile, tags, TagType.CareProvider, true);
        //            break;
        //        case DicomNewObjectType.NewPatient:
        //            tags = UpdateUidsForNewStudy(tags);
        //            dcmFile = UpdateTags(dcmFile, tags, TagType.Patient);
        //            dcmFile = UpdateTags(dcmFile, tags, TagType.Study);
        //            dcmFile = UpdateTags(dcmFile, tags, TagType.Series);
        //            dcmFile = UpdateTags(dcmFile, tags, TagType.Image);
        //            break;
        //        case DicomNewObjectType.NewStudy:
        //            tags = UpdateUidsForNewStudy(tags);
        //            dcmFile = UpdateTags(dcmFile, tags, TagType.Study);
        //            dcmFile = UpdateTags(dcmFile, tags, TagType.Series);
        //            dcmFile = UpdateTags(dcmFile, tags, TagType.Image);
        //            break;
        //        case DicomNewObjectType.NewSeries:
        //            tags = UpdateUidsForNewSeries(tags);
        //            dcmFile = UpdateTags(dcmFile, tags, TagType.Series);
        //            dcmFile = UpdateTags(dcmFile, tags, TagType.Image);
        //            break;
        //        case DicomNewObjectType.NewImage:
        //            tags = UpdateUidsForNewImage(tags);
        //            dcmFile = UpdateTags(dcmFile, tags, TagType.Image);
        //            break;
        //        case DicomNewObjectType.NoChange:
        //            break;
        //        default:
        //            throw new ArgumentOutOfRangeException(nameof(dicomNewObjectType), dicomNewObjectType, null);
        //    }
        //    dcmFile.Save(filepath);
        //}

        /// <summary>
        /// Creates and returns a collection of tags from a file, based on a given type or types.
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        public static DicomTagCollection GetTagsByType(string filepath, params TagType[] types)
        {
            var dcmFile = new DicomFile(filepath);
            dcmFile.Load();
            DicomTagCollection tags = new DicomTagCollection();

            var filetags = dcmFile.DataSet;
            foreach (var tag in tags)
            {
                if (types.Contains(tag.DicomTagType))
                {
                    tags.SetTagValue(tag.GetTagValue(), filetags[tag.GetTagValue()].Values);
                }
            }

            return tags;
        }

        /// <summary>
        /// Updates the given Dicom headers without trying to be clever.
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="tags"></param>
        public static void ForceUpdateDicomHeaders(string filepath, DicomTagCollection tags)
        {
            var dcmFile = new ClearCanvas.Dicom.DicomFile(filepath);
            dcmFile.Load(filepath);

            tags.ToList().ForEach(tag => UpdateTag(dcmFile, tag));
            dcmFile.Save();
        }

        /// <summary>
        /// Update the tags for the given files. Files will be given a generated SeriesInstanceUid and ImageUid.
        /// </summary>
        /// <param name="filesPath">List of files to apply the tags to.</param>
        /// <param name="tags">The tags which you'd like to apply to the above files.</param>
        public static void GenerateSeriesHeaderForAllFiles(string[] filesPath, DicomTagCollection tags, int uidpostfix=1)
        {
            tags.SeriesInstanceUid.Values = new[] { GenerateNewSeriesUid(uidpostfix.ToString()) };
            tags.ImageUid.Values = new[] { GenerateNewImageUid() };
            foreach (var filepath in filesPath)
            {
                var dcmFile = new ClearCanvas.Dicom.DicomFile(filepath);
                dcmFile.Load(filepath);
                dcmFile = UpdateTags(dcmFile, tags, TagType.Series);
                dcmFile.Save(filepath);
            }
        }

        private static DicomFile UpdateTags(
            ClearCanvas.Dicom.DicomFile dcmFile, DicomTagCollection newTags, TagType tagType, bool overwriteIfNotProvided = false)
        {
            if (newTags == null) return dcmFile;
            newTags.ToList().ForEach(tag =>
            {
                if (tag.DicomTagType == tagType)
                    dcmFile = UpdateTag(dcmFile, tag, overwriteIfNotProvided);
            });
            return dcmFile;
        }

        /// <summary>
        /// This method will update a tag 
        /// </summary>
        /// <param name="dcmFile"></param>
        /// <param name="newTag"></param>
        /// <param name="overwriteIfNotProvided"></param>
        /// <returns></returns>
        private static DicomFile UpdateTag(
            ClearCanvas.Dicom.DicomFile dcmFile, IDicomTag newTag, bool overwriteIfNotProvided = false)
        {
            if (newTag?.Values == null && !overwriteIfNotProvided) return dcmFile;
            var value = newTag?.Values != null && newTag.Values.Length > 0 ? newTag.Values[0] : "";

            if (newTag?.GetValueType() == typeof(string[]))
            {
                var vals = new string[newTag.Values.Length];
                newTag.Values.CopyTo(vals, 0);
                dcmFile.DataSet[newTag.GetTagValue()].Values = vals;
            }
            else if (newTag?.GetValueType() == typeof(string))
            {
                dcmFile.DataSet[newTag.GetTagValue()].Values = value;
            }
            return dcmFile;
        }

        private static DicomFile UpdateArrayTag(
            ClearCanvas.Dicom.DicomFile dcmFile, IDicomTag newTag, IEnumerable value)
        {
            dcmFile.DataSet[newTag.GetTagValue()].Values = value;
            return dcmFile;
        }

        public static DicomTagCollection UpdateUidsForNewStudy(DicomTagCollection tags)
        {
            if (tags == null) return null;
            tags.StudyInstanceUid.Values = new[] { GenerateNewStudyUid() };
            tags = UpdateUidsForNewSeries(tags);
            tags = UpdateUidsForNewImage(tags);
            return tags;
        }
        public static DicomTagCollection UpdateUidsForNewSeries(DicomTagCollection tags)
        {
            if (tags == null) return null;
            tags.SeriesInstanceUid.Values = new[] { GenerateNewSeriesUid() };
            tags = UpdateUidsForNewImage(tags);
            return tags;
        }
        public static DicomTagCollection UpdateUidsForNewImage(DicomTagCollection tags)
        {
            if (tags == null) return null;
            tags.ImageUid.Values = new[] { GenerateNewImageUid() };
            return tags;
        }

        /// <summary>
        /// Gets the tags for a dicom file. 
        /// </summary>
        /// <param name="filePath">The dicom file.</param>
        /// <returns></returns>
        public static DicomTagCollection GetDicomTags(string filePath)
        {
            var dcmFile = new ClearCanvas.Dicom.DicomFile(filePath);
            dcmFile.Load(filePath);
            var tags = new DicomTagCollection();
            var updatedTags = new DicomTagCollection();
            foreach (var tag in tags.ToList())
                updatedTags.SetTagValue(tag.GetTagValue(), dcmFile.DataSet[tag.GetTagValue()].Values);
            return updatedTags;
        }

        private static object _uidlock = new object();
        private static int _uidseq = 0;

        public static string GenerateNewStudyUid(string postfix = "1")
        {
            lock(_uidlock)
            {
                if (_uidseq > 100) _uidseq = 0;
                var tix = DateTime.Now.Ticks % 9999;
                Thread.Sleep(5);
                // By the looks, this prefix is assigned from https://www.medicalconnections.co.uk/FreeUID. I assume for us. -P@
                return $"1.2.826.0.1.3680043.9.7303.1.1.{DateTime.Now:yyyyMMddHHmmssfff}{tix}.{++_uidseq}.{postfix}";
            }
        }
        public static string GenerateNewSeriesUid(string postfix="1")
        {
            lock (_uidlock)
            {
                if (_uidseq > 100) _uidseq = 0;
                var tix = DateTime.Now.Ticks % 9999;
                Thread.Sleep(5);
                return $"1.2.826.0.1.3680043.9.7303.1.2.{DateTime.Now:yyyyMMddHHmmssfff}{tix}.{++_uidseq}.{postfix}";
            }
        }
        public static string GenerateNewImageUid(string postfix = "1")
        {
            lock (_uidlock)
            {
                if (_uidseq > 100) _uidseq = 0;
                var tix = DateTime.Now.Ticks % 9999;
                Thread.Sleep(5);
                return $"1.2.826.0.1.3680043.9.7303.1.3.{DateTime.Now:yyyyMMddHHmmssfff}{tix}.{++_uidseq}.{postfix}";
            }
        }

        public static void ConvertBmpsToDicom(string bmpFolder, string dicomFolder, SliceType sliceType, string dicomHeadersFolder, bool matchByFilename = false)
        {
            FileSystem.DirectoryExistsIfNotCreate(dicomFolder);

            var orderedDicomFiles = new List<string>();
            if (!string.IsNullOrEmpty(dicomHeadersFolder))
            {
                orderedDicomFiles = GetFilesOrderedByInstanceNumber(Directory.GetFiles(dicomHeadersFolder)).ToList();
                if (!DicomFilesInRightOrder(dicomHeadersFolder, sliceType))
                    orderedDicomFiles = orderedDicomFiles.ToArray().Reverse().ToList();
            }

            var bmpFiles = Directory.GetFiles(bmpFolder);

            if (matchByFilename)
                bmpFiles = MatchBmpFilesWithDicomFiles(bmpFiles, orderedDicomFiles);

            // check if number of dicom and bmp files match
            if (bmpFiles.Length > orderedDicomFiles.Count)
                throw new Exception($"BMP file count must not be greater than dicom file count. {bmpFiles.Length} > {orderedDicomFiles.Count}");

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

        private static bool DicomFilesInRightOrder(string dicomFolderPath, SliceType sliceType)
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

        public static void ConvertBmpToDicom(string bmpFilepath, string dicomFilePath, string dicomHeadersFilePath = "")
        {
            var arguments = string.Empty;
            if (!string.IsNullOrEmpty(dicomHeadersFilePath))
                arguments = $"-df \"{dicomHeadersFilePath}\" "; // Copy dicom headers from dicom file: -df = dataset file
            
            arguments += $"-i BMP \"{bmpFilepath}\" \"{dicomFilePath}\"";
            ProcessBuilder.CallExecutableFile(CapiConfig.GetConfig().Binaries.img2dcm, arguments);
        }

        /// <summary>
        /// Replaces the tags in a given Dicom file with those provided, preserving only the pixel data from the original.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="cleanTags"></param>
        public static void UpdateTags(string file, DicomTagCollection cleanTags, bool overwriteIfNotProvided = false)
        {
            DicomFile df = new DicomFile(file);
            df.Load();
            foreach (var tag in df.DataSet)
            {
                // Don't overwrite or remove the pixel data tag.
                if (tag.Tag.TagValue == 0x7FE00010)
                {
                    continue;
                }
                else
                {
                    // Find the updated tag (or null / default)
                    var newTag = cleanTags.FirstOrDefault(t => t.GetTagValue() == tag.Tag.TagValue);
                    // Update the tag, overwriting with an empty value if no tag is present.
                    UpdateTag(df, newTag, overwriteIfNotProvided);
                }
            }

            df.Save();
        }

        //public static void ConvertBmpToDicomAndAddToExistingFolder(string bmpFilePath, string dicomFolderPath, string newFileName = "")
        //{
        //    if (string.IsNullOrEmpty(newFileName)) newFileName = Path.GetFileNameWithoutExtension(bmpFilePath);
        //    if (Directory.GetFiles(dicomFolderPath)
        //        .Any(f => Path.GetFileName(f).ToLower().Contains(newFileName ?? throw new ArgumentNullException(nameof(newFileName)))))
        //        newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");

        //    var dicomFileHighestInstanceNo = GetDicomFileWithHighestInstanceNumber(dicomFolderPath);
        //    var headers = GetDicomTags(dicomFileHighestInstanceNo);
        //    var newFilePath = Path.Combine(dicomFolderPath, newFileName ?? throw new ArgumentNullException(nameof(newFileName)));

        //    ConvertBmpToDicom(bmpFilePath, newFilePath, dicomFileHighestInstanceNo);
        //    var newFileInstanceNumber = headers.InstanceNumber.Values == null || headers.InstanceNumber.Values.Length < 1 ? 1 : int.Parse(headers.InstanceNumber.Values[0]) + 1;
        //    headers.InstanceNumber.Values = new[] { newFileInstanceNumber.ToString() };
        //    UpdateDicomHeaders(newFilePath, headers, DicomNewObjectType.NewImage);
        //}

        public static string GetPatientIdFromDicomFile(string dicomFilePath)
        {
            var dcmFile = new ClearCanvas.Dicom.DicomFile(dicomFilePath);
            dcmFile.Load(dicomFilePath);
            var patientIdTag = dcmFile.DataSet[1048608].Values as string[];
            return patientIdTag?[0];
        }

        public static void UpdateImagePositionFromReferenceSeries(string[] dicomFilesToUpdate, string[] orientationDicomFiles)
        {
            if (dicomFilesToUpdate == null || dicomFilesToUpdate.Length == 0) throw new ArgumentNullException("Orientation Dicom files not available to read from");
            if (orientationDicomFiles == null || orientationDicomFiles.Length == 0) throw new ArgumentNullException("Dicom files to copy orientation data to not available");

            if (dicomFilesToUpdate.Length != orientationDicomFiles.Length)
                throw new Exception("Number of files in \"Orientation dicom\" folder and \"Dicom Files to Update\" do not match");

            var orderedFilesToUpdate = dicomFilesToUpdate
                .OrderBy((f) =>
                {
                    var vals = GetDicomTags(f).InstanceNumber.Values;
                    return vals != null && vals.Length > 0 ?
                        int.Parse(GetDicomTags(f).InstanceNumber.Values[0])
                        : 0;
                }).ToArray();

            var orderedOrientationFiles = orientationDicomFiles
                .OrderBy((f) =>
                {
                    var vals = GetDicomTags(f).InstanceNumber.Values;
                    return vals != null && vals.Length > 0 ?
                        int.Parse(GetDicomTags(f).InstanceNumber.Values[0])
                        : 0;
                }).ToArray();

            for (var i = 0; i < orderedFilesToUpdate.Count(); i++)
            {
                var fileToUpdate = orderedFilesToUpdate[i];
                var orientationFile = orderedOrientationFiles[i];

                var imagePatientOrientation = GetDicomTags(orientationFile).ImagePositionPatient;
                var imageOrientation = GetDicomTags(orientationFile).ImageOrientation;
                var frameOfReferenceUid = GetDicomTags(orientationFile).FrameOfReferenceUid;
                var sliceLocation = GetDicomTags(orientationFile).SliceLocation;

                var dcmFile = new ClearCanvas.Dicom.DicomFile();
                dcmFile.Load(fileToUpdate);
                dcmFile = UpdateArrayTag(dcmFile, imagePatientOrientation, imagePatientOrientation.Values);
                dcmFile = UpdateArrayTag(dcmFile, imageOrientation, imageOrientation.Values);
                dcmFile = UpdateArrayTag(dcmFile, frameOfReferenceUid, frameOfReferenceUid.Values);
                dcmFile = UpdateArrayTag(dcmFile, sliceLocation, sliceLocation.Values);
                dcmFile.Save(fileToUpdate);
            }
        }

        private static string GetDicomFileWithLowestInstanceNumber(string dicomFolderPath)
        {
            var dicomFiles = Directory.GetFiles(dicomFolderPath);
            
            return dicomFiles.OrderBy((f) =>
            {
                var vals = GetDicomTags(f).InstanceNumber.Values;
                return vals != null && vals.Length > 0 ?
                    int.Parse(GetDicomTags(f).InstanceNumber.Values[0])
                    : 0;
            }).FirstOrDefault();
        }

        private static IEnumerable<string> GetFilesOrderedByInstanceNumber(IEnumerable<string> files)
        {
            return files.OrderBy((f) =>
            {
                var tags = GetDicomTags(f);
                if (tags.InstanceNumber?.Values?.Length > 0)
                {
                    return Convert.ToInt32(tags.InstanceNumber.Values[0]);
                }

                return int.MaxValue;
                
            }).ToList();
        }
    }
}
