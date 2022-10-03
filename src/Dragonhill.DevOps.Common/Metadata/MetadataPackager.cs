using Dragonhill.DevOps.Helpers;
using Dragonhill.DevOps.Metadata.Dto;
using Dragonhill.DevOps.Metadata.Dto.Validators;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Dragonhill.DevOps.Metadata;

public class MetadataPackager : IDisposable
{
    private const string NugetOutputDir = "metadata";
    private const string LanguageMetadataFilename = ".meta.yaml";
    private static readonly Regex LanguageRegex = new("^[a-zA-Z][a-zA-Z0-9]*$", RegexOptions.Compiled);

    private readonly NuGetVersion _version;
    private readonly string _metadataRootDirectory;
    private readonly string _outputDirectory;
    private readonly DevopsMetaDto _config;

    private readonly IDeserializer _deserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();
    
    private readonly ISerializer _serializer = new SerializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();

    private readonly TempDirectory _tempDirectory = new();

    public MetadataPackager(string version, string metadataRootDirectory, string outputDirectory)
    {
        _version = new NuGetVersion(version);
        _metadataRootDirectory = metadataRootDirectory;
        _outputDirectory = outputDirectory;

        var configPath = Path.Combine(_metadataRootDirectory, ".config", "devops-meta.yaml");
        if (!File.Exists(configPath))
        {
            throw new DirectoryNotFoundException($"Required configuration file not found: '{configPath}'");
        }

        _config = _deserializer.Deserialize<DevopsMetaDto>(File.ReadAllText(configPath));

        var validationResult = new DevopsMetaDtoValidator().Validate(_config);
        if (!validationResult.IsValid)
        {
            throw new InvalidOperationException($"Config file ('{configPath}') validation error: {validationResult}");
        }
    }

    public void Run()
    {
        BuildTempLanguagesDir();
        BuildPackage();
    }

    private void BuildPackage()
    {
        var builder = new PackageBuilder
            {
                Id = _config.Nuget.PackageName,
                Version = _version,
                Description = "Containing build metadata in a custom nuget package format",
                PackageTypes = new List<PackageType>
                    {
                        new("Dragonhill.Devops.Meta", new Version(0,1))
                    },
                Repository = new RepositoryMetadata
                    {
                        Type = _config.Nuget.RepositoryType,
                        Url = _config.Nuget.RepositoryUrl
                    }
            };

        foreach (var author in _config.Nuget.Authors)
        {
            builder.Authors.Add(author);
        }
        
        builder.AddFiles(_tempDirectory.RootPath, Path.Join(_tempDirectory.RootPath, "**"), NugetOutputDir);

        Directory.CreateDirectory(_outputDirectory);

        var packagePath = Path.Combine(_outputDirectory, $"{_config.Nuget.PackageName}.{_version.ToString()}.nupkg");
        using(var fileStream = File.OpenWrite(packagePath))
        {
            builder.Save(fileStream);
        }
        
        Console.WriteLine($"Package written to: {packagePath}");
    }

    private void BuildTempLanguagesDir()
    {
        var languagesSubdirectory = Path.Combine(_metadataRootDirectory, "languages");

        if (!Directory.Exists(languagesSubdirectory))
        {
            throw new DirectoryNotFoundException($"Required languages subdirectory not found: '{languagesSubdirectory}'");
        }
        
        var languagesTempPath = Path.Combine(_tempDirectory.RootPath, "languages");
        Directory.CreateDirectory(languagesTempPath);
        
        CopyGitignorePartial(languagesSubdirectory, languagesTempPath);
        CopyEditorconfigPartial(languagesSubdirectory, languagesTempPath, "*");

        var languageDirectories = Directory.GetDirectories(languagesSubdirectory);

        Dictionary<string, LanguageOutputMetadata> languageMetadataDict = new(languageDirectories.Length);

        foreach (var languageDirectory in languageDirectories)
        {
            var languageMetadata = ParseLanguage(languageDirectory, languagesTempPath);
            languageMetadataDict.Add(languageMetadata.CanonicalExtension, languageMetadata);
        }
        
        //Validate dependencies
        foreach (var languageMetadata in languageMetadataDict.Values)
        {
            foreach (var dependantLanguage in languageMetadata.DependantLanguages)
            {
                if (!languageMetadataDict.ContainsKey(dependantLanguage))
                {
                    throw new InvalidOperationException($"No configuration for dependant language '{dependantLanguage}' of language '{languageMetadata.CanonicalExtension}' is found. (Note: always use the canonical extension name!)");
                }
            }
        }
    }

    private LanguageOutputMetadata ParseLanguage(string languageDirectory, string languagesTempRoot)
    {
        var language = Path.GetFileName(languageDirectory);
            
        if (!LanguageRegex.IsMatch(language))
        {
            throw new InvalidOperationException($"The language folder '{language}' is not in the pattern format '{LanguageRegex}'");
        }

        LanguageMetaDto? languageMetaInput = null;
        var languageMetaPath = Path.Combine(languageDirectory, "language-meta.yaml");
        if (File.Exists(languageMetaPath))
        {
            try
            {
                languageMetaInput = _deserializer.Deserialize<LanguageMetaDto>(File.ReadAllText(languageMetaPath));
            }
            catch(Exception exception)
            {
                throw new FormatException($"Invalid format of '{languageMetaPath}'", exception);
            }
        }

        LanguageOutputMetadata languageOutputMetadata = new(language, languageMetaInput);

        var languageTempPath = Path.Combine(languagesTempRoot, language);
        Directory.CreateDirectory(languageTempPath);

        CopyGitignorePartial(languageDirectory, languageTempPath);
        CopyEditorconfigPartial(languageDirectory, languageTempPath, language);

        var languageMetadataFilePath = Path.Join(languageTempPath, LanguageMetadataFilename);
        using var writer = File.OpenWrite(languageMetadataFilePath);
        using var textWriter = new StreamWriter(writer);
        _serializer.Serialize(textWriter, languageOutputMetadata);

        return languageOutputMetadata;
    }

    private static void CopyGitignorePartial(string sourceDirectory, string targetDirectory)
    {
        var sourceGitignorePath = Path.Combine(sourceDirectory, Filetypes.Gitignore);
        if (File.Exists(sourceGitignorePath))
        {
            File.Copy(sourceGitignorePath, Path.Combine(targetDirectory, Filetypes.Gitignore));
        }
    }

    private static void CopyEditorconfigPartial(string sourceDirectory, string targetDirectory, string expectedLanguage)
    {
        var sourceEditorconfigPath = Path.Combine(sourceDirectory, Filetypes.Editorconfig);
        if (File.Exists(sourceEditorconfigPath))
        {
            var targetEditorconfigPath = Path.Combine(targetDirectory, Filetypes.Editorconfig);
            File.Copy(sourceEditorconfigPath, targetEditorconfigPath);
            EditorconfigValidator.Validate(targetEditorconfigPath, expectedLanguage);
        }
    }

    public void Dispose()
    {
        _tempDirectory.Dispose();
    }
}
