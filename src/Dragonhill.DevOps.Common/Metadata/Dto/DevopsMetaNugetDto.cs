namespace Dragonhill.DevOps.Metadata.Dto;

public class DevopsMetaNugetDto
{
    public string PackageName { get; set; }
    public string RepositoryType { get; set; }
    public string RepositoryUrl { get; set; }
    public string[] Authors { get; set; }
}
