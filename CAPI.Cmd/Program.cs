using CAPI.Agent.Models;
using CAPI.Common;
using CAPI.Dicom;
using CAPI.Dicom.Abstractions;
using CAPI.Dicom.Model;
using Newtonsoft.Json;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;

namespace CAPI.Cmd
{
    class Program
    {
        /// <summary>
        /// Entry point. We're really only parsing out the params and throwing over to the CAPIit method.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static async Task<int> Main(params string[] args)
        {
            RootCommand rootCommand = new RootCommand(
              description: "Converts an image file from one format to another."
              , treatUnmatchedTokensAsErrors: false);

            Option priorOption = new Option(
              aliases: new string[] { "--prior", "-p" }
              , description: "Prior accession number, nifti, or dicom folder"
              , argument: new Argument<string>());
            rootCommand.AddOption(priorOption);

            Option currentOption = new Option(
              aliases: new string[] { "--current", "-c" }
              , description: "Current accession number, nifti, or dicom folder"
              , argument: new Argument<string>());
            rootCommand.AddOption(currentOption);

            Option recipeOption = new Option(
              aliases: new string[] { "--recipe", "-r" }
              , description: "Recipe to use"
              , argument: new Argument<string>());
            rootCommand.AddOption(recipeOption);

            Option outputOption = new Option(
              aliases: new string[] { "--output-type", "-t" }
              , description: "Type of output [nifti|dicom|scp]. Output can be NIfTI files, Dicom folders, or direct to PACS system"
              , argument: new Argument<string>());
            rootCommand.AddOption(outputOption);

            Option aetOption = new Option(
              aliases: new string[] { "--calling-ae", "-a" }
              , description: "Set my calling AE [title,hostname,port] e.g. [CAPI,127.0.0.1,4030] (no spaces)"
              , argument: new Argument<string>());
            rootCommand.AddOption(aetOption);

            Option aecOption = new Option(
              aliases: new string[] { "--target-ae", "-e" }
              , description: "Set the target [title,hostname,port] e.g. [CAPI,127.0.0.1,4030] (no spaces). " +
                "Target AET will be used for input / output based on accession number and scp mode, respectively. " +
                "To use different source and target AETs use the --recipe option."
              , argument: new Argument<string>());
            rootCommand.AddOption(aecOption);

            rootCommand.Handler =
              CommandHandler.Create((Action<string, string, string, string, string, string>)CAPIit);

            // Print help if nothing happened.
            if (args.Length < 1) args = new string[] { "-h" };

            return await rootCommand.InvokeAsync(args);
        }

        static public void CAPIit(
          string prior = null, string current = null, string recipe = null, string output = null, string callingAE = null, string targetAE = null)
        {
            var dicomConfig = new DicomConfig();
            
            // Attempt to create nodes.
            var localNode = ParseNode(callingAE);
            var sourceNode = ParseNode(targetAE);
            
            // Attempt to read recipe.
            Recipe r = ReadRecipe(recipe);
            if (recipe != null && r == null)
            {
                System.Console.WriteLine($"Error reading recipe ({recipe}). We'll try to make things happen anyway...");
            }

            // Parse and infer type of inputs and output.
            var priorType = GuessType(prior);
            var currentType = GuessType(current);
            var outputType = GuessType(output);

            // Create a recipe based on the args.
            if (r == null)
            {
                r = new Recipe()
                {
                    BiasFieldCorrection = true,
                    ExtractBrain = true,
                    SourceAet = sourceNode.AeTitle,
                   // CurrentAccession = currentType == IOType.ACCESSION ? current : currentType == IOType ?  : null,
                    PriorAccession = priorType == IOType.ACCESSION ? prior : null,
                };
            }

            //var ip = new ImageProcessor();
            if (outputType == IOType.ACCESSION)

            //DicomServices dicomServices = new DicomServices(dicomConfig);

            //var studies = dicomServices.GetStudiesForPatientId("2234162", localNode, sourceNode);
            //foreach (var study in studies)
            //{
            //    System.Console.WriteLine("Acc: " + study.AccessionNumber);
            //    System.Console.WriteLine("Acc: " + study.StudyInstanceUid);
            //    var series = dicomServices.GetSeriesForStudy(study.StudyInstanceUid, localNode, sourceNode);

            //    foreach (var sery in series)
            //    {
            //        if (sery.SeriesDescription.Contains("VT"))
            //        {
            //            System.Console.WriteLine("Sery: " + sery.SeriesDescription);
            //            try
            //            {
            //                var dir = "d:/vt_temp/" + study.AccessionNumber + "/";
            //                FileSystem.DirectoryExistsIfNotCreate(dir);
            //                dicomServices.SaveSeriesToLocalDisk(sery, dir, localNode, sourceNode);
            //            }
            //            catch
            //            {
            //                System.Console.WriteLine("Nuppers");
            //            }
            //            foreach (var image in sery.Images)
            //            {
            //                System.Console.WriteLine("Img: " + image.ToString());
            //            }
            //        }
            //    }
            //}

            Console.ReadKey();
        }

        private static IOType GuessType(string thing)
        {
            thing = thing.Trim(' ');

            if ("nifti".Equals(thing.ToLower())) return IOType.NIFTI;
            if ("dicom".Equals(thing.ToLower())) return IOType.DICOM;
            if ("scp".Equals(thing.ToLower())) return IOType.ACCESSION;

            if (thing.EndsWith(".nii")) return IOType.NIFTI;
            if (Directory.Exists(thing)) return IOType.DICOM;

            // Assume accession (can't find any hard spec on what'd going to be set by RIS.
            // We'll check that there's an AE later.
            return IOType.ACCESSION;
        }

        private static IDicomNode ParseNode(string AE)
        {
            IDicomNode node = null;
            if (AE != null)
            {
                string[] nodeStuff = AE.Trim(' ', '[', ']').Split(',');
                node = new DicomNode(nodeStuff[0].Trim(' '), nodeStuff[0].Trim(' '), nodeStuff[1].Trim(' '), int.Parse(nodeStuff[2].Trim(' ')));
            }

            return node;
        }

        private static Recipe ReadRecipe(string recipePath)
        {
            // Try to parse the recipe.
            Recipe recipe = null;
            try
            {
                var recipeText = File.ReadAllText(recipePath);
                recipe = JsonConvert.DeserializeObject<Recipe>(recipeText);
            }
            catch { }

            return recipe;
        }

        /// <summary>
        /// CAPI Entry point.
        /// </summary>
        /// <param name = "recipe" > Recipe file</param>
        /// <param name = "src" > Source PACs</param>
        /// <param name = "accession" > Accession reference for PACs system for current</param>
        /// <param name = "priorAccession" > Prior accession for PACs system</param>
        /// <param name = "outputType" > Output type[nii | pacs | dicom]. If blank or invalid this will be inferred where possible.</param>
        //static void Main(string[] args)
        //{
        //    //    //Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory + "../../");

        //    //    //string repo = "D:/Capi-Files/ImageRepository/MCGRE-18R0204857-181128_140605997";

        //    //    ////foreach(var arg in args)
        //    //    ////{
        //    //    ////    if (arg.StartsWith("-r")) repo = arg.Substring(3).Trim(' ');
        //    //    ////}


        //    //    //string currentDicomFolder = repo + "/Current/Dicom";
        //    //    //string priorDicomFolder = repo + "/Prior/Dicom";
        //    //    //string referenceDicomFolder = repo + "/Reference/Dicom";

        //    //    //string floatingPath = "floating.nii";
        //    //    //string fixedPath = "fixed.nii";
        //    //    //string outputPrefix = "capiout";

        //    //    ////if (args.Length >= 2)
        //    //    ////{
        //    //    ////    floatingPath = args[0];
        //    //    ////    fixedPath = args[1];
        //    //    ////    if (args.Length >= 3) outputPrefix = args[2];
        //    //    ////}
        //    //    ////else
        //    //    ////{
        //    //    ////    System.Console.WriteLine("Usage:");
        //    //    ////    System.Console.WriteLine("capi [floating file] [fixed file] [optional: output prefix]");
        //    //    ////}


        //    //    //System.Console.WriteLine("Files read...");

        //    //    //var timer = new Stopwatch();
        //    //    //var totaltime = new Stopwatch();
        //    //    //timer.Start();
        //    //    //totaltime.Start();

        //    //    //IImageProcessor ip = new ImageProcessor(null);

        //    //    //// Generate Nifti file from Dicom and pass to ProcessNifti Method for current series.
        //    //    //System.Console.WriteLine($@"Start converting series dicom files to Nii");

        //    //    ////var currentNifti = Tools.Dcm2Nii(currentDicomFolder, "current.nii");
        //    //    ////var priorNifti = Tools.Dcm2Nii(priorDicomFolder, "prior.nii");
        //    //    //var referenceNifti = Tools.Dcm2Nii(referenceDicomFolder, "reference.nii");

        //    //    ////DicomFile dfile = new DicomFile();
        //    //    ////dfile.Load(currentDicomFolder + "/000");
        //    //    ////using (var writer = File.CreateText(currentDicomFolder + "metadata.txt"))
        //    //    ////{
        //    //    ////    writer.Write(dfile.DataSet.DumpString);
        //    //    ////    writer.Flush();
        //    //    ////    writer.Close();
        //    //    ////}
        //    //    ////foreach (var dacThing in dfile.DataSet)
        //    //    ////{

        //    //    ////    System.Console.WriteLine(dacThing.Tag);
        //    //    ////    System.Console.WriteLine(dacThing.ToString());
        //    //    ////}

        //    //    ////System.Console.WriteLine(dfile.ImplementationClassUid);
        //    //    ////System.Console.WriteLine(dfile.ImplementationVersionName);
        //    //    ////System.Console.WriteLine(dfile.Loaded);
        //    //    ////System.Console.WriteLine(dfile.MediaStorageSopClassUid);
        //    //    ////System.Console.WriteLine(dfile.MediaStorageSopInstanceUid);
        //    //    ////System.Console.WriteLine(dfile.MetaInfoFileLength);
        //    //    ////System.Console.WriteLine(dfile.PrivateInformationCreatorUid);
        //    //    ////System.Console.WriteLine(dfile.SopClass);
        //    //    ////System.Console.WriteLine(dfile.SourceApplicationEntityTitle);
        //    //    ////System.Console.WriteLine(dfile.TransferSyntax);
        //    //    ////System.Console.WriteLine(dfile.TransferSyntaxUid);
        //    //    ////System.Console.WriteLine(dfile);


        //    //    //System.Console.WriteLine($@"Finished converting series dicom files to Nii");

        //    //    //string[] outputs = { outputPrefix + "-increase.nii", outputPrefix + "-decrease.nii" };

        //    //    //// Process Nifti files.
        //    //    ////ip.MSLesionCompare(currentNifti, priorNifti, referenceNifti, true, true, true, outputs, "prior-resliced.nii");

        //    //    //foreach (var output in outputs)
        //    //    //{
        //    //    //    Console.WriteLine($@"Reading: {output}");
        //    //    //    new Nifti().ReadNifti(output);
        //    //    //}

        //    //    //Console.WriteLine($@" ALL DONE! [{totaltime.Elapsed}]");
        //    //    //Console.ReadKey();
        //    //}
    }
}
