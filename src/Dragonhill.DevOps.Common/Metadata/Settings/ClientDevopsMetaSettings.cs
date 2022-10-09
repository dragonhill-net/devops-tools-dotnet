namespace Dragonhill.DevOps.Metadata.Settings;

public class ClientDevopsMetaSettings
{
    public string RepositoryUrl { get; set; }
    public string Package { get; set; }
    public string[] Languages { get; set; }

    public bool UsePrerelease { get; set; }
}
