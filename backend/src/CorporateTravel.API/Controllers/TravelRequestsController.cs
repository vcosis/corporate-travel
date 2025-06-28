using CorporateTravel.Application.Dtos;
using CorporateTravel.Application.Features.TravelRequests.Commands.CreateTravelRequest;
using CorporateTravel.Application.Features.TravelRequests.Queries.GetAllTravelRequests;
using CorporateTravel.Application.Features.TravelRequests.Queries.GetTravelRequestById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using CorporateTravel.Application.Features.TravelRequests.Commands.ApproveTravelRequest;
using CorporateTravel.Application.Features.TravelRequests.Commands.RejectTravelRequest;
using CorporateTravel.Application.Features.TravelRequests.Commands.UpdateTravelRequest;
using CorporateTravel.Application.Features.TravelRequests.Commands.DeleteTravelRequest;
using CorporateTravel.API.Attributes;
using Microsoft.EntityFrameworkCore;
using CorporateTravel.Infrastructure.Data;
using System.Collections.Generic;
using Serilog;

namespace CorporateTravel.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TravelRequestsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ApplicationDbContext _context;
    private readonly Serilog.ILogger _logger;

    public TravelRequestsController(IMediator mediator, ApplicationDbContext context)
    {
        _mediator = mediator;
        _context = context;
        _logger = Log.ForContext<TravelRequestsController>();
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] GetAllTravelRequestsQuery query)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        
        _logger.Debug("GetAll travel requests - UserId: {UserId}, UserRoles: {UserRoles}", userId, userRoles);
        
        query.UserId = userId;
        query.UserRoles = userRoles;
        
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var query = new GetTravelRequestByIdQuery { Id = id };
            var result = await _mediator.Send(query);
            
            if (result == null)
                return NotFound();
                
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message, details = ex.ToString() });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateTravelRequestDto createTravelRequestDto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            return BadRequest("Invalid user");
        }

        var command = new CreateTravelRequestCommand
        {
            RequestingUserId = userGuid,
            Origin = createTravelRequestDto.Origin,
            Destination = createTravelRequestDto.Destination,
            Reason = createTravelRequestDto.Reason,
            StartDate = createTravelRequestDto.StartDate,
            EndDate = createTravelRequestDto.EndDate
        };

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    [AuthorizeRoles("Admin")]
    public async Task<IActionResult> Update(Guid id, UpdateTravelRequestDto updateTravelRequestDto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            return BadRequest("Invalid user");
        }

        var command = new UpdateTravelRequestCommand
        {
            Id = id,
            RequestingUserId = userGuid,
            UserRoles = userRoles,
            Origin = updateTravelRequestDto.Origin,
            Destination = updateTravelRequestDto.Destination,
            Reason = updateTravelRequestDto.Reason,
            StartDate = updateTravelRequestDto.StartDate,
            EndDate = updateTravelRequestDto.EndDate
        };

        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    [AuthorizeRoles("Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            return BadRequest("Invalid user");
        }

        var command = new DeleteTravelRequestCommand
        {
            Id = id,
            RequestingUserId = userGuid,
            UserRoles = userRoles
        };

        var result = await _mediator.Send(command);
        
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok();
    }

    [HttpPut("{id}/approve")]
    [AuthorizeRoles("Manager", "Admin")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            return BadRequest("Invalid user");
        }

        var command = new ApproveTravelRequestCommand
        {
            Id = id,
            ApproverId = userGuid
        };

        var result = await _mediator.Send(command);
        
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok();
    }

    [HttpPut("{id}/reject")]
    [AuthorizeRoles("Manager", "Admin")]
    public async Task<IActionResult> Reject(Guid id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            return BadRequest("Invalid user");
        }

        var command = new RejectTravelRequestCommand
        {
            Id = id,
            ApproverId = userGuid
        };

        var result = await _mediator.Send(command);
        
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok();
    }

    [HttpGet("{id}/exists")]
    public async Task<IActionResult> Exists(Guid id)
    {
        try
        {
            var exists = await _context.TravelRequests.AnyAsync(tr => tr.Id == id);
            return Ok(new { exists, id });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    public class BatchTravelRequestActionDto {
        public List<Guid> Ids { get; set; } = new();
    }

    [HttpPost("batch-approve")]
    [AuthorizeRoles("Manager", "Admin")]
    public async Task<IActionResult> BatchApprove([FromBody] BatchTravelRequestActionDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            return BadRequest("Invalid user");
        var results = new List<object>();
        foreach (var id in dto.Ids)
        {
            var command = new ApproveTravelRequestCommand { Id = id, ApproverId = userGuid };
            var result = await _mediator.Send(command);
            results.Add(new { id, result });
        }
        return Ok(results);
    }

    [HttpPost("batch-reject")]
    [AuthorizeRoles("Manager", "Admin")]
    public async Task<IActionResult> BatchReject([FromBody] BatchTravelRequestActionDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            return BadRequest("Invalid user");
        var results = new List<object>();
        foreach (var id in dto.Ids)
        {
            var command = new RejectTravelRequestCommand { Id = id, ApproverId = userGuid };
            var result = await _mediator.Send(command);
            results.Add(new { id, result });
        }
        return Ok(results);
    }

    [HttpPost("batch-delete")]
    [AuthorizeRoles("Admin")]
    public async Task<IActionResult> BatchDelete([FromBody] BatchTravelRequestActionDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            return BadRequest("Invalid user");
        var results = new List<object>();
        foreach (var id in dto.Ids)
        {
            var command = new DeleteTravelRequestCommand { Id = id, RequestingUserId = userGuid, UserRoles = userRoles };
            var result = await _mediator.Send(command);
            results.Add(new { id, result });
        }
        return Ok(results);
    }
} 