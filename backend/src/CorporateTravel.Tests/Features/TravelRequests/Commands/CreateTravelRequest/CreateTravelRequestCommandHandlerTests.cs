using AutoMapper;
using CorporateTravel.Application.Dtos;
using CorporateTravel.Application.Features.TravelRequests.Commands.CreateTravelRequest;
using CorporateTravel.Application.Interfaces;
using CorporateTravel.Domain.Entities;
using CorporateTravel.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace CorporateTravel.Tests.Features.TravelRequests.Commands.CreateTravelRequest;

public class CreateTravelRequestCommandHandlerTests
{
    private readonly Mock<ITravelRequestRepository> _mockRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly Mock<IRequestCodeService> _mockRequestCodeService;
    private readonly CreateTravelRequestCommandHandler _handler;

    public CreateTravelRequestCommandHandlerTests()
    {
        _mockRepository = new Mock<ITravelRequestRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockNotificationService = new Mock<INotificationService>();
        _mockRequestCodeService = new Mock<IRequestCodeService>();
        _handler = new CreateTravelRequestCommandHandler(
            _mockRepository.Object,
            _mockMapper.Object,
            _mockNotificationService.Object,
            _mockRequestCodeService.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateTravelRequest()
    {
        // Arrange
        var command = new CreateTravelRequestCommand
        {
            Origin = "São Paulo",
            Destination = "Rio de Janeiro",
            StartDate = DateTime.UtcNow.AddDays(7),
            EndDate = DateTime.UtcNow.AddDays(10),
            Reason = "Reunião com cliente",
            RequestingUserId = Guid.NewGuid()
        };

        var requestCode = "TR-2024-001";
        var createdTravelRequest = new TravelRequest
        {
            Id = Guid.NewGuid(),
            RequestCode = requestCode,
            Origin = command.Origin,
            Destination = command.Destination,
            StartDate = command.StartDate,
            EndDate = command.EndDate,
            Reason = command.Reason,
            RequestingUserId = command.RequestingUserId,
            Status = TravelRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        var expectedDto = new TravelRequestDto
        {
            Id = createdTravelRequest.Id,
            RequestCode = requestCode,
            Origin = command.Origin,
            Destination = command.Destination,
            StartDate = command.StartDate,
            EndDate = command.EndDate,
            Reason = command.Reason,
            Status = TravelRequestStatus.Pending.ToString()
        };

        _mockRequestCodeService.Setup(x => x.GenerateRequestCodeAsync())
            .ReturnsAsync(requestCode);

        _mockRepository.Setup(x => x.AddAsync(It.IsAny<TravelRequest>()))
            .ReturnsAsync(createdTravelRequest);

        _mockMapper.Setup(x => x.Map<TravelRequestDto>(createdTravelRequest))
            .Returns(expectedDto);

        _mockNotificationService.Setup(x => x.SendNotificationToManagersAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<NotificationType>(),
            It.IsAny<string>(),
            It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedDto);

        _mockRequestCodeService.Verify(x => x.GenerateRequestCodeAsync(), Times.Once);
        _mockRepository.Verify(x => x.AddAsync(It.Is<TravelRequest>(tr =>
            tr.RequestCode == requestCode &&
            tr.Origin == command.Origin &&
            tr.Destination == command.Destination &&
            tr.StartDate == command.StartDate &&
            tr.EndDate == command.EndDate &&
            tr.Reason == command.Reason &&
            tr.RequestingUserId == command.RequestingUserId &&
            tr.Status == TravelRequestStatus.Pending)), Times.Once);
        _mockMapper.Verify(x => x.Map<TravelRequestDto>(createdTravelRequest), Times.Once);
        _mockNotificationService.Verify(x => x.SendNotificationToManagersAsync(
            "Nova Requisição de Viagem",
            It.Is<string>(msg => msg.Contains(requestCode) && msg.Contains(command.Origin) && msg.Contains(command.Destination)),
            NotificationType.Info,
            createdTravelRequest.Id.ToString(),
            "TravelRequest"), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSetCorrectStatusAndCreatedAt()
    {
        // Arrange
        var command = new CreateTravelRequestCommand
        {
            Origin = "São Paulo",
            Destination = "Rio de Janeiro",
            StartDate = DateTime.UtcNow.AddDays(7),
            EndDate = DateTime.UtcNow.AddDays(10),
            Reason = "Reunião com cliente",
            RequestingUserId = Guid.NewGuid()
        };

        var requestCode = "TR-2024-002";
        var createdTravelRequest = new TravelRequest
        {
            Id = Guid.NewGuid(),
            RequestCode = requestCode,
            Origin = command.Origin,
            Destination = command.Destination,
            StartDate = command.StartDate,
            EndDate = command.EndDate,
            Reason = command.Reason,
            RequestingUserId = command.RequestingUserId,
            Status = TravelRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        var expectedDto = new TravelRequestDto
        {
            Id = createdTravelRequest.Id,
            RequestCode = requestCode,
            Origin = command.Origin,
            Destination = command.Destination,
            StartDate = command.StartDate,
            EndDate = command.EndDate,
            Reason = command.Reason,
            Status = TravelRequestStatus.Pending.ToString()
        };

        _mockRequestCodeService.Setup(x => x.GenerateRequestCodeAsync())
            .ReturnsAsync(requestCode);

        _mockRepository.Setup(x => x.AddAsync(It.IsAny<TravelRequest>()))
            .ReturnsAsync(createdTravelRequest);

        _mockMapper.Setup(x => x.Map<TravelRequestDto>(createdTravelRequest))
            .Returns(expectedDto);

        _mockNotificationService.Setup(x => x.SendNotificationToManagersAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<NotificationType>(),
            It.IsAny<string>(),
            It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockRepository.Verify(x => x.AddAsync(It.Is<TravelRequest>(tr =>
            tr.Status == TravelRequestStatus.Pending &&
            tr.CreatedAt >= DateTime.UtcNow.AddMinutes(-1) &&
            tr.CreatedAt <= DateTime.UtcNow.AddMinutes(1))), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSendNotificationWithCorrectParameters()
    {
        // Arrange
        var command = new CreateTravelRequestCommand
        {
            Origin = "São Paulo",
            Destination = "Rio de Janeiro",
            StartDate = DateTime.UtcNow.AddDays(7),
            EndDate = DateTime.UtcNow.AddDays(10),
            Reason = "Reunião com cliente",
            RequestingUserId = Guid.NewGuid()
        };

        var requestCode = "TR-2024-003";
        var createdTravelRequest = new TravelRequest
        {
            Id = Guid.NewGuid(),
            RequestCode = requestCode,
            Origin = command.Origin,
            Destination = command.Destination,
            StartDate = command.StartDate,
            EndDate = command.EndDate,
            Reason = command.Reason,
            RequestingUserId = command.RequestingUserId,
            Status = TravelRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        var expectedDto = new TravelRequestDto
        {
            Id = createdTravelRequest.Id,
            RequestCode = requestCode,
            Origin = command.Origin,
            Destination = command.Destination,
            StartDate = command.StartDate,
            EndDate = command.EndDate,
            Reason = command.Reason,
            Status = TravelRequestStatus.Pending.ToString()
        };

        _mockRequestCodeService.Setup(x => x.GenerateRequestCodeAsync())
            .ReturnsAsync(requestCode);

        _mockRepository.Setup(x => x.AddAsync(It.IsAny<TravelRequest>()))
            .ReturnsAsync(createdTravelRequest);

        _mockMapper.Setup(x => x.Map<TravelRequestDto>(createdTravelRequest))
            .Returns(expectedDto);

        _mockNotificationService.Setup(x => x.SendNotificationToManagersAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<NotificationType>(),
            It.IsAny<string>(),
            It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockNotificationService.Verify(x => x.SendNotificationToManagersAsync(
            "Nova Requisição de Viagem",
            It.Is<string>(msg => 
                msg.Contains(requestCode) && 
                msg.Contains(command.Origin) && 
                msg.Contains(command.Destination)),
            NotificationType.Info,
            createdTravelRequest.Id.ToString(),
            "TravelRequest"), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        var command = new CreateTravelRequestCommand
        {
            Origin = "São Paulo",
            Destination = "Rio de Janeiro",
            StartDate = DateTime.UtcNow.AddDays(7),
            EndDate = DateTime.UtcNow.AddDays(10),
            Reason = "Reunião com cliente",
            RequestingUserId = Guid.NewGuid()
        };

        var requestCode = "TR-2024-004";

        _mockRequestCodeService.Setup(x => x.GenerateRequestCodeAsync())
            .ReturnsAsync(requestCode);

        _mockRepository.Setup(x => x.AddAsync(It.IsAny<TravelRequest>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Database error");
    }

    [Fact]
    public async Task Handle_WhenNotificationServiceThrowsException_ShouldStillReturnResult()
    {
        // Arrange
        var command = new CreateTravelRequestCommand
        {
            Origin = "São Paulo",
            Destination = "Rio de Janeiro",
            StartDate = DateTime.UtcNow.AddDays(7),
            EndDate = DateTime.UtcNow.AddDays(10),
            Reason = "Reunião com cliente",
            RequestingUserId = Guid.NewGuid()
        };

        var requestCode = "TR-2024-005";
        var createdTravelRequest = new TravelRequest
        {
            Id = Guid.NewGuid(),
            RequestCode = requestCode,
            Origin = command.Origin,
            Destination = command.Destination,
            StartDate = command.StartDate,
            EndDate = command.EndDate,
            Reason = command.Reason,
            RequestingUserId = command.RequestingUserId,
            Status = TravelRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        var expectedDto = new TravelRequestDto
        {
            Id = createdTravelRequest.Id,
            RequestCode = requestCode,
            Origin = command.Origin,
            Destination = command.Destination,
            StartDate = command.StartDate,
            EndDate = command.EndDate,
            Reason = command.Reason,
            Status = TravelRequestStatus.Pending.ToString()
        };

        _mockRequestCodeService.Setup(x => x.GenerateRequestCodeAsync())
            .ReturnsAsync(requestCode);

        _mockRepository.Setup(x => x.AddAsync(It.IsAny<TravelRequest>()))
            .ReturnsAsync(createdTravelRequest);

        _mockMapper.Setup(x => x.Map<TravelRequestDto>(createdTravelRequest))
            .Returns(expectedDto);

        _mockNotificationService.Setup(x => x.SendNotificationToManagersAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<NotificationType>(),
            It.IsAny<string>(),
            It.IsAny<string>()))
            .ThrowsAsync(new Exception("Notification service error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedDto);
    }
} 