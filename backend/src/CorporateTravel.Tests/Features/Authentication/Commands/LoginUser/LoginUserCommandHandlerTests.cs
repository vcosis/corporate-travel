using Xunit;
using CorporateTravel.Application.Dtos;
using CorporateTravel.Application.Features.Authentication.Commands.LoginUser;
using CorporateTravel.Application.Interfaces;
using CorporateTravel.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CorporateTravel.Tests.Features.Authentication.Commands.LoginUser;

public class LoginUserCommandHandlerTests
{
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<ITokenService> _mockTokenService;
    private readonly LoginUserCommandHandler _handler;

    public LoginUserCommandHandlerTests()
    {
        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        _mockUserManager = new Mock<UserManager<ApplicationUser>>(
            userStoreMock.Object, 
            new Mock<IOptions<IdentityOptions>>().Object, 
            new Mock<IPasswordHasher<ApplicationUser>>().Object, 
            new List<IUserValidator<ApplicationUser>>(), 
            new List<IPasswordValidator<ApplicationUser>>(), 
            new Mock<ILookupNormalizer>().Object, 
            new Mock<IdentityErrorDescriber>().Object, 
            new Mock<IServiceProvider>().Object, 
            new Mock<ILogger<UserManager<ApplicationUser>>>().Object);

        _mockTokenService = new Mock<ITokenService>();
        _handler = new LoginUserCommandHandler(_mockUserManager.Object, _mockTokenService.Object);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldReturnAuthResponse()
    {
        // Arrange
        var command = new LoginUserCommand
        {
            Email = "test@example.com",
            Password = "Password123!"
        };

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Name = "Test User",
            UserName = "test@example.com"
        };

        var roles = new List<string> { "User" };
        var token = "valid.jwt.token";

        _mockUserManager.Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync(user);

        _mockUserManager.Setup(x => x.CheckPasswordAsync(user, command.Password))
            .ReturnsAsync(true);

        _mockUserManager.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(roles);

        _mockTokenService.Setup(x => x.GenerateTokenAsync(user))
            .ReturnsAsync(token);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Token.Should().Be(token);
        result.Name.Should().Be(user.Name);
        result.Email.Should().Be(user.Email);
        result.Roles.Should().BeEquivalentTo(roles);
        result.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddHours(8), TimeSpan.FromMinutes(1));

        _mockUserManager.Verify(x => x.FindByEmailAsync(command.Email), Times.Once);
        _mockUserManager.Verify(x => x.CheckPasswordAsync(user, command.Password), Times.Once);
        _mockUserManager.Verify(x => x.GetRolesAsync(user), Times.Once);
        _mockTokenService.Verify(x => x.GenerateTokenAsync(user), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidEmail_ShouldReturnNull()
    {
        // Arrange
        var command = new LoginUserCommand
        {
            Email = "invalid@example.com",
            Password = "Password123!"
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeNull();

        _mockUserManager.Verify(x => x.FindByEmailAsync(command.Email), Times.Once);
        _mockUserManager.Verify(x => x.CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        _mockUserManager.Verify(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()), Times.Never);
        _mockTokenService.Verify(x => x.GenerateTokenAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithInvalidPassword_ShouldReturnNull()
    {
        // Arrange
        var command = new LoginUserCommand
        {
            Email = "test@example.com",
            Password = "wrongpassword"
        };

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Name = "Test User",
            UserName = "test@example.com"
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync(user);

        _mockUserManager.Setup(x => x.CheckPasswordAsync(user, command.Password))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeNull();

        _mockUserManager.Verify(x => x.FindByEmailAsync(command.Email), Times.Once);
        _mockUserManager.Verify(x => x.CheckPasswordAsync(user, command.Password), Times.Once);
        _mockUserManager.Verify(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()), Times.Never);
        _mockTokenService.Verify(x => x.GenerateTokenAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithMultipleRoles_ShouldReturnAllRoles()
    {
        // Arrange
        var command = new LoginUserCommand
        {
            Email = "admin@example.com",
            Password = "Password123!"
        };

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "admin@example.com",
            Name = "Admin User",
            UserName = "admin@example.com"
        };

        var roles = new List<string> { "Admin", "Manager", "User" };
        var token = "valid.jwt.token";

        _mockUserManager.Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync(user);

        _mockUserManager.Setup(x => x.CheckPasswordAsync(user, command.Password))
            .ReturnsAsync(true);

        _mockUserManager.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(roles);

        _mockTokenService.Setup(x => x.GenerateTokenAsync(user))
            .ReturnsAsync(token);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Roles.Should().HaveCount(3);
        result.Roles.Should().Contain("Admin");
        result.Roles.Should().Contain("Manager");
        result.Roles.Should().Contain("User");
    }

    [Fact]
    public async Task Handle_WithNullUserEmail_ShouldHandleGracefully()
    {
        // Arrange
        var command = new LoginUserCommand
        {
            Email = "test@example.com",
            Password = "Password123!"
        };

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = null,
            Name = "Test User",
            UserName = "test@example.com"
        };

        var roles = new List<string> { "User" };
        var token = "valid.jwt.token";

        _mockUserManager.Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync(user);

        _mockUserManager.Setup(x => x.CheckPasswordAsync(user, command.Password))
            .ReturnsAsync(true);

        _mockUserManager.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(roles);

        _mockTokenService.Setup(x => x.GenerateTokenAsync(user))
            .ReturnsAsync(token);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().BeNull();
    }
} 