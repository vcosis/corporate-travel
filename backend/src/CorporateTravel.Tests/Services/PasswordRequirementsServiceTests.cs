using Xunit;
using CorporateTravel.Application.Services;
using FluentAssertions;

namespace CorporateTravel.Tests.Services;

public class PasswordRequirementsServiceTests
{
    private readonly PasswordRequirementsService _service;

    public PasswordRequirementsServiceTests()
    {
        _service = new PasswordRequirementsService();
    }

    [Fact]
    public void GetPasswordRequirements_ShouldReturnCorrectRequirements()
    {
        // Act
        var requirements = _service.GetPasswordRequirements();

        // Assert
        requirements.Should().NotBeNull();
        requirements.MinimumLength.Should().Be(8);
        requirements.RequireDigit.Should().BeTrue();
        requirements.RequireLowercase.Should().BeTrue();
        requirements.RequireUppercase.Should().BeTrue();
        requirements.RequireNonAlphanumeric.Should().BeTrue();
        requirements.Description.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData("ValidPass123!", true)]
    [InlineData("StrongP@ss1", true)]
    [InlineData("", false)]
    [InlineData("weak", false)]
    [InlineData("WeakPass", false)]
    [InlineData("weakpass1", false)]
    [InlineData("WEAKPASS1", false)]
    [InlineData("WeakPass1", false)]
    public void ValidatePassword_ShouldValidateCorrectly(string password, bool expectedIsValid)
    {
        // Act
        var isValid = _service.ValidatePassword(password, out var errors);

        // Assert
        isValid.Should().Be(expectedIsValid);
        
        if (!expectedIsValid)
        {
            errors.Should().NotBeEmpty();
        }
        else
        {
            errors.Should().BeEmpty();
        }
    }

    [Fact]
    public void ValidatePassword_WithEmptyPassword_ShouldReturnError()
    {
        // Act
        var isValid = _service.ValidatePassword("", out var errors);

        // Assert
        isValid.Should().BeFalse();
        errors.Should().Contain("A senha é obrigatória.");
    }

    [Fact]
    public void ValidatePassword_WithShortPassword_ShouldReturnError()
    {
        // Act
        var isValid = _service.ValidatePassword("Abc1!", out var errors);

        // Assert
        isValid.Should().BeFalse();
        errors.Should().Contain("A senha deve ter pelo menos 8 caracteres.");
    }

    [Fact]
    public void ValidatePassword_WithoutDigit_ShouldReturnError()
    {
        // Act
        var isValid = _service.ValidatePassword("Abcdefg!", out var errors);

        // Assert
        isValid.Should().BeFalse();
        errors.Should().Contain("A senha deve conter pelo menos um dígito.");
    }

    [Fact]
    public void ValidatePassword_WithoutLowercase_ShouldReturnError()
    {
        // Act
        var isValid = _service.ValidatePassword("ABCDEFG1!", out var errors);

        // Assert
        isValid.Should().BeFalse();
        errors.Should().Contain("A senha deve conter pelo menos uma letra minúscula.");
    }

    [Fact]
    public void ValidatePassword_WithoutUppercase_ShouldReturnError()
    {
        // Act
        var isValid = _service.ValidatePassword("abcdefg1!", out var errors);

        // Assert
        isValid.Should().BeFalse();
        errors.Should().Contain("A senha deve conter pelo menos uma letra maiúscula.");
    }

    [Fact]
    public void ValidatePassword_WithoutSpecialCharacter_ShouldReturnError()
    {
        // Act
        var isValid = _service.ValidatePassword("Abcdefg1", out var errors);

        // Assert
        isValid.Should().BeFalse();
        errors.Should().Contain("A senha deve conter pelo menos um caractere especial.");
    }

    [Fact]
    public void ValidatePassword_WithValidPassword_ShouldReturnNoErrors()
    {
        // Act
        var isValid = _service.ValidatePassword("ValidPass123!", out var errors);

        // Assert
        isValid.Should().BeTrue();
        errors.Should().BeEmpty();
    }
} 