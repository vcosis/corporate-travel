using CorporateTravel.Application.Interfaces;
using CorporateTravel.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CorporateTravel.Infrastructure.Services;

public class RequestCodeService : IRequestCodeService
{
    private readonly ApplicationDbContext _context;

    public RequestCodeService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> GenerateRequestCodeAsync()
    {
        // Formato: TR-YYYYMMDD-XXXX (ex: TR-20241225-0001)
        var today = DateTime.UtcNow;
        var datePrefix = today.ToString("yyyyMMdd");
        var baseCode = $"TR-{datePrefix}";

        // Buscar o último código do dia
        var lastCode = await _context.TravelRequests
            .Where(tr => tr.RequestCode.StartsWith(baseCode))
            .OrderByDescending(tr => tr.RequestCode)
            .Select(tr => tr.RequestCode)
            .FirstOrDefaultAsync();

        int sequenceNumber = 1;
        if (!string.IsNullOrEmpty(lastCode))
        {
            // Extrair o número sequencial do último código
            var parts = lastCode.Split('-');
            if (parts.Length == 3 && int.TryParse(parts[2], out int lastNumber))
            {
                sequenceNumber = lastNumber + 1;
            }
        }

        return $"{baseCode}-{sequenceNumber:D4}";
    }
} 