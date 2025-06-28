using CorporateTravel.Application.Dtos;
using CorporateTravel.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CorporateTravel.Application.Interfaces;

public interface ITravelRequestRepository
{
    Task<TravelRequest?> GetByIdAsync(Guid id);
    Task<IEnumerable<TravelRequest>> GetAllAsync();
    Task<TravelRequest> AddAsync(TravelRequest travelRequest);
    Task UpdateAsync(TravelRequest travelRequest);
    Task DeleteAsync(Guid id);
    Task<Dictionary<string, int>> GetStatusCountsAsync(string? userId = null);
    Task<PaginatedResult<TravelRequest>> GetPaginatedAsync(
        int page, 
        int pageSize, 
        string? status = null, 
        string? searchTerm = null, 
        string? userId = null,
        string? period = null,
        string? requestingUser = null,
        string? approver = null,
        string? sortBy = null,
        string? sortOrder = null,
        string? startDate = null,
        string? endDate = null
    );
    // Other methods will be added here
} 