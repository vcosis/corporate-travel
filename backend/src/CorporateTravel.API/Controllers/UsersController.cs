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

namespace CorporateTravel.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AuthorizeRoles("Admin")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
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
        Console.WriteLine($"=== Delete User API ===");
        Console.WriteLine($"Received ID: '{id}'");
        Console.WriteLine($"ID type: {id?.GetType()}");
        Console.WriteLine($"ID length: {id?.Length}");
        Console.WriteLine($"ID is null or empty: {string.IsNullOrEmpty(id)}");
        
        // Verificar ModelState
        if (!ModelState.IsValid)
        {
            Console.WriteLine("ModelState is invalid");
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            Console.WriteLine($"ModelState errors: {string.Join(", ", errors)}");
            return BadRequest(ModelState);
        }
        
        // Validar se o ID não está vazio
        if (string.IsNullOrEmpty(id))
        {
            Console.WriteLine("ID is null or empty");
            return BadRequest("ID do usuário é obrigatório");
        }
        
        Console.WriteLine($"Processing delete for ID: {id}");
        
        var command = new DeleteUserCommand
        {
            Id = id
        };

        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            Console.WriteLine($"Delete failed. Errors: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            return BadRequest(result.Errors);
        }

        Console.WriteLine("Delete successful");
        return Ok();
    }
} 