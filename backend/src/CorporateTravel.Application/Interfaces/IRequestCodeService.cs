namespace CorporateTravel.Application.Interfaces;

public interface IRequestCodeService
{
    Task<string> GenerateRequestCodeAsync();
} 