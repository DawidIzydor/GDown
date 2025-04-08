using GHent.Data;

// Running async causes deadloc in DownloadWorker
public class FileService : IFileService
{
    public bool Exists(string path) => File.Exists(path);

    public Task<string> ReadAllTextAsync(string path) => File.ReadAllTextAsync(path);

    public Task WriteAllTextAsync(string path, string contents) => File.WriteAllTextAsync(path, contents);
}


public class SynchronousFileService : IFileService
{
    public bool Exists(string path) => File.Exists(path);

    public Task<string> ReadAllTextAsync(string path) => Task.FromResult(File.ReadAllText(path));

    public Task WriteAllTextAsync(string path, string contents)
    {
        File.WriteAllText(path, contents);
        return Task.CompletedTask;
    }
}