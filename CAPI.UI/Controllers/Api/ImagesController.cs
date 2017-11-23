using CAPI.Dicom;
using CAPI.Dicom.Model;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Web.Http;
using System.Web.Mvc;
using CAPI.Common;
using CAPI.ImageProcessing;
using CAPI.UI.Models;

namespace CAPI.UI.Controllers.Api
{
    public class ImagesController : ApiController
    {
        [System.Web.Http.HttpPost]
        public JsonResult GetSampleSeries([FromBody]string[] seriesDirNames)
        {
            var response = new Response();
            try
            {
                var imageRepDirPath = Config.GetImageRepositoryPath();
                var seriesList = (
                    from seriesDirName 
                    in seriesDirNames
                    let dicomSeriesDirPath = $"{imageRepDirPath}\\Viewable\\{seriesDirName}"
                    where Directory.Exists(dicomSeriesDirPath)
                    select new DicomSeries(seriesDirName, dicomSeriesDirPath).GetViewModel()).ToArray();
                response.Data = seriesList;
            }
            catch (Exception exception)
            {
                response.Exception = exception;
            }

            return new JsonResult{ Data = response.GetViewModel() };
        }

        [System.Web.Http.HttpGet]
        public JsonResult GetDicomSeries()
        {
            var response = new Response();
            try
            {
                var imageRepDirPath = Config.GetImageRepositoryPath();
                var dicomDir = $"{imageRepDirPath}\\Dicom";
                if (!Directory.Exists(dicomDir)) throw new DirectoryNotFoundException(dicomDir);
                var seriesList = (
                    from seriesDirPath
                    in Directory.GetDirectories(dicomDir)
                    let test = Path.GetFileName(seriesDirPath)
                    select new DicomSeries(Path.GetFileName(seriesDirPath), seriesDirPath).GetViewModel()).ToArray();
                response.Data = seriesList;
            }
            catch (Exception exception)
            {
                response.Exception = exception;
            }

            return new JsonResult { Data = response.GetViewModel() };
        }

        [System.Web.Http.HttpPost]
        public JsonResult ConvertToViewable([FromBody]dynamic seriesDetails)
        {
            var files = (seriesDetails["files"] as JArray)?.Select(x => x.Value<string>()).ToList();
            var seriesName = seriesDetails["seriesName"].Value;
            var imageRepoPath = Config.GetImageRepositoryPath();
            var response = new Response();
            try
            {
                if (files?.Count < 1) throw new Exception("No files passed for conversion");
                var dicomDir = Path.GetDirectoryName($"{imageRepoPath}\\{files?.FirstOrDefault()}");
                var imageConverter = new ImageConverter();
                var outFiles = imageConverter.ConvertDicom2Viewable(dicomDir);
                response.Data = outFiles.Select(f => f.Replace("\\", "/"));
            }
            catch (Exception exception)
            {
                response.Exception = exception;
            }

            return new JsonResult { Data = response.GetViewModel() };
        }

        [System.Web.Http.HttpGet]
        public void SendToPacs()
        {
            //const string fileFullPath = @"C:\temp\test\1.3.12.2.1107.5.2.32.35208.2012081409204631896413069.dcm";
            const string fileFullPath = @"C:\temp\test\00000010_modified";
            File.Copy(@"C:\temp\test\00000010", fileFullPath, true);
            var dicomTags = new DicomTagCollection
            {
                PatientName = { Values = new [] { "Test1^Mehdi" } },
                PatientId = { Values = new [] { "1999999998" } },
                PatientSex = { Values = new [] { "O" } }
            };

            DicomServices.UpdateDicomHeaders(fileFullPath, dicomTags, DicomNewObjectType.NewImage);
            //DicomServices.UpdateDicomHeaders(fileFullPath, dicomTags, DicomNewObjectType.Anonymized);
            //DicomServices.UpdateDicomHeaders(fileFullPath, dicomTags, DicomNewObjectType.SiteDetailsRemoved);
            //DicomServices.UpdateDicomHeaders(fileFullPath, dicomTags, DicomNewObjectType.CareProviderDetailsRemoved);

            var dicomNode = new DicomNode { LogicalName = "Home PC", AeTitle = "ORTHANC", IpAddress = "127.0.0.1", Port = 4242 };
            //var destinationDicomNode = new DicomNode { AeTitle = "KPSB", IpAddress = "172.28.42.42", Port = 104 };
            DicomServices.SendDicomFile(fileFullPath, dicomNode.AeTitle, dicomNode);
            //DicomServices.SendDicomFile(dicomFileWithUpdatedHeaders, "KPSB", destinationDicomNode);
        }
    }
}