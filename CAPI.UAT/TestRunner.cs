using CAPI.Agent;
using CAPI.Common.Config;
using CAPI.Dicom.Abstractions;
using CAPI.General.Abstractions.Services;
using CAPI.ImageProcessing.Abstraction;
using System;
using System.Collections.Generic;

namespace CAPI.UAT
{
    public class TestRunner
    {
        private static UatTests _testsToRun;
        private static readonly string Nl = Environment.NewLine;
        private IDicomFactory _dicomFactory;
        private IImageProcessingFactory _imgProcFactory;
        private IFileSystem _fileSystem;
        private IProcessBuilder _processBuilder;
        private readonly CapiConfig _capiConfig;

        public TestRunner(IDicomFactory dicomFactory, IImageProcessingFactory imgProcFactory,
                          IFileSystem fileSystem, IProcessBuilder processBuilder)
        {
            _dicomFactory = dicomFactory;
            _imgProcFactory = imgProcFactory;
            _fileSystem = fileSystem;
            _processBuilder = processBuilder;
            _capiConfig = new CapiConfig().GetConfig();
        }

        public void Run()
        {
            Console.WriteLine($"UAT (User Acceptance Testing) for CAPI project...");

            _testsToRun = GetAllTestsToRun();

            Console.WriteLine($"Running {_testsToRun.Tests.Count} test(s) to ensure all features function properly.");
            Console.WriteLine($"{Nl}Enter \"quit\" to exit the UAT.");

            _testsToRun.RunAll();

            // ReSharper disable once PossibleNullReferenceException
            while (!Console.ReadLine().ToLower().Equals("quit")) { }
        }

        private UatTests GetAllTestsToRun()
        {
            var tests = new UatTests();
            tests.Tests.Add(new Tests.ConfigFilesExists());
            tests.Tests.Add(new Tests.DbConnectionString { CapiConfig = _capiConfig });
            // Add Tests Here!
            return tests;
        }
    }

    internal class UatTests
    {
        private readonly string _nl = Environment.NewLine;
        private const ConsoleColor ConsoleColor = System.ConsoleColor.DarkGray;
        public List<IUatTest> Tests { get; set; }

        public UatTests()
        {
            Tests = new List<IUatTest>();
        }

        public void RunAll()
        {
            var anyTestsFailed = false;
            Console.ForegroundColor = ConsoleColor;
            for (var i = 0; i < Tests.Count; i++) // test in Tests)
            {
                Logger.Write($"{new string('-', Console.BufferWidth / 2)}", true, Logger.TextType.Content, true, 1, 0);
                Logger.Write($"Test {i + 1:D2}: ", false, Logger.TextType.Content, false, 0, 0);
                Logger.Write($"{Tests[i].Name}", true, Logger.TextType.Content, true, 0, 0);
                Logger.Write($"{Tests[i].Description}");
                bool testResult;
                try
                {
                    testResult = Tests[i].Run();
                }
                catch (Exception ex)
                {
                    Logger.Write("Test failed due to following exception:", true, Logger.TextType.Fail, false, 1);
                    Logger.Write($"[Message]:{ex.Message})", true, Logger.TextType.Fail, false, 1);
                    Logger.Write($"[Data]:{ex.Data}", true, Logger.TextType.Fail, false, 1);
                    Logger.Write($"[Stack]:{ex.StackTrace}", true, Logger.TextType.Fail, false, 1);
                    anyTestsFailed = true;
                    Tests[i].FailureResolution();
                    break;
                }

                var color = Console.ForegroundColor;
                if (testResult)
                {
                    Logger.Write($"{_nl}[Success] ", false, Logger.TextType.Success, true, 1, 0);
                    Logger.Write($"{Tests[i].SuccessMessage}", false, Logger.TextType.Success, false, 0, 0);
                }
                else
                {
                    Logger.Write("[Fail] ", false, Logger.TextType.Fail, true, 1, 0);
                    Logger.Write($"{Tests[i].FailureMessage}", true, Logger.TextType.Fail, false, 0, 0);
                    anyTestsFailed = true;
                    Tests[i].FailureResolution();
                    break;
                }
                Console.ForegroundColor = color;
            }
            if (!anyTestsFailed)
            {
                Logger.Write($"{new string('-', Console.BufferWidth / 2)}", true, Logger.TextType.Success, true, 2, 0);
                Logger.Write("*** All tests passed successfully ***", true, Logger.TextType.Success, true, 0, 0);
            }
            else
            {
                const string failureMessage = "xxx Please fix issues and run tests again xxx";
                Logger.Write($"{new string('-', failureMessage.Length)}", true, Logger.TextType.Fail, true, 1, 0);
                Logger.Write(failureMessage, true, Logger.TextType.Fail, true, 0, 0);
            }

        }
    }

    internal interface IUatTest
    {
        string Name { get; set; }
        string Description { get; set; }
        string SuccessMessage { get; set; }
        string FailureMessage { get; set; }
        string TestGroup { get; set; }
        CapiConfig CapiConfig { get; set; }
        AgentRepository Context { get; set; }

        bool Run();
        void FailureResolution();
    }
}
