using Microsoft.Build.Evaluation;
using NuGet.CommandLine.XPlat;
using NuGet.Common;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Tool4Grpc
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // arg0: project file
            // arg1: proto file
            var projectPath = @"C:\gh\tp\NewConsole\NewConsole.csproj";
            var proto = @"..\greet.proto";

            // Initialize helper
            var msbuild = new MSBuildAPIUtility(NullLogger.Instance);
            var addCommand = new AddPackageReferenceCommandRunner();

            // Add Google.Protobuf
            await AddDependency(msbuild, addCommand, projectPath, "Google.Protobuf");
            // Add Grpc.Server.AspNetCore
            await AddDependency(msbuild, addCommand, projectPath, "Grpc.Server.AspNetCore");
            // Add Grpc.Tools
            await AddDependency(msbuild, addCommand, projectPath, "Grpc.Tools");
            // TODO: Mark as PrivateAsset=All?

            var project = new Project(projectPath);

            if (!project.Items.Any(i => i.ItemType == "Protobuf" && i.UnevaluatedInclude == proto))
            {
                project.AddItem("Protobuf", proto, new[] { new KeyValuePair<string, string>("GrpcServices", "Server") });
                project.Save();
            }

            foreach (var item in project.Items)
            {
                Console.WriteLine(item.ItemType);
            }
        }

        static async Task<int> AddDependency(MSBuildAPIUtility msbuild, AddPackageReferenceCommandRunner addCommand, string projectPath, string packageName, bool privateAssets = false)
        {
            var tempDgFilePath = Path.GetTempFileName();

            var packageDependency = new PackageDependency(packageName, VersionRange.Parse("*"));
            var packageRefArgs = new PackageReferenceArgs(projectPath, packageDependency, NullLogger.Instance)
            {
                //Frameworks = CommandLineUtility.SplitAndJoinAcrossMultipleValues(frameworks.Values),
                //Sources = CommandLineUtility.SplitAndJoinAcrossMultipleValues(sources.Values),
                //PackageDirectory = packageDirectory.Value(),
                NoRestore = true,
                //NoVersion = noVersion,
                //DgFilePath = tempDgFilePath,
                //Interactive = interactive.HasValue()
            };

            var retVal = await addCommand.ExecuteCommand(packageRefArgs, msbuild);

            if (File.Exists(tempDgFilePath))
            {
                File.Delete(tempDgFilePath);
            }

            return retVal;
        }
    }
}
