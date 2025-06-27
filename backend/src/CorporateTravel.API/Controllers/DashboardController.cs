using CorporateTravel.Application.Features.Dashboard.Queries.GetDashboardStats;
using CorporateTravel.Application.Features.TravelRequests.Queries.GetAllTravelRequests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CorporateTravel.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetDashboardStats()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        
        var query = new GetDashboardStatsQuery 
        { 
            UserId = userId,
            UserRoles = userRoles
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("recent-requests")]
    public async Task<IActionResult> GetRecentRequests()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        
        var query = new GetAllTravelRequestsQuery 
        { 
            PageSize = 5,
            UserId = userId,
            UserRoles = userRoles
        };
        var result = await _mediator.Send(query);
        return Ok(result.Items);
    }
} 