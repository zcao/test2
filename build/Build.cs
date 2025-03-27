using System;
using System.IO.Compression;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.AzurePipelines;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Utilities.Collections;
using Nuke.Common.Tools.MSBuild;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using System.IO;
using Nuke.Common.Tools.MinVer;
using Nuke.Common.Tools.DotNet;
using System.Diagnostics;
using Nuke.Common.CI.GitHubActions;
[GitHubActions("test1", GitHubActionsImage.UbuntuLatest, On = new[] { GitHubActionsTrigger.Push }, InvokedTargets = new[] { nameof(Test) })]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main () => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration;

    [Solution(GenerateProjects = true)]

    readonly Solution Solution;

    AzurePipelines AzurePipelines => AzurePipelines.Instance;

    readonly MinVer MinVer;

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            Console.WriteLine("Cleaning...");
            DotNetTasks.DotNetClean();
        });

    Target Restore => _ => _
    // .DependsOn(Clean)
        .Executes(() =>
        {
            // Debugger.Launch();
            Console.WriteLine("Restoring...");
            DotNetTasks.DotNetRestore();
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {  
             Console.WriteLine($"Building...{Solution.Name} in {RootDirectory} {Configuration}MinVer{MinVer}");

            DotNetTasks.DotNetBuild();
            MSBuildTasks.MSBuild(s => s
                .SetTargetPath(Solution)
                .SetConfiguration(Configuration.Debug));
         
        });
    Target Test => _ => _
        // .DependsOn(Compile)
        .Executes(() =>
        {
            Console.WriteLine("Running tests...");
            DotNetTasks.DotNetTest(s => s
                .SetProjectFile("AAAA")
                .SetFilter("TestCategory=PB_TEST")
                .SetLoggers("nunit;LogFilePath=../_Reports/results.xml")
                .SetConfiguration(Configuration.Debug));
        });
        
    Target Publish => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            Console.WriteLine($"Building...{Solution.Name} in {RootDirectory} {Configuration} MinVer{MinVer}");
            var debugFolder = RootDirectory/"ConsoleApp1" / "bin" / "Debug";
            var zipFile = RootDirectory / "debug.zip";
            if (File.Exists(zipFile))
            {
                File.Delete(zipFile);
            }
            ZipFile.CreateFromDirectory(debugFolder, zipFile);
        
        });

}
