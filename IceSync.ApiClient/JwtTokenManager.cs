using IceSync.CommonAbstractions;

namespace IceSync.ApiClient;

public class JwtTokenManager : IJwtTokenManager
{
    private readonly IFileSystem _fileSystem;
    private readonly IJwtSecurityTokenHandler _tokenHandler;
    private readonly IDateTimeService _dateTimeService;
    private readonly string _tokenFilePath;

    private string? _token;

    public JwtTokenManager(IFileSystem fileSystem, IEnvironment environment, IJwtSecurityTokenHandler tokenHandler, IDateTimeService dateTimeService)
    {
        _fileSystem = fileSystem;
        _tokenFilePath = fileSystem.PathCombine(environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData),
            "IceSync",
            "jwt-token");
        _tokenHandler = tokenHandler;
        _dateTimeService = dateTimeService;
    }

    public bool IsTokenExpired(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return true;
        }

        var jwtToken = _tokenHandler.ReadToken(token);

        var expiration = jwtToken.ValidTo;

        return expiration < _dateTimeService.UtcNow;
    }

    public async Task<string?> LoadTokenAsync()
    {
        if (!string.IsNullOrEmpty(_token))
        {
            return _token;
        }

        if (!_fileSystem.FileExists(_tokenFilePath))
        {
            return null;
        }

        _token = await _fileSystem.ReadAllTextAsync(_tokenFilePath);

        return _token;
    }

    public async Task SaveTokenAsync(string token)
    {
        var directory = _fileSystem.GetDirectoryName(_tokenFilePath);

        if (!_fileSystem.DirectoryExists(directory))
        {
            _fileSystem.CreateDirectory(directory!);
        }

        await _fileSystem.WriteAllTextAsync(_tokenFilePath, token);
        _token = token;
    }
}