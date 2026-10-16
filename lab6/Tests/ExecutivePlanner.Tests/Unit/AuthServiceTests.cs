using ExecutivePlanner.Helpers;
using ExecutivePlanner.Models;
using ExecutivePlanner.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace ExecutivePlanner.Tests.Unit;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _users = new();

    private AuthService CreateService() => new(_users.Object, NullLogger<AuthService>.Instance);

    private void SetupUser(string login, string password, UserRole role) =>
        _users.Setup(u => u.GetByLoginAsync(login)).ReturnsAsync(OperationResult<User?>.Ok(
            new User { Id = 1, Login = login, PasswordHash = PasswordHasher.Hash(password), Role = role }));

    [Fact]
    public async Task Login_WithCorrectPassword_SetsCurrentUserAndRole()
    {
        // Arrange
        SetupUser("manager", "pass123", UserRole.Manager);
        var service = CreateService();

        // Act
        var ok = await service.LoginAsync("manager", "pass123");

        // Assert
        Assert.True(ok);
        Assert.True(service.IsAuthenticated);
        Assert.True(service.IsInRole(UserRole.Manager));
        Assert.False(service.IsInRole(UserRole.Assistant));
    }

    [Fact]
    public async Task Login_WithWrongPassword_Fails()
    {
        SetupUser("manager", "pass123", UserRole.Manager);
        var service = CreateService();

        Assert.False(await service.LoginAsync("manager", "wrong"));
        Assert.False(service.IsAuthenticated);
    }

    [Fact]
    public async Task Login_UnknownUser_Fails()
    {
        _users.Setup(u => u.GetByLoginAsync("ghost")).ReturnsAsync(OperationResult<User?>.Ok(null));

        Assert.False(await CreateService().LoginAsync("ghost", "x"));
    }

    [Fact]
    public async Task Login_WhenRepositoryFails_Fails()
    {
        _users.Setup(u => u.GetByLoginAsync("manager")).ReturnsAsync(OperationResult<User?>.Fail());

        Assert.False(await CreateService().LoginAsync("manager", "pass123"));
    }

    [Theory]
    [InlineData("", "pass")]
    [InlineData("   ", "pass")]
    [InlineData("manager", "")]
    public async Task Login_WithEmptyCredentials_FailsWithoutCallingRepository(string login, string password)
    {
        Assert.False(await CreateService().LoginAsync(login, password));
        _users.Verify(u => u.GetByLoginAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Logout_ClearsCurrentUser()
    {
        SetupUser("assistant", "pass123", UserRole.Assistant);
        var service = CreateService();
        await service.LoginAsync("assistant", "pass123");

        service.Logout();

        Assert.False(service.IsAuthenticated);
        Assert.False(service.IsInRole(UserRole.Assistant));
    }
}
