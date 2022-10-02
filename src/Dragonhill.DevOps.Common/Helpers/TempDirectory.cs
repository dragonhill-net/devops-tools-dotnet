namespace Dragonhill.DevOps.Helpers;

public class TempDirectory : IDisposable
{
    public string RootPath { get; }

    public TempDirectory()
    {
        RootPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(RootPath);
    }
    
    public void Dispose()
    {
        Directory.Delete(RootPath, true);
    }
}
