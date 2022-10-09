using Dragonhill.DevOps.Metadata;
using System.CommandLine;

namespace Dragonhill.DevOps.Cli;

public static class MetadataCommands
{
    public static Command CreateCommand()
    {
        var command = new Command("metadata", "Commands supporting devops metadata creation and management");

        command.AddCommand(CreateCreatePackageCommand());
        command.AddCommand(CreateApplyCommand());
        
        return command;
    }

    private static Command CreateCreatePackageCommand()
    {
        var createPackageCommand = new Command("create-package", "Creates a metadata nuget package");
        
        var createPackageVersionArgument = new Argument<string>("version", "The package version to be created");
        createPackageCommand.AddArgument(createPackageVersionArgument);

        var createPackageRootPathOption = new Option<DirectoryInfo?>("--root-path", () => null, "The root of the metadata repository to be parsed (defaults to current directory)");
        createPackageCommand.AddOption(createPackageRootPathOption);

        var createPackageOutputOption = new Option<DirectoryInfo?>("--output", "The output directory to write the package to (defaults to current directory)");
        createPackageCommand.AddOption(createPackageOutputOption);
        
        createPackageCommand.SetHandler((versionArgument, rootPathArgument, outputArgument) =>
        {
            var rootPath = rootPathArgument?.FullName ?? Directory.GetCurrentDirectory();
            var outputDirectory = outputArgument?.FullName ?? Directory.GetCurrentDirectory();
            
            using var metadataPackager = new MetadataPackager(versionArgument, rootPath, outputDirectory);
            metadataPackager.Run();
            
        }, createPackageVersionArgument, createPackageRootPathOption, createPackageOutputOption);

        return createPackageCommand;
    }

    private static Command CreateApplyCommand()
    {
        var applyCommand = new Command("apply", "Applies a metadata nuget package");
        
        var applyRootPathOption = new Option<DirectoryInfo?>("--root-path", () => null, "The root directory (default to current directory)");
        applyCommand.AddOption(applyRootPathOption);
        
        applyCommand.SetHandler(async (rootPathArgument) =>
        {
            var rootPath = rootPathArgument?.FullName ?? Directory.GetCurrentDirectory();

            using var metadataDownloader = new MetadataDownloader(rootPath);
            
            await metadataDownloader.Apply();

        }, applyRootPathOption);

        return applyCommand;
    }
}
