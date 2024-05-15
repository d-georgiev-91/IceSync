namespace IceSync.CommonAbstractions
{
    public interface IFileSystem
    {
        string PathCombine(params string[] paths);

        bool FileExists(string? path);

        Task<string?> ReadAllTextAsync(string path, CancellationToken cancellationToken = default);

        string? GetDirectoryName(string path);

        bool DirectoryExists(string? path);

        void CreateDirectory(string path);

        Task WriteAllTextAsync(string path, string? contents);
    }
}
