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
using Nuke.Common.Tools.Docker;
using static Nuke.Common.Tools.Docker.DockerTasks;

[GitHubActions("test1", GitHubActionsImage.UbuntuLatest, 
On = new[] { GitHubActionsTrigger.Push }, 
PublishArtifacts =true,
InvokedTargets = new[] { nameof(Test) },
ImportSecrets = new[] { "MySecret" }, AutoGenerate =false)]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    [Parameter("Docker user")]
    readonly string MySecret;

    [Parameter("Docker Hub username")] readonly string DockerUsername;
    [Parameter("Docker Hub password")] readonly string DockerPassword;

    public static int Main () => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration;

    [Solution(GenerateProjects = true)]
    readonly Solution Solution;

    [CI] readonly GitHubActions GitHubActions;

    Project MainProject => Solution.GetProject("ConsoleApp1");
    AbsolutePath OutputDirectory => RootDirectory / "output";
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";

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
             Console.WriteLine($"Building...{Solution.Name} in {RootDirectory} {Configuration}");

            DotNetTasks.DotNetBuild();
            MSBuildTasks.MSBuild(s => s
                .SetTargetPath(Solution)
                .SetConfiguration(Configuration.Debug));
         
        });
    Target Test => _ => _
        .DependsOn(Compile)
        .Produces(ArtifactsDirectory / "*.zip")  // Declare artifact outputs
        .Executes(() =>
        {            
            // Create artifacts directory
            ArtifactsDirectory.CreateOrCleanDirectory();
            Debugger.Launch();
            // Create a zip of the build output
            var framework = MainProject.GetTargetFrameworks().FirstOrDefault() ?? "net6.0";
            Console.WriteLine($"framework ={framework}...");
            var buildOutput = MainProject.Directory / "bin" / Configuration / framework;
            ZipFile.CreateFromDirectory(
                buildOutput,
                ArtifactsDirectory / "build-output.zip");
            
            return ;
            // Continue with existing Docker operations
            
            var dockerFilePath = RootDirectory / "Dockerfile";
            var imageName = "ohyesboy/consoleapp1";

            if (!File.Exists(dockerFilePath))
            {
                Console.WriteLine("Dockerfile not found.");
                return;
            }

            DockerTasks.DockerBuild(s => s
                .SetPath(".")
                .SetFile(dockerFilePath)
                .SetTag(imageName)
                .SetProcessWorkingDirectory(RootDirectory));

            DockerTasks.DockerPush(s => s
                .SetName(imageName));


        });

    Target Publish => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            Console.WriteLine($"Building...{Solution.Name} in {RootDirectory} {Configuration} ");
            var debugFolder = RootDirectory/"ConsoleApp1" / "bin" / "Debug";
            var zipFile = RootDirectory / "debug.zip";
            if (File.Exists(zipFile))
            {
                File.Delete(zipFile);
            }
            ZipFile.CreateFromDirectory(debugFolder, zipFile);
        
        });

    Target DockerLogin2 => _ => _
        .Executes(() =>
        {
            Console.WriteLine("DockerPassword=" +DockerPassword);
            DockerLogin(_ => _
                .SetUsername(DockerUsername)
                .SetPassword(DockerPassword));
        });

    Target BuildAndPushDocker => _ => _
        // .DependsOn(DockerLogin2)
        .Executes(() =>
        {
            DockerBuild(_ => _
                .SetPath(".")
                .SetTag($"{DockerUsername}/consoleapp1:latest"));

            DockerPush(_ => _
                .SetName($"{DockerUsername}/consoleapp1:latest"));
        });
}
