using CorporateTravel.Domain.Enums;

namespace CorporateTravel.Domain.Entities;

public class TravelRequest
{
    public Guid Id { get; set; }
    public string RequestCode { get; set; } = string.Empty;
    public Guid RequestingUserId { get; set; }
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public TravelRequestStatus Status { get; set; }
    public Guid? ApproverId { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual ApplicationUser RequestingUser { get; set; } = null!;
    public virtual ApplicationUser? Approver { get; set; }
} 