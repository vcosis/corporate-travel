using AutoMapper;
using CorporateTravel.Application.Dtos;
using CorporateTravel.Application.Features.TravelRequests.Queries.GetAllTravelRequests;
using CorporateTravel.Application.Interfaces;
using CorporateTravel.Domain.Entities;
using CorporateTravel.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace CorporateTravel.Tests.Features.TravelRequests.Queries.GetAllTravelRequests;

public class GetAllTravelRequestsQueryHandlerTests
{
    private readonly Mock<ITravelRequestRepository> _mockRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly GetAllTravelRequestsQueryHandler _handler;

    public GetAllTravelRequestsQueryHandlerTests()
    {
        _mockRepository = new Mock<ITravelRequestRepository>();
        _mockMapper = new Mock<IMapper>();
        _handler = new GetAllTravelRequestsQueryHandler(_mockRepository.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPaginatedResult()
    {
        // Arrange
        var query = new GetAllTravelRequestsQuery
        {
            UserId = Guid.NewGuid().ToString(),
            UserRoles = new List<string> { "Admin" },
            Page = 1,
            PageSize = 10
        };

        var paginatedResult = new PaginatedResult<TravelRequest>
        {
            Items = new List<TravelRequest>(),
            TotalCount = 0,
            Page = 1,
            PageSize = 10
        };

        _mockRepository.Setup(x => x.GetPaginatedAsync(
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<string?>(),
            It.IsAny<string?>()))
            .ReturnsAsync(paginatedResult);

        _mockMapper.Setup(x => x.Map<IEnumerable<TravelRequestDto>>(It.IsAny<IEnumerable<TravelRequest>>()))
            .Returns(new List<TravelRequestDto>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().NotBeNull();
        result.TotalCount.Should().Be(0);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }
} 