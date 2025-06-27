using System.Collections.Generic;

namespace CorporateTravel.Application.Dtos;

public class DashboardStatsDto
{
    public int Pending { get; set; }
    public int Approved { get; set; }
    public int Rejected { get; set; }
    public int Total { get; set; }
    public Dictionary<string, int> StatusCounts { get; set; } = new();
} 