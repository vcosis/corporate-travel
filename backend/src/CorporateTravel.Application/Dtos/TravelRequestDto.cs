using CorporateTravel.Domain.Enums;

namespace CorporateTravel.Application.Dtos;

public class TravelRequestDto
{
    public Guid Id { get; set; }
    public string RequestCode { get; set; } = string.Empty;
    public Guid RequestingUserId { get; set; }
    public string RequestingUserName { get; set; } = string.Empty;
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Guid? ApproverId { get; set; }
    public string? ApproverName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ApprovalDate { get; set; }
} 