using CorporateTravel.Application.Dtos;
using CorporateTravel.Application.Interfaces;
using CorporateTravel.Domain.Entities;
using CorporateTravel.Domain.Enums;
using CorporateTravel.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CorporateTravel.Infrastructure.Repositories;

public class TravelRequestRepository : ITravelRequestRepository
{
    private readonly ApplicationDbContext _context;

    public TravelRequestRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TravelRequest?> GetByIdAsync(Guid id)
    {
        return await _context.TravelRequests
            .Include(tr => tr.RequestingUser)
            .Include(tr => tr.Approver)
            .FirstOrDefaultAsync(tr => tr.Id == id);
    }

    public async Task<IEnumerable<TravelRequest>> GetAllAsync()
    {
        return await _context.TravelRequests
            .Include(tr => tr.RequestingUser)
            .Include(tr => tr.Approver)
            .OrderByDescending(tr => tr.CreatedAt)
            .ToListAsync();
    }

    public async Task<TravelRequest> AddAsync(TravelRequest travelRequest)
    {
        travelRequest.CreatedAt = DateTime.UtcNow;
        await _context.TravelRequests.AddAsync(travelRequest);
        await _context.SaveChangesAsync();
        return travelRequest;
    }

    public async Task UpdateAsync(TravelRequest travelRequest)
    {
        travelRequest.UpdatedAt = DateTime.UtcNow;
        _context.Entry(travelRequest).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var travelRequest = await _context.TravelRequests.FindAsync(id);
        if (travelRequest != null)
        {
            _context.TravelRequests.Remove(travelRequest);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Dictionary<string, int>> GetStatusCountsAsync(string? userId = null)
    {
        var query = _context.TravelRequests.AsQueryable();

        // Filtrar por usuário se especificado
        if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var userGuid))
        {
            query = query.Where(tr => tr.RequestingUserId == userGuid);
        }

        var counts = await query
            .GroupBy(tr => tr.Status)
            .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
            .ToListAsync();

        var result = new Dictionary<string, int>();
        foreach (var count in counts)
        {
            result[count.Status] = count.Count;
        }

        return result;
    }

    public async Task<PaginatedResult<TravelRequest>> GetPaginatedAsync(int page, int pageSize, string? status = null, string? searchTerm = null, string? userId = null)
    {
        var query = _context.TravelRequests
            .Include(tr => tr.RequestingUser)
            .Include(tr => tr.Approver)
            .AsQueryable();

        // Filtrar por usuário se especificado
        if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var userGuid))
        {
            query = query.Where(tr => tr.RequestingUserId == userGuid);
        }

        // Aplicar filtros
        if (!string.IsNullOrEmpty(status))
        {
            if (Enum.TryParse<TravelRequestStatus>(status, true, out var statusEnum))
            {
                query = query.Where(tr => tr.Status == statusEnum);
            }
        }

        if (!string.IsNullOrEmpty(searchTerm))
        {
            var searchLower = searchTerm.ToLower();
            query = query.Where(tr =>
                tr.Origin.ToLower().Contains(searchLower) ||
                tr.Destination.ToLower().Contains(searchLower) ||
                tr.Reason.ToLower().Contains(searchLower) ||
                (tr.RequestingUser != null && tr.RequestingUser.UserName.ToLower().Contains(searchLower))
            );
        }

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        var items = await query
            .OrderByDescending(tr => tr.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResult<TravelRequest>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
