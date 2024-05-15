namespace IceSync.CommonAbstractions;

public class FileSystem : IFileSystem
{
    public string PathCombine(params string[] paths) => Path.Combine(paths);

    public bool FileExists(string? path) => File.Exists(path);

    public async Task<string?> ReadAllTextAsync(string path, CancellationToken cancellationToken = default) => 
        await File.ReadAllTextAsync(path, cancellationToken);

    public string? GetDirectoryName(string path) => Path.GetDirectoryName(path);

    public bool DirectoryExists(string? path) => Directory.Exists(path);

    public void CreateDirectory(string path) => Directory.CreateDirectory(path);

    public async Task WriteAllTextAsync(string path, string? contents) => await File.WriteAllTextAsync(path, contents);
}