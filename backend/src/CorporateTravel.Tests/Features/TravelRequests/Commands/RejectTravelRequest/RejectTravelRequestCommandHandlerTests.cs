using Xunit;
using CorporateTravel.Application.Dtos;
using CorporateTravel.Application.Features.TravelRequests.Commands.RejectTravelRequest;
using CorporateTravel.Application.Interfaces;
using CorporateTravel.Domain.Entities;
using CorporateTravel.Domain.Enums;
using FluentAssertions;
using Moq;

namespace CorporateTravel.Tests.Features.TravelRequests.Commands.RejectTravelRequest;

public class RejectTravelRequestCommandHandlerTests
{
    private readonly Mock<ITravelRequestRepository> _mockRepository;
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly RejectTravelRequestCommandHandler _handler;

    public RejectTravelRequestCommandHandlerTests()
    {
        _mockRepository = new Mock<ITravelRequestRepository>();
        _mockNotificationService = new Mock<INotificationService>();
        _handler = new RejectTravelRequestCommandHandler(_mockRepository.Object, _mockNotificationService.Object);
    }

    [Fact]
    public async Task Handle_WithValidPendingRequest_ShouldRejectSuccessfully()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var approverId = Guid.NewGuid();
        var requestingUserId = Guid.NewGuid();
        var command = new RejectTravelRequestCommand
        {
            Id = requestId,
            ApproverId = approverId
        };

        var travelRequest = new TravelRequest
        {
            Id = requestId,
            RequestCode = "TR-2024-001",
            Origin = "São Paulo",
            Destination = "Rio de Janeiro",
            StartDate = DateTime.UtcNow.AddDays(7),
            EndDate = DateTime.UtcNow.AddDays(10),
            Reason = "Reunião com cliente",
            Status = TravelRequestStatus.Pending,
            RequestingUserId = requestingUserId,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        _mockRepository.Setup(x => x.GetByIdAsync(requestId))
            .ReturnsAsync(travelRequest);

        _mockRepository.Setup(x => x.UpdateAsync(It.IsAny<TravelRequest>()))
            .Returns(Task.CompletedTask);

        _mockNotificationService.Setup(x => x.CreateNotificationAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<NotificationType>(),
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            It.IsAny<string>()))
            .ReturnsAsync(new NotificationDto());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Message.Should().Be("Travel request rejected successfully");

        _mockRepository.Verify(x => x.GetByIdAsync(requestId), Times.Once);
        _mockRepository.Verify(x => x.UpdateAsync(It.Is<TravelRequest>(tr =>
            tr.Id == requestId &&
            tr.Status == TravelRequestStatus.Rejected &&
            tr.ApproverId == approverId &&
            tr.UpdatedAt >= DateTime.UtcNow.AddMinutes(-1) &&
            tr.UpdatedAt <= DateTime.UtcNow.AddMinutes(1))), Times.Once);

        _mockNotificationService.Verify(x => x.CreateNotificationAsync(
            "Requisição de Viagem Rejeitada",
            $"Sua requisição de viagem para {travelRequest.Destination} foi rejeitada.",
            NotificationType.Warning,
            requestingUserId,
            requestId.ToString(),
            "TravelRequest"), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentRequest_ShouldReturnFailure()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var approverId = Guid.NewGuid();
        var command = new RejectTravelRequestCommand
        {
            Id = requestId,
            ApproverId = approverId
        };

        _mockRepository.Setup(x => x.GetByIdAsync(requestId))
            .ReturnsAsync((TravelRequest?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be("Travel request not found");

        _mockRepository.Verify(x => x.GetByIdAsync(requestId), Times.Once);
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<TravelRequest>()), Times.Never);
        _mockNotificationService.Verify(x => x.CreateNotificationAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<NotificationType>(),
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithAlreadyApprovedRequest_ShouldReturnFailure()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var approverId = Guid.NewGuid();
        var command = new RejectTravelRequestCommand
        {
            Id = requestId,
            ApproverId = approverId
        };

        var travelRequest = new TravelRequest
        {
            Id = requestId,
            RequestCode = "TR-2024-001",
            Origin = "São Paulo",
            Destination = "Rio de Janeiro",
            StartDate = DateTime.UtcNow.AddDays(7),
            EndDate = DateTime.UtcNow.AddDays(10),
            Reason = "Reunião com cliente",
            Status = TravelRequestStatus.Approved,
            RequestingUserId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        _mockRepository.Setup(x => x.GetByIdAsync(requestId))
            .ReturnsAsync(travelRequest);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be("Only pending travel requests can be rejected");

        _mockRepository.Verify(x => x.GetByIdAsync(requestId), Times.Once);
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<TravelRequest>()), Times.Never);
        _mockNotificationService.Verify(x => x.CreateNotificationAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<NotificationType>(),
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithAlreadyRejectedRequest_ShouldReturnFailure()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var approverId = Guid.NewGuid();
        var command = new RejectTravelRequestCommand
        {
            Id = requestId,
            ApproverId = approverId
        };

        var travelRequest = new TravelRequest
        {
            Id = requestId,
            RequestCode = "TR-2024-001",
            Origin = "São Paulo",
            Destination = "Rio de Janeiro",
            StartDate = DateTime.UtcNow.AddDays(7),
            EndDate = DateTime.UtcNow.AddDays(10),
            Reason = "Reunião com cliente",
            Status = TravelRequestStatus.Rejected,
            RequestingUserId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        _mockRepository.Setup(x => x.GetByIdAsync(requestId))
            .ReturnsAsync(travelRequest);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be("Only pending travel requests can be rejected");

        _mockRepository.Verify(x => x.GetByIdAsync(requestId), Times.Once);
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<TravelRequest>()), Times.Never);
        _mockNotificationService.Verify(x => x.CreateNotificationAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<NotificationType>(),
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var approverId = Guid.NewGuid();
        var command = new RejectTravelRequestCommand
        {
            Id = requestId,
            ApproverId = approverId
        };

        _mockRepository.Setup(x => x.GetByIdAsync(requestId))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Database error");
    }

    [Fact]
    public async Task Handle_WhenUpdateThrowsException_ShouldPropagateException()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var approverId = Guid.NewGuid();
        var command = new RejectTravelRequestCommand
        {
            Id = requestId,
            ApproverId = approverId
        };

        var travelRequest = new TravelRequest
        {
            Id = requestId,
            RequestCode = "TR-2024-001",
            Origin = "São Paulo",
            Destination = "Rio de Janeiro",
            StartDate = DateTime.UtcNow.AddDays(7),
            EndDate = DateTime.UtcNow.AddDays(10),
            Reason = "Reunião com cliente",
            Status = TravelRequestStatus.Pending,
            RequestingUserId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        _mockRepository.Setup(x => x.GetByIdAsync(requestId))
            .ReturnsAsync(travelRequest);

        _mockRepository.Setup(x => x.UpdateAsync(It.IsAny<TravelRequest>()))
            .ThrowsAsync(new InvalidOperationException("Update failed"));

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Update failed");
    }

    [Theory]
    [InlineData(TravelRequestStatus.Approved)]
    [InlineData(TravelRequestStatus.Rejected)]
    public async Task Handle_WithNonPendingStatus_ShouldReturnFailure(TravelRequestStatus status)
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var approverId = Guid.NewGuid();
        var command = new RejectTravelRequestCommand
        {
            Id = requestId,
            ApproverId = approverId
        };

        var travelRequest = new TravelRequest
        {
            Id = requestId,
            RequestCode = "TR-2024-001",
            Origin = "São Paulo",
            Destination = "Rio de Janeiro",
            StartDate = DateTime.UtcNow.AddDays(7),
            EndDate = DateTime.UtcNow.AddDays(10),
            Reason = "Reunião com cliente",
            Status = status,
            RequestingUserId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        _mockRepository.Setup(x => x.GetByIdAsync(requestId))
            .ReturnsAsync(travelRequest);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.Message.Should().Be("Only pending travel requests can be rejected");

        _mockRepository.Verify(x => x.GetByIdAsync(requestId), Times.Once);
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<TravelRequest>()), Times.Never);
        _mockNotificationService.Verify(x => x.CreateNotificationAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<NotificationType>(),
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Never);
    }
} 