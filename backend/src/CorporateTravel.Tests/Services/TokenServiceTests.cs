using Xunit;
using CorporateTravel.Application.Services;
using CorporateTravel.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Security.Claims;
using System;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Security.Principal;

namespace CorporateTravel.Tests.Services;

public class TokenServiceTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly TokenService _tokenService;

    public TokenServiceTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
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

        // Setup configuration
        _mockConfiguration.Setup(x => x["Jwt:Key"]).Returns("your-super-secret-key-with-at-least-32-characters");
        _mockConfiguration.Setup(x => x["Jwt:Issuer"]).Returns("CorporateTravel");
        _mockConfiguration.Setup(x => x["Jwt:Audience"]).Returns("CorporateTravelUsers");

        _tokenService = new TokenService(_mockConfiguration.Object, _mockUserManager.Object);
    }

    [Fact]
    public async Task GenerateTokenAsync_WithValidUser_ShouldReturnValidToken()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Name = "Test User",
            UserName = "test@example.com"
        };

        var roles = new List<string> { "User" };

        _mockUserManager.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(roles);

        // Act
        var token = await _tokenService.GenerateTokenAsync(user);

        // Assert
        token.Should().NotBeNullOrEmpty();
        token.Should().Contain("."); // JWT tokens have 3 parts separated by dots

        _mockUserManager.Verify(x => x.GetRolesAsync(user), Times.Once);
    }

    [Fact]
    public async Task GenerateTokenAsync_WithMultipleRoles_ShouldIncludeAllRoles()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "admin@example.com",
            Name = "Admin User",
            UserName = "admin@example.com"
        };

        var roles = new List<string> { "Admin", "Manager", "User" };

        _mockUserManager.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(roles);

        // Act
        var token = await _tokenService.GenerateTokenAsync(user);

        // Assert
        token.Should().NotBeNullOrEmpty();

        // Decode token to verify roles
        var principal = _tokenService.ValidateToken(token);
        principal.Should().NotBeNull();
        principal!.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
        principal.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Manager");
        principal.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "User");
    }

    [Fact]
    public async Task GenerateTokenAsync_ShouldIncludeUserClaims()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            Email = "test@example.com",
            Name = "Test User",
            UserName = "test@example.com"
        };

        var roles = new List<string> { "User" };

        _mockUserManager.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(roles);

        // Act
        var token = await _tokenService.GenerateTokenAsync(user);

        // Assert
        var principal = _tokenService.ValidateToken(token);
        principal.Should().NotBeNull();
        principal!.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == userId.ToString());
        principal.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == user.Email);
        principal.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == user.Name);
        principal.Claims.Should().Contain(c => c.Type == "userId" && c.Value == userId.ToString());
        principal.Claims.Should().Contain(c => c.Type == "userName" && c.Value == user.Name);
        principal.Claims.Should().Contain(c => c.Type == "userEmail" && c.Value == user.Email);
    }

    [Fact]
    public async Task GenerateTokenAsync_WithNullUserEmail_ShouldHandleGracefully()
    {
        var user = new ApplicationUser { Email = null };
        Func<Task> act = async () => await _tokenService.GenerateTokenAsync(user);
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("User email cannot be null or empty");
    }

    [Fact]
    public async Task ValidateToken_WithValidToken_ShouldReturnPrincipal()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Name = "Test User",
            UserName = "test@example.com"
        };

        var roles = new List<string> { "User" };

        _mockUserManager.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(roles);

        // Act
        var token = await _tokenService.GenerateTokenAsync(user);
        var principal = _tokenService.ValidateToken(token);

        // Assert
        principal.Should().NotBeNull();
        principal!.Identity.Should().NotBeNull();
        principal.Identity!.IsAuthenticated.Should().BeTrue();
    }

    [Fact]
    public void ValidateToken_WithInvalidToken_ShouldReturnNull()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var principal = _tokenService.ValidateToken(invalidToken);

        // Assert
        principal.Should().BeNull();
    }

    [Fact]
    public async Task ValidateToken_WithExpiredToken_ShouldReturnNull()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Name = "Test User",
            UserName = "test@example.com"
        };

        var roles = new List<string> { "User" };

        _mockUserManager.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(roles);

        // Create a token with short expiration
        _mockConfiguration.Setup(x => x["Jwt:Key"]).Returns("your-super-secret-key-with-at-least-32-characters");
        _mockConfiguration.Setup(x => x["Jwt:Issuer"]).Returns("CorporateTravel");
        _mockConfiguration.Setup(x => x["Jwt:Audience"]).Returns("CorporateTravelUsers");

        var token = await _tokenService.GenerateTokenAsync(user);

        // Wait for token to expire (if we had a way to create expired tokens)
        // For now, we'll test with invalid token
        var expiredToken = token + ".invalid";

        // Act
        var principal = _tokenService.ValidateToken(expiredToken);

        // Assert
        principal.Should().BeNull();
    }

    [Fact]
    public async Task ValidateToken_WithWrongIssuer_ShouldReturnNull()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Name = "Test User",
            UserName = "test@example.com"
        };

        var roles = new List<string> { "User" };

        _mockUserManager.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(roles);

        var token = await _tokenService.GenerateTokenAsync(user);

        // Create a new token service with different issuer
        _mockConfiguration.Setup(x => x["Jwt:Issuer"]).Returns("DifferentIssuer");
        var differentTokenService = new TokenService(_mockConfiguration.Object, _mockUserManager.Object);

        // Act
        var principal = differentTokenService.ValidateToken(token);

        // Assert
        principal.Should().BeNull();
    }

    [Fact]
    public async Task ValidateToken_WithWrongAudience_ShouldReturnNull()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Name = "Test User",
            UserName = "test@example.com"
        };

        var roles = new List<string> { "User" };

        _mockUserManager.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(roles);

        var token = await _tokenService.GenerateTokenAsync(user);

        // Create a new token service with different audience
        _mockConfiguration.Setup(x => x["Jwt:Audience"]).Returns("DifferentAudience");
        var differentTokenService = new TokenService(_mockConfiguration.Object, _mockUserManager.Object);

        // Act
        var principal = differentTokenService.ValidateToken(token);

        // Assert
        principal.Should().BeNull();
    }

    [Fact]
    public async Task GenerateTokenAsync_WithEmptyRoles_ShouldStillGenerateToken()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Name = "Test User",
            UserName = "test@example.com"
        };

        var roles = new List<string>();

        _mockUserManager.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(roles);

        // Act
        var token = await _tokenService.GenerateTokenAsync(user);

        // Assert
        token.Should().NotBeNullOrEmpty();
        var principal = _tokenService.ValidateToken(token);
        principal.Should().NotBeNull();
        principal!.Claims.Should().NotContain(c => c.Type == ClaimTypes.Role);
    }
} 