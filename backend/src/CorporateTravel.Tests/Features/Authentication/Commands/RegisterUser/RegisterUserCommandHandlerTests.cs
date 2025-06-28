using Xunit;
using CorporateTravel.Application.Features.Authentication.Commands.RegisterUser;
using CorporateTravel.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace CorporateTravel.Tests.Features.Authentication.Commands.RegisterUser;

public class RegisterUserCommandHandlerTests
{
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly RegisterUserCommandHandler _handler;

    public RegisterUserCommandHandlerTests()
    {
        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        _mockUserManager = new Mock<UserManager<ApplicationUser>>(
            userStoreMock.Object, null, null, null, null, null, null, null, null);
        _handler = new RegisterUserCommandHandler(_mockUserManager.Object);
    }

    [Fact]
    public async Task Handle_WithValidUserData_ShouldCreateUserSuccessfully()
    {
        // Arrange
        var command = new RegisterUserCommand
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
            Password = "Password123!",
            Role = "User"
        };

        var successResult = IdentityResult.Success;

        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), command.Password))
            .ReturnsAsync(successResult);

        _mockUserManager.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), command.Role))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();

        _mockUserManager.Verify(x => x.CreateAsync(It.Is<ApplicationUser>(u =>
            u.Name == command.Name &&
            u.Email == command.Email &&
            u.UserName == command.Email), command.Password), Times.Once);
        _mockUserManager.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), command.Role), Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyRole_ShouldAssignDefaultUserRole()
    {
        // Arrange
        var command = new RegisterUserCommand
        {
            Name = "Jane Smith",
            Email = "jane.smith@example.com",
            Password = "Password123!",
            Role = ""
        };

        var successResult = IdentityResult.Success;

        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), command.Password))
            .ReturnsAsync(successResult);

        _mockUserManager.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User"))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();

        _mockUserManager.Verify(x => x.CreateAsync(It.Is<ApplicationUser>(u =>
            u.Name == command.Name &&
            u.Email == command.Email &&
            u.UserName == command.Email), command.Password), Times.Once);
        _mockUserManager.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User"), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNullRole_ShouldAssignDefaultUserRole()
    {
        // Arrange
        var command = new RegisterUserCommand
        {
            Name = "Bob Wilson",
            Email = "bob.wilson@example.com",
            Password = "Password123!",
            Role = null
        };

        var successResult = IdentityResult.Success;

        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), command.Password))
            .ReturnsAsync(successResult);

        _mockUserManager.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User"))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();

        _mockUserManager.Verify(x => x.CreateAsync(It.Is<ApplicationUser>(u =>
            u.Name == command.Name &&
            u.Email == command.Email &&
            u.UserName == command.Email), command.Password), Times.Once);
        _mockUserManager.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User"), Times.Once);
    }

    [Fact]
    public async Task Handle_WithCustomRole_ShouldAssignSpecifiedRole()
    {
        // Arrange
        var command = new RegisterUserCommand
        {
            Name = "Admin User",
            Email = "admin@example.com",
            Password = "Password123!",
            Role = "Admin"
        };

        var successResult = IdentityResult.Success;

        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), command.Password))
            .ReturnsAsync(successResult);

        _mockUserManager.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Admin"))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();

        _mockUserManager.Verify(x => x.CreateAsync(It.Is<ApplicationUser>(u =>
            u.Name == command.Name &&
            u.Email == command.Email &&
            u.UserName == command.Email), command.Password), Times.Once);
        _mockUserManager.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Admin"), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserCreationFails_ShouldNotAssignRole()
    {
        // Arrange
        var command = new RegisterUserCommand
        {
            Name = "Invalid User",
            Email = "invalid@example.com",
            Password = "weak",
            Role = "User"
        };

        var failureResult = IdentityResult.Failed(new IdentityError
        {
            Code = "PasswordTooShort",
            Description = "Passwords must be at least 6 characters."
        });

        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), command.Password))
            .ReturnsAsync(failureResult);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors.First().Code.Should().Be("PasswordTooShort");

        _mockUserManager.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>(), command.Password), Times.Once);
        _mockUserManager.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRoleAssignmentFails_ShouldReturnUserCreationResult()
    {
        // Arrange
        var command = new RegisterUserCommand
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
            Password = "Password123!",
            Role = "InvalidRole"
        };

        var successResult = IdentityResult.Success;
        var roleFailureResult = IdentityResult.Failed(new IdentityError
        {
            Code = "InvalidRole",
            Description = "Role 'InvalidRole' does not exist."
        });

        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), command.Password))
            .ReturnsAsync(successResult);

        _mockUserManager.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), command.Role))
            .ReturnsAsync(roleFailureResult);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue(); // Should return the user creation result, not role assignment result

        _mockUserManager.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>(), command.Password), Times.Once);
        _mockUserManager.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), command.Role), Times.Once);
    }

    [Fact]
    public async Task Handle_WithDuplicateEmail_ShouldReturnFailure()
    {
        // Arrange
        var command = new RegisterUserCommand
        {
            Name = "Duplicate User",
            Email = "existing@example.com",
            Password = "Password123!",
            Role = "User"
        };

        var failureResult = IdentityResult.Failed(new IdentityError
        {
            Code = "DuplicateEmail",
            Description = "User name 'existing@example.com' is already taken."
        });

        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), command.Password))
            .ReturnsAsync(failureResult);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors.First().Code.Should().Be("DuplicateEmail");

        _mockUserManager.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>(), command.Password), Times.Once);
        _mockUserManager.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }

    [Theory]
    [InlineData("Manager")]
    [InlineData("Admin")]
    [InlineData("User")]
    public async Task Handle_WithDifferentRoles_ShouldAssignCorrectRole(string role)
    {
        // Arrange
        var command = new RegisterUserCommand
        {
            Name = "Test User",
            Email = "test@example.com",
            Password = "Password123!",
            Role = role
        };

        var successResult = IdentityResult.Success;

        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), command.Password))
            .ReturnsAsync(successResult);

        _mockUserManager.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), role))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();

        _mockUserManager.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), role), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSetCorrectUserProperties()
    {
        // Arrange
        var command = new RegisterUserCommand
        {
            Name = "John Doe",
            Email = "john.doe@example.com",
            Password = "Password123!",
            Role = "User"
        };

        var successResult = IdentityResult.Success;

        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), command.Password))
            .ReturnsAsync(successResult);

        _mockUserManager.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), command.Role))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockUserManager.Verify(x => x.CreateAsync(It.Is<ApplicationUser>(u =>
            u.Name == "John Doe" &&
            u.Email == "john.doe@example.com" &&
            u.UserName == "john.doe@example.com"), command.Password), Times.Once);
    }
} 