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

    public async Task<PaginatedResult<TravelRequest>> GetPaginatedAsync(
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
        string? endDate = null)
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

        // Filtro por período
        if (!string.IsNullOrEmpty(period))
        {
            var cutoffDate = GetCutoffDate(period);
            if (cutoffDate.HasValue)
            {
                query = query.Where(tr => tr.CreatedAt >= cutoffDate.Value);
            }
        }

        // Filtro por solicitante
        if (!string.IsNullOrEmpty(requestingUser))
        {
            query = query.Where(tr => 
                tr.RequestingUser != null && 
                tr.RequestingUser.UserName.ToLower().Contains(requestingUser.ToLower())
            );
        }

        // Filtro por aprovador
        if (!string.IsNullOrEmpty(approver))
        {
            query = query.Where(tr => 
                tr.Approver != null && 
                tr.Approver.UserName.ToLower().Contains(approver.ToLower())
            );
        }

        // Filtro por data de início
        if (!string.IsNullOrEmpty(startDate) && DateTime.TryParse(startDate, out var start))
        {
            query = query.Where(tr => tr.StartDate >= start);
        }

        if (!string.IsNullOrEmpty(endDate) && DateTime.TryParse(endDate, out var end))
        {
            query = query.Where(tr => tr.StartDate <= end);
        }

        // Aplicar ordenação
        query = ApplySorting(query, sortBy, sortOrder);

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        var items = await query
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

    private DateTime? GetCutoffDate(string period)
    {
        var now = DateTime.UtcNow;
        return period?.ToLower() switch
        {
            "7days" => now.AddDays(-7),
            "30days" => now.AddDays(-30),
            "3months" => now.AddMonths(-3),
            "6months" => now.AddMonths(-6),
            "1year" => now.AddYears(-1),
            "all" => null,
            _ => now.AddDays(-30) // padrão: 30 dias
        };
    }

    private IQueryable<TravelRequest> ApplySorting(IQueryable<TravelRequest> query, string? sortBy, string? sortOrder)
    {
        var isDescending = string.IsNullOrEmpty(sortOrder) || sortOrder.ToLower() == "desc";
        
        return sortBy?.ToLower() switch
        {
            "startdate" => isDescending ? query.OrderByDescending(tr => tr.StartDate) : query.OrderBy(tr => tr.StartDate),
            "requestingusername" => isDescending ? query.OrderByDescending(tr => tr.RequestingUser.UserName) : query.OrderBy(tr => tr.RequestingUser.UserName),
            "requestcode" => isDescending ? query.OrderByDescending(tr => tr.RequestCode) : query.OrderBy(tr => tr.RequestCode),
            "approvaldate" => isDescending ? query.OrderByDescending(tr => tr.ApprovalDate) : query.OrderBy(tr => tr.ApprovalDate),
            "approvername" => isDescending ? query.OrderByDescending(tr => tr.Approver.UserName) : query.OrderBy(tr => tr.Approver.UserName),
            _ => isDescending ? query.OrderByDescending(tr => tr.CreatedAt) : query.OrderBy(tr => tr.CreatedAt)
        };
    }
}
