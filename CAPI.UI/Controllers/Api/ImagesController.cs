using CAPI.Common;
using CAPI.Dicom.Model;
using CAPI.ImageProcessing;
using CAPI.UI.Models;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Web.Http;
using System.Web.Mvc;
using CAPI.Common.Config;
using CAPI.Dicom.Abstraction;
using Unity;

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
                var imageRepDirPath = ImgProc.GetImageRepositoryPath();
                var seriesList = (
                    from seriesDirName 
                    in seriesDirNames
                    let dicomSeriesDirPath = $"{imageRepDirPath}\\Viewable\\{seriesDirName}"
                    where Directory.Exists(dicomSeriesDirPath)
                    select new DicomSeriesVm().MapDicomSeriesToVm(new DicomSeries(seriesDirName, dicomSeriesDirPath))).ToArray();
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
                var imageRepDirPath = ImgProc.GetImageRepositoryPath();
                var dicomDir = $"{imageRepDirPath}\\Dicom";
                if (!Directory.Exists(dicomDir)) throw new DirectoryNotFoundException(dicomDir);
                var seriesList = (
                    from seriesDirPath
                    in Directory.GetDirectories(dicomDir)
                    let test = Path.GetFileName(seriesDirPath)
                    // select new DicomSeries(Path.GetFileName(seriesDirPath), seriesDirPath).GetViewModel()).ToArray();
                    select new DicomSeriesVm().MapDicomSeriesToVm(new DicomSeries(Path.GetFileName(seriesDirPath), seriesDirPath))).ToArray();
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
            var imageRepoPath = ImgProc.GetImageRepositoryPath();
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
            var container = new UnityContainer();
            var dicomFactory = container.Resolve<IDicomFactory>();

            var dicomTags = dicomFactory.CreateDicomTagCollection();
            dicomTags.PatientName.Values = new[] { "Test1^Mehdi" };
            dicomTags.PatientId.Values = new[] { "1999999998" };
            dicomTags.PatientSex.Values = new[] { "O" };

            const string fileFullPath = @"C:\temp\test\00000010_modified";
            File.Copy(@"C:\temp\test\00000010", fileFullPath, true);

            var dicomServices = dicomFactory.CreateDicomServices();

            dicomServices.UpdateDicomHeaders(fileFullPath, dicomTags, DicomNewObjectType.NewImage);

            var dicomNode = dicomFactory.CreateDicomNode("Home PC", "ORTHANC", "127.0.0.1", 4242);
            var destinationDicomNode = dicomFactory.CreateDicomNode( "Work PC", "KPSB", "172.28.42.42", 104 );

            dicomServices.SendDicomFile(fileFullPath, dicomNode.AeTitle, dicomNode);
        }
    }
}