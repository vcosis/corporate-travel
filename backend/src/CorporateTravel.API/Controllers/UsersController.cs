using CorporateTravel.Application.Dtos;
using CorporateTravel.Application.Features.Users.Queries.GetAllUsers;
using CorporateTravel.Application.Features.Users.Commands.UpdateUser;
using CorporateTravel.Application.Features.Users.Commands.DeleteUser;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using CorporateTravel.API.Attributes;
using System.Threading.Tasks;
using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using Serilog;

namespace CorporateTravel.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AuthorizeRoles("Admin")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger _logger;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
        _logger = Log.ForContext<UsersController>();
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? searchTerm = null, [FromQuery] string? roleFilter = null, [FromQuery] string? statusFilter = null, [FromQuery] string? sortBy = null)
    {
        var query = new GetAllUsersQuery
        {
            Page = page,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            RoleFilter = roleFilter,
            StatusFilter = statusFilter,
            SortBy = sortBy
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, UpdateUserDto updateUserDto)
    {
        if (id != updateUserDto.Id)
        {
            return BadRequest("ID na URL não corresponde ao ID no corpo da requisição");
        }

        var command = new UpdateUserCommand
        {
            Id = updateUserDto.Id,
            Name = updateUserDto.Name,
            Email = updateUserDto.Email,
            Role = updateUserDto.Role
        };

        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        _logger.Information("Delete user request - ID: {UserId}, Type: {IdType}, Length: {IdLength}, IsNullOrEmpty: {IsNullOrEmpty}", 
            id, id?.GetType(), id?.Length, string.IsNullOrEmpty(id));
        
        // Verificar ModelState
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            _logger.Warning("ModelState is invalid - Errors: {Errors}", errors);
            return BadRequest(ModelState);
        }
        
        // Validar se o ID não está vazio
        if (string.IsNullOrEmpty(id))
        {
            _logger.Warning("User ID is null or empty");
            return BadRequest("ID do usuário é obrigatório");
        }
        
        _logger.Debug("Processing delete for user ID: {UserId}", id);
        
        var command = new DeleteUserCommand
        {
            Id = id
        };

        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            _logger.Warning("Delete user failed - ID: {UserId}, Errors: {Errors}", 
                id, string.Join(", ", result.Errors.Select(e => e.Description)));
            return BadRequest(result.Errors);
        }

        _logger.Information("User deleted successfully - ID: {UserId}", id);
        return Ok();
    }
} 