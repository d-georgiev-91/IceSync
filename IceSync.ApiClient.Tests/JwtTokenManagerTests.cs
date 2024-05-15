using IceSync.CommonAbstractions;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using NUnit.Framework;

namespace IceSync.ApiClient.Tests;

[TestFixture]
public class JwtTokenManagerTests
{
    private IFileSystem _fileSystem;
    private IEnvironment _environment;
    private IJwtSecurityTokenHandler _tokenHandler;
    private IDateTimeService _dateTimeService;
    private JwtTokenManager _jwtTokenManager;

    private const string TokenFilePath = @"C:\Users\UserName\AppData\Local\IceSync\jwt-token";
    private const string Token = "mock-jwt-token";

    [SetUp]
    public void SetUp()
    {
        _fileSystem = Substitute.For<IFileSystem>();
        _environment = Substitute.For<IEnvironment>();
        _tokenHandler = Substitute.For<IJwtSecurityTokenHandler>();
        _dateTimeService = Substitute.For<IDateTimeService>();

        _environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData).Returns(@"C:\Users\UserName\AppData\Local");
        _fileSystem.PathCombine(Arg.Any<string[]>()).Returns(TokenFilePath);

        _jwtTokenManager = new JwtTokenManager(_fileSystem, _environment, _tokenHandler, _dateTimeService);
    }

    [Test]
    public void IsTokenExpired_TokenIsEmptyString_ReturnsTrue()
    {
        var result = _jwtTokenManager.IsTokenExpired(string.Empty);

        Assert.That(result, Is.True);
    }

    [Test]
    public void IsTokenExpired_TokenIsExpired_ReturnsTrue()
    {
        var jwtToken = Substitute.For<SecurityToken>();
        jwtToken.ValidTo.Returns(DateTime.UtcNow.AddMinutes(-1));
        _tokenHandler.ReadToken(Token).Returns(jwtToken);
        _dateTimeService.UtcNow.Returns(DateTime.UtcNow);

        var result = _jwtTokenManager.IsTokenExpired(Token);

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task IsTokenExpired_TokenIsNotExpired_ReturnsFalse()
    {
        var jwtToken = Substitute.For<SecurityToken>();
        jwtToken.ValidTo.Returns(DateTime.UtcNow.AddMinutes(15));
        _fileSystem.FileExists(Arg.Any<string>()).Returns(true);
        _fileSystem.ReadAllTextAsync(Arg.Any<string>()).Returns(Token);
        _tokenHandler.ReadToken(Token).Returns(jwtToken);
        _dateTimeService.UtcNow.Returns(DateTime.UtcNow);
        await _jwtTokenManager.LoadTokenAsync();

        var result = _jwtTokenManager.IsTokenExpired(Token);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task LoadTokenAsync_TokenIsLoaded_ReturnsToken()
    {
        await _jwtTokenManager.SaveTokenAsync(Token);
        _fileSystem.FileExists(TokenFilePath).Returns(true);
        _fileSystem.ReadAllTextAsync(TokenFilePath).Returns(Task.FromResult<string?>(Token));

        var result = await _jwtTokenManager.LoadTokenAsync();

        Assert.That(Token, Is.EqualTo(result));
    }

    [Test]
    public async Task LoadTokenAsync_TokenIsNotLoaded_ReadsTokenFromFileSystemAndReturnsToken()
    {
        _fileSystem.FileExists(TokenFilePath).Returns(true);
        _fileSystem.ReadAllTextAsync(TokenFilePath).Returns(Task.FromResult<string?>(Token));

        var result = await _jwtTokenManager.LoadTokenAsync();

        Assert.That(Token, Is.EqualTo(result));
    }

    [Test]
    public async Task LoadTokenAsync_TokenFileDoesNotExist_ReturnsNull()
    {
        _fileSystem.FileExists(TokenFilePath).Returns(false);

        var result = await _jwtTokenManager.LoadTokenAsync();

        Assert.That(result, Is.Null);
    }


    [Test]
    public async Task SaveTokenAsync_IfDirectoryDoesNotExists_CreatesDirectoryAndSavesToken()
    {
        _fileSystem.GetDirectoryName(TokenFilePath).Returns(@"C:\Users\UserName\AppData\Local\IceSync");
        _fileSystem.DirectoryExists(Arg.Any<string>());

        await _jwtTokenManager.SaveTokenAsync(Token);

        _fileSystem.Received().CreateDirectory(@"C:\Users\UserName\AppData\Local\IceSync");
        await _fileSystem.Received().WriteAllTextAsync(TokenFilePath, Token);
    }

    [Test]
    public async Task SaveTokenAsync_IfDirectoryExists_DoesNotCreateDirectoryAndSavesToken()
    {
        _fileSystem.GetDirectoryName(TokenFilePath).Returns(@"C:\Users\UserName\AppData\Local\IceSync");
        _fileSystem.DirectoryExists(Arg.Any<string>()).Returns(true);

        await _jwtTokenManager.SaveTokenAsync(Token);

        _fileSystem.DidNotReceive();
        await _fileSystem.Received().WriteAllTextAsync(TokenFilePath, Token);
    }
}