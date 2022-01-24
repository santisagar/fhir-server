﻿using ResourceProcessorNamespace;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace FHIRDataSynth
{
    internal class Program
    {
        private static void ValidateTaskCount(string s, out int tasksCount)
        {
            if (!(int.TryParse(s, out tasksCount) && tasksCount > 0 && tasksCount <= 256))
            {
                throw new Exception($"Invalid task count '{s}.");
            }
        }

        private static void ValidateTargetRatiosFile(string pathName, out TargetRatios targetRatios)
        {
            targetRatios = null;
            if (!File.Exists(pathName))
            {
                throw new Exception($"Invalid target ratios path or file name '{pathName}',  or insufficient permission.");
            }

            try
            {
                targetRatios = JsonSerializer.Deserialize<TargetRatios>(File.ReadAllText(pathName));
                foreach (TargetProfile tp in targetRatios.targetRatios)
                {
                    tp.Validate();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error reading target ratios file {pathName}. ({ex.Message})", ex);
            }
        }

        private class TargetRatios
        {
            public TargetProfile[] targetRatios { get; set; }
        }

        private static int Main(string[] args)
        {
            try
            {
                int ret;

                DateTime startTime = DateTime.Now;
                Console.WriteLine($"Start time: {startTime}");

                if (args.Length < 1)
                {
                    Console.WriteLine("Command line instructions:");
                    Console.WriteLine();
                    Console.WriteLine("To create blob blend:");
                    Console.WriteLine(" FHIRDataSynth.exe 'blob' in-blob-container-name out-blend-blob-container-name TaskCount TargetRatiosFile ConnectionString [OutConnectionString]");
                    Console.WriteLine("example:");
                    Console.WriteLine(" FHIRDataSynth.exe blob in-container test-out 2 \"..\\T R.json\" \"Storage Account Connection String\"");
                    Console.WriteLine();
                    Console.WriteLine("To verify blob blend:");
                    Console.WriteLine(" FHIRDataSynth.exe 'verifyblob' in-blob-container-name TaskCount TargetRatiosFile ConnectionString");
                    Console.WriteLine("example:");
                    Console.WriteLine(" FHIRDataSynth.exe verifyblob in-container 2 \"..\\T R.json\" \"Storage Account Connection String\"");
                    Console.WriteLine();
                    Console.WriteLine("To create file blend:");
                    Console.WriteLine(" FHIRDataSynth.exe 'file' InDir OutDir TaskCount TargetRatiosFile");
                    Console.WriteLine("example:");
                    Console.WriteLine(" FHIRDataSynth.exe file \"..\\In Dir\" \"..\\My Dir\" 2 \"..\\T R.json\"");
                    Console.WriteLine();
                    Console.WriteLine("To verify file blend:");
                    Console.WriteLine(" FHIRDataSynth.exe 'verifyfile' DirToBeVerified TaskCount TargetRatiosFile");
                    Console.WriteLine("example:");
                    Console.WriteLine(" FHIRDataSynth.exe verifyfile \"..\\V Dir\" 1 \"..\\T R.json\"");
                    Console.WriteLine();
                    Console.WriteLine("To create target ratios file:");
                    Console.WriteLine(" FHIRDataSynth.exe 'target' BlobGroupsInfoFile OneGroupInfoFile BlendRatiosFile TargetRatiosFile, TargetRatiosFileCsv");
                    Console.WriteLine("example:");
                    Console.WriteLine(" FHIRDataSynth.exe target ..\\BGI.json \"..\\OGI.json\" \"..\\B R.json\" \"..\\T R.json\" ..\\TR.csv");
                    Console.WriteLine();
                    Console.WriteLine("To import blend to server:");
                    Console.WriteLine(" FHIRDataSynth.exe 'import' ServerUrl ResourceGroupCount InputStrorageUrl input-blob-container importResultFileName InputConnectionString");
                    Console.WriteLine("example:");
                    Console.WriteLine(" FHIRDataSynth.exe import http://svr.azurewebsites.net 4 https://syntheadatastore.blob.core.windows.net blend-container ..\\importresult.json \"Storage Account Connection String\"");
                    Console.WriteLine();
                    Console.WriteLine("To check if served finished writting blend to database:");
                    Console.WriteLine(" FHIRDataSynth.exe 'isfinished' importResultFileName");
                    Console.WriteLine("example:");
                    Console.WriteLine(" FHIRDataSynth.exe isfinished ..\\importresult.json");
                    ret = -1;
                }
                else
                {
                    Console.WriteLine(string.Join(' ', args));
                    Console.WriteLine();

                    const string fileCommand = "file";
                    const string verifyFileCommand = "verifyfile";
                    const string blobCommand = "blob";
                    const string verifyBlobCommand = "verifyblob";
                    const string targetCommand = "target";
                    const string importCommand = "import";
                    const string isFinishedCommand = "isfinished";

                    string command = args[0];
                    switch (command)
                    {
                        case blobCommand:
                            {
                                string inContainerName;
                                string outContainerName;
                                int taskCount;
                                TargetRatios targetRatios;
                                string connectionString;
                                string outConnectionString;

                                if (args.Length != 6 && args.Length != 7)
                                {
                                    throw new Exception("Invalid number of input parameters.");
                                }

                                if (!CalculatorTargetRatios.IsValidBlobContainerName(args[1]))
                                {
                                    throw new Exception($"Invalid blend profile name '{args[1]}'. Follow Azure Blob naming rules.");
                                }

                                inContainerName = args[1];
                                if (!CalculatorTargetRatios.IsValidBlobContainerName(args[2]))
                                {
                                    throw new Exception($"Invalid blend profile name '{args[2]}'. Follow Azure Blob naming rules.");
                                }

                                outContainerName = args[2];
                                ValidateTaskCount(args[3], out taskCount);
                                ValidateTargetRatiosFile(args[4], out targetRatios);
                                connectionString = args[5];
                                if (args.Length == 7)
                                {
                                    outConnectionString = args[6];
                                }
                                else
                                {
                                    outConnectionString = connectionString;
                                }

                                foreach (TargetProfile tp in targetRatios.targetRatios)
                                {
                                    BlobResourceProcessor blobResourceProcessor = new BlobResourceProcessor(connectionString, inContainerName, outConnectionString, outContainerName + "-" + tp.name);
                                    blobResourceProcessor.Process(taskCount, tp);
                                }

                                ret = 0;
                            }

                            break;
                        case verifyBlobCommand:
                            {
                                string inContainerName;
                                int taskCount;
                                TargetRatios targetRatios;
                                string connectionString;

                                if (args.Length != 5)
                                {
                                    throw new Exception("Invalid number of input parameters.");
                                }

                                if (!CalculatorTargetRatios.IsValidBlobContainerName(args[1]))
                                {
                                    throw new Exception($"Invalid blend profile name '{args[1]}'. Follow Azure Blob naming rules.");
                                }

                                inContainerName = args[1];
                                ValidateTaskCount(args[2], out taskCount);
                                ValidateTargetRatiosFile(args[3], out targetRatios);
                                connectionString = args[4];
                                foreach (TargetProfile tp in targetRatios.targetRatios)
                                {
                                    BlobResourceProcessor blobResourceProcessor = new BlobResourceProcessor(connectionString, inContainerName + "-" + tp.name, null, null);
                                    blobResourceProcessor.Process(taskCount, tp);
                                }

                                ret = 0;
                            }

                            break;
                        case fileCommand:
                            {
                                int taskCount;
                                TargetRatios targetRatios;
                                string inDir;
                                string outDir;

                                if (args.Length != 5)
                                {
                                    throw new Exception("Invalid number of input parameters.");
                                }

                                inDir = args[1];
                                outDir = args[2];
                                ValidateTaskCount(args[3], out taskCount);
                                ValidateTargetRatiosFile(args[4], out targetRatios);
                                foreach (TargetProfile tp in targetRatios.targetRatios)
                                {
                                    RDResourceProcessor rdResourceProcessor = new RDResourceProcessor(inDir, outDir + "-" + tp.name);
                                    rdResourceProcessor.Process(taskCount, tp);
                                }

                                ret = 0;
                            }

                            break;
                        case verifyFileCommand:
                            {
                                int taskCount;
                                TargetRatios targetRatios;
                                string inDir;

                                if (args.Length != 4)
                                {
                                    throw new Exception("Invalid number of input parameters.");
                                }

                                inDir = args[1];
                                ValidateTaskCount(args[2], out taskCount);
                                ValidateTargetRatiosFile(args[3], out targetRatios);
                                foreach (TargetProfile tp in targetRatios.targetRatios)
                                {
                                    RDResourceProcessor rdResourceProcessor = new RDResourceProcessor(inDir + "-" + tp.name, null);
                                    rdResourceProcessor.Process(taskCount, tp);
                                }

                                ret = 0;
                            }

                            break;
                        case targetCommand:
                            {
                                if (args.Length != 6)
                                {
                                    throw new Exception("Invalid number of input parameters.");
                                }

                                string blobGroupsInfoPath = args[1];
                                string oneGroupInfoPath = args[2];
                                string blendRatiosFilePath = args[3];
                                string targetRatiosPath = args[4];
                                string targetRatiosPathCsv = args[5];
                                CalculatorTargetRatios.Calculate(blobGroupsInfoPath, oneGroupInfoPath, blendRatiosFilePath, targetRatiosPath, targetRatiosPathCsv);
                                ret = 0;
                            }

                            break;
                        case importCommand:
                            {
                                if (args.Length != 7)
                                {
                                    throw new Exception("Invalid number of input parameters.");
                                }

                                string serverUrl = args[1];
                                string resourceGroupCount = args[2];
                                string inputUrl = args[3];
                                string inputBlobContainerName = args[4];
                                string importResultFileName = args[5];
                                string inputConnectionString = args[6];
                                ServerImport.Import(serverUrl, resourceGroupCount, inputUrl, inputBlobContainerName, importResultFileName, inputConnectionString).Wait();
                                ret = 0;
                            }

                            break;
                        case isFinishedCommand:
                            {
                                if (args.Length != 2)
                                {
                                    throw new Exception("Invalid number of input parameters.");
                                }

                                string importResultFileName = args[1];
                                Task<bool> t = ServerImport.IsImportFinished(importResultFileName);
                                t.Wait();
                                if (t.Result)
                                {
                                    ret = 0;
                                }
                                else
                                {
                                    ret = 1; // Retry later.
                                }
                            }

                            break;
                        default:
                            {
                                throw new Exception($"Invalid command '{args[0]}'.");
                            }
                    }
                }

                Console.WriteLine($"End time: {DateTime.Now}");
                Console.WriteLine($"Execution time: {DateTime.Now - startTime}");

                // Console.WriteLine("Press enter to close application.");
                // Console.ReadLine();
                return ret;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR! {ex.Message}");

                // Console.WriteLine("Press enter to close application.");
                // Console.ReadLine();
                return -1;
            }
        }
    }
}
